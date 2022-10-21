using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using CMS.Helpers.Caching;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using XperienceCommunity.CQRS.Web;

namespace Microsoft.AspNetCore.Mvc.TagHelpers;

// Sourced from : https://github.com/dotnet/aspnetcore/blob/v6.0.4/src/Mvc/Mvc.TagHelpers/src/CacheTagHelper.cs

/// <summary>
/// <see cref="TagHelper"/> implementation targeting &lt;cache&gt; elements.
/// </summary>
public class ScopedCacheTagHelper : CacheTagHelperBase
{
    /// <summary>
    /// Prefix used by <see cref="ScopedCacheTagHelper"/> instances when creating entries in <see cref="MemoryCache"/>.
    /// </summary>
    public static readonly string CacheKeyPrefix = nameof(ScopedCacheTagHelper);
    private readonly ICacheDependenciesScope scope;
    private readonly RazorCacheService cacheService;
    private readonly ICacheDependencyAdapter cacheDependencyAdapter;
    private const string CachePriorityAttributeName = "priority";

    // We need to come up with a value for the size of entries when storing a gating Task on the cache. Any value
    // greater than 0 will suffice. We choose 56 bytes as an approximation of the size of the task that we store
    // in the cache. This size got calculated as an upper bound for the size of an actual task on an x64 architecture
    // and corresponds to 24 bytes for the object header block plus the 40 bytes added by the members of the task
    // object.
    private const int PlaceholderSize = 64;

    /// <summary>
    /// If true, the <see cref="RazorCacheService" /> values will be used to configure the cache settings.
    /// </summary>
    /// <value></value>
    [HtmlAttributeName("use-defaults")]
    public bool UseDefaults { get; set; } = true;

    /// <summary>
    /// Creates a new <see cref="ScopedCacheTagHelper"/>.
    /// </summary>
    /// <param name="factory">The factory containing the private <see cref="IMemoryCache"/> instance
    /// used by the <see cref="ScopedCacheTagHelper"/>.</param>
    /// <param name="htmlEncoder">The <see cref="HtmlEncoder"/> to use.</param>
    /// <param name="scope"></param>
    /// <param name="cacheService"></param>
    /// <param name="cacheDependencyAdapter"></param>
    public ScopedCacheTagHelper(
        CacheTagHelperMemoryCacheFactory factory,
        HtmlEncoder htmlEncoder,
        ICacheDependenciesScope scope,
        RazorCacheService cacheService,
        ICacheDependencyAdapter cacheDependencyAdapter)
        : base(htmlEncoder)
    {
        MemoryCache = factory.Cache;
        this.scope = scope;
        this.cacheService = cacheService;
        this.cacheDependencyAdapter = cacheDependencyAdapter;
    }

    /// <summary>
    /// Gets the <see cref="IMemoryCache"/> instance used to cache entries.
    /// </summary>
    protected IMemoryCache MemoryCache { get; }

    /// <summary>
    /// Gets or sets the <see cref="CacheItemPriority"/> policy for the cache entry.
    /// </summary>
    [HtmlAttributeName(CachePriorityAttributeName)]
    public CacheItemPriority? Priority { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        IHtmlContent content;

        if (UseDefaults)
        {
            Enabled = cacheService.IsEnabled;

            if (!ExpiresSliding.HasValue)
            {
                ExpiresSliding = cacheService.SlidingExpiration;
            }

            if (!ExpiresAfter.HasValue)
            {
                ExpiresAfter = cacheService.AbsoluteExpiration;
            }
        }

        if (Enabled)
        {
            if (UseDefaults && string.IsNullOrWhiteSpace(VaryBy))
            {
                VaryBy = cacheService.VaryByPage();
            }

            var cacheKey = new ScopedCacheTagKey(this, context);
            if (MemoryCache.TryGetValue(cacheKey, out Task<IHtmlContent> cachedResult))
            {
                // There is either some value already cached (as a Task) or a worker processing the output.
                try
                {
                    content = await cachedResult;
                }
                /* Cancelations discard the result, so returning an empty value is fine here and prevents
                 * the exception from bubbling up and spamming the error log
                 */
                catch (TaskCanceledException)
                {
                    content = HtmlString.Empty;
                }
            }
            else
            {
                content = await CreateCacheEntry(cacheKey, output);
            }
        }
        else
        {
            content = await output.GetChildContentAsync();
        }

        // Clear the contents of the "cache" element since we don't want to render it.
        output.SuppressOutput();
        _ = output.Content.SetHtmlContent(content);
    }

