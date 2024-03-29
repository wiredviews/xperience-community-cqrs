using System.Security.Claims;
using CMS.DocumentEngine;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using XperienceCommunity.PageBuilderUtilities;

namespace XperienceCommunity.CQRS.Web;

public class RazorCacheConfiguration
{
    /// <summary>
    /// Sets the sliding cache expiration for view caching.
    /// </summary>
    /// <remarks>Defaults to 1 minute</remarks>
    public TimeSpan CacheSlidingExpiration { get; set; } = TimeSpan.FromMinutes(1);
    /// <summary>
    /// Sets the absolute cache expiration for view caching.
    /// </summary>
    /// <remarks>Defaults to 3 minutes</remarks>
    /// <value></value>
    public TimeSpan CacheAbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(3);
    /// <summary>
    /// Enables or disables view caching.
    /// </summary>
    /// <remarks>Defaults to <see langword="true"/></remarks>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Provides cache configuration for Razor views (HTML caching).
/// </summary>
/// <remarks>
/// Should be injected into Views using _ViewImports.cshtml
/// </remarks>
public class RazorCacheService
{
    private readonly IPageBuilderContext pageBuilderContext;
    private readonly IContactContext contactContext;
    private readonly ICacheDependenciesScope scope;
    private readonly IPageDataContextRetriever retriever;
    private readonly IHttpContextAccessor accessor;
    private readonly RazorCacheConfiguration config;

    public RazorCacheService(
        IPageBuilderContext pageBuilderContext,
        IContactContext contactContext,
        ICacheDependenciesScope scope,
        IOptions<RazorCacheConfiguration> config,
        IPageDataContextRetriever retriever,
        IHttpContextAccessor accessor)
    {
        this.pageBuilderContext = pageBuilderContext;
        this.contactContext = contactContext;
        this.scope = scope;
        this.retriever = retriever;
        this.accessor = accessor;
        this.config = config.Value;
    }

    /// <summary>
    /// Based on the <see cref="RazorCacheConfiguration.CacheSlidingExpiration" />
    /// </summary>
    public TimeSpan SlidingExpiration => config.CacheSlidingExpiration;
    /// <summary>
    /// Based on <see cref="RazorCacheConfiguration.CacheAbsoluteExpiration" />
    /// </summary>
    public TimeSpan AbsoluteExpiration => config.CacheAbsoluteExpiration;

    /// <summary>
    /// Razor view caching is only enabled if the Page Builder is in "Live" mode (not Preview or Edit),
    /// no query caching has failed, and the <see cref="RazorCacheConfiguration.IsEnabled" /> is true 
    /// </summary>
    public bool IsEnabled =>
        pageBuilderContext.IsLiveMode &&
        scope.IsCacheEnabled &&
        config.IsEnabled;

    /// <summary>
    /// Cached content is dependent on the current Page (Document).
    /// Falls back to the Request Path if no Page Data is available.
    /// </summary>
    /// <returns></returns>
    public string VaryByPage() =>
        retriever.TryRetrieve<TreeNode>(out var data)
            ? $"DocID={data.Page.DocumentID}"
            : $"Path={accessor.HttpContext?.Request.Path}";

    /// <summary>
    /// Cached content is dependent on the current Contact's Persona combined with <see cref="VaryByPage"/>.
    /// Falls back to <see cref="VaryByPage"/>.
    /// </summary>
    /// <returns></returns>
    public string VaryByPersona() =>
        contactContext.Contact
            .Bind(c => c.PersonaID)
            .Map(personaID => $"PersonaID={personaID}|{VaryByPage()}")
            .GetValueOrDefault(() => VaryByPage());

    /// <summary>
    /// Cached content is dependent on <see cref="VaryByPage" /> and whether or not the current user is authenticated.
    /// </summary>
    /// <returns></returns>
    public string VaryByAuthenticated()
    {
        var principal = accessor.HttpContext?.User;

        return principal is null || principal.Identity is not ClaimsIdentity identity
            ? $"{VaryByPage()}|AuthN:False"
            : $"{VaryByPage()}|AuthN:{identity.IsAuthenticated}";
    }

    /// <summary>
    /// Cached content is dependent on <see cref="VaryByPage" /> and the <see cref="CMS.Membership.UserInfo.UserID" /> of the current user.
    /// </summary>
    /// <returns></returns>
    public string VaryByUser()
    {
        var principal = accessor.HttpContext?.User;

        if (principal is null || principal.Identity is not ClaimsIdentity identity)
        {
            return $"{VaryByPage()}|User:anonymous";
        }

        string id = identity.Claims
            .Where(c => c.Type == ClaimTypes.NameIdentifier)
            .Select(c => c.Value)
            .FirstOrDefault() ?? identity.Name ?? "anonymous";

        return $"{VaryByPage()}|User:{id}";
    }


    /// <summary>
    /// Begins a cache scope, capturing all successive cache dependencies generated by operations.
    /// </summary>
    /// <remarks>Should be followed by a call to <see cref="EndScope"/></remarks>
    /// <returns></returns>
    public HtmlString BeginScope()
    {
        scope.Begin();

        return HtmlString.Empty;
    }

    /// <summary>
    /// Ends a cache scope, returning all captured cache dependencies
    /// </summary>
    /// <remarks>Should be preceeded by a call to <see cref="BeginScope"/></remarks>
    /// <returns></returns>
    public string[] EndScope() => scope.End().ToArray();
}