    private async Task<IHtmlContent> CreateCacheEntry(ScopedCacheTagKey cacheKey, TagHelperOutput output)
    {
        var tokenSource = new CancellationTokenSource();

        var options = GetMemoryCacheEntryOptions()
            .AddExpirationToken(new CancellationChangeToken(tokenSource.Token))
            .SetSize(PlaceholderSize);

        var tcs = new TaskCompletionSource<IHtmlContent>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);

        // The returned value is ignored, we only do this so that
        // the compiler doesn't complain about the returned task
        // not being awaited
        _ = MemoryCache.Set(cacheKey, tcs.Task, options);

        IHtmlContent content;
        try
        {
            // The entry is set instead of assigning a value to the
            // task so that the expiration options are not impacted
            // by the time it took to compute it.

            // Use the CreateEntry to ensure a cache scope is created that will copy expiration tokens from
            // cache entries created from the GetChildContentAsync call to the current entry.
            var entry = MemoryCache.CreateEntry(cacheKey);

            // The result is processed inside an entry
            // such that the tokens are inherited.

            scope.Begin();

            var result = ProcessContentAsync(output);
            content = await result;

            // If something happened during this scope (ex: db query failure) then we don't want
            // to cache the results but we still need to cleanup this scope/cache entry and return the generated content
            if (!scope.IsCacheEnabled)
            {
                // Remove the worker task from the cache in case it can't complete.
                tokenSource.Cancel();

                // Cancel the TCS so other awaiters see the cancelation.
                _ = tcs.TrySetCanceled();

                // End the scope and discard the cache keys since we don't want to cache this content
                _ = scope.End();

                return content;
            }

            string[] dependencyKeys = scope.End().ToArray();

            if (dependencyKeys.Length > 0)
            {
                // Add Xperience cache dependency keys change token
                _ = options.AddExpirationToken(cacheDependencyAdapter.GetChangeToken(dependencyKeys));
            }

            _ = options.SetSize(GetSize(content));
            _ = entry.SetOptions(options);

            // Only if the Value is set on a cache entry will it be committed to the cache when .Dispose() is called
            entry.Value = result;

            // An entry gets committed to the cache when disposed gets called. We only want to do this when
            // the content has been correctly generated (didn't throw an exception). For that reason the entry
            // can't be put inside a using block.
            entry.Dispose();

            // Set the result on the TCS once we've committed the entry to the cache since commiting to the cache
            // may throw.
            tcs.SetResult(content);
            return content;
        }
        catch (Exception ex)
        {
            // Remove the worker task from the cache in case it can't complete.
            tokenSource.Cancel();

            // Fail the TCS so other awaiters see the exception.
            _ = tcs.TrySetException(ex);

            // End the cache scope since the cache entry was never made
            _ = scope.End();

            throw;
        }
        finally
        {
            // The tokenSource needs to be disposed as the MemoryCache
            // will register a callback on the Token.
            tokenSource.Dispose();
        }
    }

    private long GetSize(IHtmlContent content)
    {
        if (content is CharBufferHtmlContent charBuffer)
        {
            // We need to multiply the size of the buffer
            // by a factor of two due to the fact that
            // characters in .NET are UTF-16 which means
            // every character uses two bytes (surrogates
            // are represented as two characters)
            return charBuffer.Buffer.Length * sizeof(char);
        }

        Debug.Fail($"{nameof(content)} should be an {nameof(CharBufferHtmlContent)}.");
        return -1;
    }

    // Internal for unit testing
    internal MemoryCacheEntryOptions GetMemoryCacheEntryOptions()
    {
        bool hasEvictionCriteria = false;
        var options = new MemoryCacheEntryOptions();
        if (ExpiresOn != null)
        {
            hasEvictionCriteria = true;
            _ = options.SetAbsoluteExpiration(ExpiresOn.Value);
        }

        if (ExpiresAfter != null)
        {
            hasEvictionCriteria = true;
            _ = options.SetAbsoluteExpiration(ExpiresAfter.Value);
        }

        if (ExpiresSliding != null)
        {
            hasEvictionCriteria = true;
            _ = options.SetSlidingExpiration(ExpiresSliding.Value);
        }

        if (Priority != null)
        {
            _ = options.SetPriority(Priority.Value);
        }

        if (!hasEvictionCriteria)
        {
            _ = options.SetSlidingExpiration(DefaultExpiration);
        }

        return options;
    }

    private async Task<IHtmlContent> ProcessContentAsync(TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        using var writer = new CharBufferTextWriter();
        content.WriteTo(writer, HtmlEncoder);
        return new CharBufferHtmlContent(writer.Buffer);
    }

    private class CharBufferTextWriter : TextWriter
    {
        public CharBufferTextWriter() => Buffer = new PagedCharBuffer(CharArrayBufferSource.Instance);

        public override Encoding Encoding => Null.Encoding;

        public PagedCharBuffer Buffer { get; }

        public override void Write(char value) => Buffer.Append(value);

        public override void Write(char[] buffer, int index, int count) => Buffer.Append(buffer, index, count);

        public override void Write(string? value) => Buffer.Append(value);
    }

    private class CharBufferHtmlContent : IHtmlContent
    {
        public CharBufferHtmlContent(PagedCharBuffer buffer) => Buffer = buffer;

        public PagedCharBuffer Buffer { get; }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            int length = Buffer.Length;
            if (length == 0)
            {
                return;
            }

            for (int i = 0; i < Buffer.Pages.Count; i++)
            {
                char[]? page = Buffer.Pages[i];
                int pageLength = Math.Min(length, page.Length);
                writer.Write(page, index: 0, count: pageLength);
                length -= pageLength;
            }

            Debug.Assert(length == 0);
        }
    }
}

internal class PagedCharBuffer : IDisposable
{
    public const int PageSize = 1024;
    private int charIndex;

    public PagedCharBuffer(ICharBufferSource bufferSource) => BufferSource = bufferSource;

    public ICharBufferSource BufferSource { get; }

    // Strongly typed rather than IList for performance
    public List<char[]> Pages { get; } = new List<char[]>();

    public int Length
    {
        get
        {
            int length = charIndex;
            var pages = Pages;
            int fullPages = pages.Count - 1;
            for (int i = 0; i < fullPages; i++)
            {
                length += pages[i].Length;
            }

            return length;
        }
    }

    private char[]? CurrentPage { get; set; }

    public void Append(char value)
    {
        char[]? page = GetCurrentPage();
        page[charIndex++] = value;
    }

    public void Append(string? value)
    {
        if (value == null)
        {
            return;
        }

        int index = 0;
        int count = value.Length;

        while (count > 0)
        {
            char[]? page = GetCurrentPage();
            int copyLength = Math.Min(count, page.Length - charIndex);
            Debug.Assert(copyLength > 0);

            value.CopyTo(
                index,
                page,
                charIndex,
                copyLength);

            charIndex += copyLength;
            index += copyLength;

            count -= copyLength;
        }
    }

    public void Append(char[] buffer, int index, int count)
    {
        while (count > 0)
        {
            char[]? page = GetCurrentPage();
            int copyLength = Math.Min(count, page.Length - charIndex);
            Debug.Assert(copyLength > 0);

            Array.Copy(
                buffer,
                index,
                page,
                charIndex,
                copyLength);

            charIndex += copyLength;
            index += copyLength;
            count -= copyLength;
        }
    }

    /// <summary>
    /// Return all but one of the pages to the <see cref="ICharBufferSource"/>.
    /// This way if someone writes a large chunk of content, we can return those buffers and avoid holding them
    /// for extended durations.
    /// </summary>
    public void Clear()
    {
        var pages = Pages;
        for (int i = pages.Count - 1; i > 0; i--)
        {
            char[]? page = pages[i];

            try
            {
                pages.RemoveAt(i);
            }
            finally
            {
                BufferSource.Return(page);
            }
        }

        charIndex = 0;
        CurrentPage = pages.Count > 0 ? pages[0] : null;
    }

    private char[] GetCurrentPage()
    {
        if (CurrentPage == null || charIndex == CurrentPage.Length)
        {
            CurrentPage = NewPage();
            charIndex = 0;
        }

        return CurrentPage;
    }

    private char[] NewPage()
    {
        char[]? page = null;
        try
        {
            page = BufferSource.Rent(PageSize);
            Pages.Add(page);
        }
        catch when (page != null)
        {
            BufferSource.Return(page);
            throw;
        }

        return page;
    }

    public void Dispose()
    {
        var pages = Pages;
        int count = pages.Count;
        for (int i = 0; i < count; i++)
        {
            BufferSource.Return(pages[i]);
        }

        pages.Clear();
    }
}

internal interface ICharBufferSource
{
    char[] Rent(int bufferSize);

    void Return(char[] buffer);
}

internal class CharArrayBufferSource : ICharBufferSource
{
    public static readonly CharArrayBufferSource Instance = new();

    public char[] Rent(int bufferSize) => new char[bufferSize];

    public void Return(char[] buffer)
    {
        // Do nothing.
    }
}

/// <summary>
/// An instance of <see cref="ScopedCacheTagKey"/> represents the state of <see cref="CacheTagHelper"/>
/// or <see cref="DistributedCacheTagHelper"/> keys.
/// </summary>
public class ScopedCacheTagKey : IEquatable<ScopedCacheTagKey>
{
    private static readonly char[] attributeSeparator = new[] { ',' };
    private static readonly Func<IRequestCookieCollection?, string, string?> cookieAccessor = (c, key) => c?[key];
    private static readonly Func<IHeaderDictionary?, string, string?> headerAccessor = (c, key) => c?[key];
    private static readonly Func<IQueryCollection?, string, string?> queryAccessor = (c, key) => c?[key];
    private static readonly Func<RouteValueDictionary?, string, string?> routeValueAccessor = (c, key) =>
        Convert.ToString(c?[key], CultureInfo.InvariantCulture);

    private const string CacheKeyTokenSeparator = "||";
    private const string VaryByName = "VaryBy";
    private const string VaryByHeaderName = "VaryByHeader";
    private const string VaryByQueryName = "VaryByQuery";
    private const string VaryByRouteName = "VaryByRoute";
    private const string VaryByCookieName = "VaryByCookie";
    private const string VaryByUserName = "VaryByUser";
    private const string VaryByCulture = "VaryByCulture";

    private readonly string? prefix;
    private readonly string? varyBy;
    private readonly DateTimeOffset? expiresOn;
    private readonly TimeSpan? expiresAfter;
    private readonly TimeSpan? expiresSliding;
    private readonly IList<KeyValuePair<string, string>>? headers;
    private readonly IList<KeyValuePair<string, string>>? queries;
    private readonly IList<KeyValuePair<string, string>>? routeValues;
    private readonly IList<KeyValuePair<string, string>>? cookies;
    private readonly bool varyByUser;
    private readonly bool varyByCulture;
    private readonly string? username;
    private readonly CultureInfo? requestCulture;
    private readonly CultureInfo? requestUICulture;

    private string? generatedKey;
    private int? hashcode;

    /// <summary>
    /// Creates an instance of <see cref="ScopedCacheTagKey"/> for a specific <see cref="CacheTagHelper"/>.
    /// </summary>
    /// <param name="tagHelper">The <see cref="CacheTagHelper"/>.</param>
    /// <param name="context">The <see cref="TagHelperContext"/>.</param>
    /// <returns>A new <see cref="ScopedCacheTagKey"/>.</returns>
    public ScopedCacheTagKey(ScopedCacheTagHelper tagHelper, TagHelperContext context)
        : this(tagHelper)
    {
        Key = context.UniqueId;
        prefix = nameof(CacheTagHelper);
    }

    /// <summary>
    /// Creates an instance of <see cref="ScopedCacheTagKey"/> for a specific <see cref="DistributedCacheTagHelper"/>.
    /// </summary>
    /// <param name="tagHelper">The <see cref="DistributedCacheTagHelper"/>.</param>
    /// <returns>A new <see cref="ScopedCacheTagKey"/>.</returns>
    public ScopedCacheTagKey(DistributedCacheTagHelper tagHelper)
        : this((CacheTagHelperBase)tagHelper)
    {
        Key = tagHelper.Name;
        prefix = nameof(DistributedCacheTagHelper);
    }

    private ScopedCacheTagKey(CacheTagHelperBase tagHelper)
    {
        var httpContext = tagHelper.ViewContext.HttpContext;
        var request = httpContext.Request;

        expiresAfter = tagHelper.ExpiresAfter;
        expiresOn = tagHelper.ExpiresOn;
        expiresSliding = tagHelper.ExpiresSliding;
        varyBy = tagHelper.VaryBy;
        cookies = ExtractCollection(tagHelper.VaryByCookie, request.Cookies, cookieAccessor);
        headers = ExtractCollection(tagHelper.VaryByHeader, request.Headers, headerAccessor);
        queries = ExtractCollection(tagHelper.VaryByQuery, request.Query, queryAccessor);
        routeValues = ExtractCollection(
            tagHelper.VaryByRoute,
            tagHelper.ViewContext.RouteData.Values,
            routeValueAccessor);
        varyByUser = tagHelper.VaryByUser;
        varyByCulture = tagHelper.VaryByCulture;

        if (varyByUser)
        {
            username = httpContext.User?.Identity?.Name;
        }

        if (varyByCulture)
        {
            requestCulture = CultureInfo.CurrentCulture;
            requestUICulture = CultureInfo.CurrentUICulture;
        }
    }

    // Internal for unit testing.
    internal string? Key { get; }

    /// <summary>
    /// Creates a <see cref="string"/> representation of the key.
    /// </summary>
    /// <returns>A <see cref="string"/> uniquely representing the key.</returns>
    public string GenerateKey()
    {
        // Caching as the key is immutable and it can be called multiple times during a request.
        if (generatedKey != null)
        {
            return generatedKey;
        }

        var builder = new StringBuilder(prefix);
        _ = builder
            .Append(CacheKeyTokenSeparator)
            .Append(Key);

        if (!string.IsNullOrEmpty(varyBy))
        {
            _ = builder
                .Append(CacheKeyTokenSeparator)
                .Append(VaryByName)
                .Append(CacheKeyTokenSeparator)
                .Append(varyBy);
        }

        AddStringCollection(builder, VaryByCookieName, cookies);
        AddStringCollection(builder, VaryByHeaderName, headers);
        AddStringCollection(builder, VaryByQueryName, queries);
        AddStringCollection(builder, VaryByRouteName, routeValues);

        if (varyByUser)
        {
            _ = builder
                .Append(CacheKeyTokenSeparator)
                .Append(VaryByUserName)
                .Append(CacheKeyTokenSeparator)
                .Append(username);
        }

        if (varyByCulture)
        {
            _ = builder
                .Append(CacheKeyTokenSeparator)
                .Append(VaryByCulture)
                .Append(CacheKeyTokenSeparator)
                .Append(requestCulture)
                .Append(CacheKeyTokenSeparator)
                .Append(requestUICulture);
        }

        generatedKey = builder.ToString();

        return generatedKey;
    }

    /// <summary>
    /// Creates a hashed value of the key.
    /// </summary>
    /// <returns>A cryptographic hash of the key.</returns>
    public string GenerateHashedKey()
    {
        string? key = GenerateKey();

        // The key is typically too long to be useful, so we use a cryptographic hash
        // as the actual key (better randomization and key distribution, so small vary
        // values will generate dramatically different keys).
        byte[]? contentBytes = Encoding.UTF8.GetBytes(key);
        byte[]? hashedBytes = SHA256.HashData(contentBytes);
        return Convert.ToBase64String(hashedBytes);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ScopedCacheTagKey other && Equals(other);

    /// <inheritdoc />
    public bool Equals(ScopedCacheTagKey? other)
    {
        return string.Equals(other?.Key, Key, StringComparison.Ordinal) &&
            other?.expiresAfter == expiresAfter &&
            other?.expiresOn == expiresOn &&
            other?.expiresSliding == expiresSliding &&
            string.Equals(other?.varyBy, varyBy, StringComparison.Ordinal) &&
            AreSame(cookies, other?.cookies) &&
            AreSame(headers, other?.headers) &&
            AreSame(queries, other?.queries) &&
            AreSame(routeValues, other?.routeValues) &&
            varyByUser == other?.varyByUser &&
                (!varyByUser || string.Equals(other?.username, username, StringComparison.Ordinal)) &&
            CultureEquals();

        bool CultureEquals()
        {
            if (varyByCulture != other?.varyByCulture)
            {
                return false;
            }

            if (!varyByCulture)
            {
                // Neither has culture set.
                return true;
            }

            return (requestCulture?.Equals(other.requestCulture) ?? false) &&
                (requestUICulture?.Equals(other.requestUICulture) ?? false);
        }
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // The hashcode is intentionally not using the computed
        // stringified key in order to prevent string allocations
        // in the common case where it's not explicitly required.

        // Caching as the key is immutable and it can be called
        // multiple times during a request.
        if (hashcode.HasValue)
        {
            return hashcode.Value;
        }

        var hashCode = new HashCode();

        hashCode.Add(Key, StringComparer.Ordinal);
        hashCode.Add(expiresAfter);
        hashCode.Add(expiresOn);
        hashCode.Add(expiresSliding);
        hashCode.Add(varyBy, StringComparer.Ordinal);
        hashCode.Add(username, StringComparer.Ordinal);
        hashCode.Add(requestCulture);
        hashCode.Add(requestUICulture);

        CombineCollectionHashCode(ref hashCode, VaryByCookieName, cookies);
        CombineCollectionHashCode(ref hashCode, VaryByHeaderName, headers);
        CombineCollectionHashCode(ref hashCode, VaryByQueryName, queries);
        CombineCollectionHashCode(ref hashCode, VaryByRouteName, routeValues);

        hashcode = hashCode.ToHashCode();

        return hashcode.Value;
    }

    private static IList<KeyValuePair<string, string>>? ExtractCollection<TSourceCollection>(
        string keys,
        TSourceCollection? collection,
        Func<TSourceCollection?, string, string?> accessor)
    {
        if (string.IsNullOrEmpty(keys))
        {
            return null;
        }

        var tokenizer = new StringTokenizer(keys, attributeSeparator);

        var result = new List<KeyValuePair<string, string>>();

        foreach (var item in tokenizer)
        {
            var trimmedValue = item.Trim();

            if (trimmedValue.Length != 0)
            {
                string? value = accessor(collection, trimmedValue.Value);
                result.Add(new KeyValuePair<string, string>(trimmedValue.Value, value ?? string.Empty));
            }
        }

        return result;
    }

    private static void AddStringCollection(
        StringBuilder builder,
        string collectionName,
        IList<KeyValuePair<string, string>>? values)
    {
        if (values == null || values.Count == 0)
        {
            return;
        }

        // keyName(param1=value1|param2=value2)
        _ = builder
            .Append(CacheKeyTokenSeparator)
            .Append(collectionName)
            .Append('(');

        for (int i = 0; i < values.Count; i++)
        {
            var item = values[i];

            if (i > 0)
            {
                _ = builder.Append(CacheKeyTokenSeparator);
            }

            _ = builder
                .Append(item.Key)
                .Append(CacheKeyTokenSeparator)
                .Append(item.Value);
        }

        _ = builder.Append(')');
    }

    private static void CombineCollectionHashCode(
        ref HashCode hashCode,
        string collectionName,
        IList<KeyValuePair<string, string>>? values)
    {
        if (values != null)
        {
            hashCode.Add(collectionName, StringComparer.Ordinal);

            for (int i = 0; i < values.Count; i++)
            {
                var item = values[i];
                hashCode.Add(item.Key);
                hashCode.Add(item.Value);
            }
        }
    }

    private static bool AreSame(IList<KeyValuePair<string, string>>? values1, IList<KeyValuePair<string, string>>? values2)
    {
        if (values1 == values2)
        {
            return true;
        }

        if (values1 == null || values2 == null || values1.Count != values2.Count)
        {
            return false;
        }

        for (int i = 0; i < values1.Count; i++)
        {
            if (!string.Equals(values1[i].Key, values2[i].Key, StringComparison.Ordinal) ||
                !string.Equals(values1[i].Value, values2[i].Value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
