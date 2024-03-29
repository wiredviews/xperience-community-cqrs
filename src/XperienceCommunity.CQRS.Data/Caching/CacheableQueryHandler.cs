using CMS.Base;
using CMS.Helpers;
using XperienceCommunity.PageBuilderUtilities;

namespace XperienceCommunity.CQRS.Data;

/// <summary>
/// Base <see cref="IQueryHandler{TQuery, TResponse}"/> that supports Xperience data caching
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public abstract class CacheableQueryHandler<TQuery, TResponse> :
    IQueryHandlerCacheKeysCreator<TQuery, TResponse>,
    IQueryHandlerCacheSettingsCustomizer<TQuery, TResponse>,
    IQueryHandler<TQuery, TResponse>

    where TQuery : IQuery<TResponse>
{
    public CacheableQueryHandler(IQueryContext context) => Context = context;

    private readonly HashSet<string> customKeys = new(StringComparer.OrdinalIgnoreCase);

    protected IQueryContext Context { get; }
    protected IPageBuilderContext PageBuilderContext => Context.PageBuilderContext;
    protected ISiteService SiteService => Context.SiteService;
    protected ICultureContext CultureContext => Context.CultureContext;

    /// <inheritdoc/>
    public abstract Task<Result<TResponse>> Execute(TQuery query, CancellationToken token = default);

    /// <inheritdoc/>
    public string[] DependencyKeys(TQuery query, TResponse response)
    {
        var builder = new CacheDependencyKeysBuilder(SiteService);
        _ = builder.CustomKeys(customKeys);
        _ = AddDependencyKeys(query, response, builder);

        return builder.GetKeys().ToArray();
    }

    /// <summary>
    /// Used to add custom keys to the generated set of cache Dependency Keys, not based on the <typeparamref name="TQuery"/> or <typeparamref name="TResponse"/>
    /// </summary>
    /// <param name="setCustomKeys"></param>
    protected void SetCustomKeys(Action<ICacheDependencyKeysBuilder> setCustomKeys)
    {
        var builder = new CacheDependencyKeysBuilder(SiteService);

        setCustomKeys(builder);

        customKeys.UnionWith(builder.GetKeys());
    }

    /// <summary>
    /// Overridable to explicitly set the cache dependency keys based on the <typeparamref name="TQuery"/> and <typeparamref name="TResponse"/>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="response"></param>
    /// <param name="builder"></param>
    /// <returns></returns>
    protected virtual ICacheDependencyKeysBuilder AddDependencyKeys(TQuery query, TResponse response, ICacheDependencyKeysBuilder builder) => builder;

    /// <inheritdoc/>
    public virtual object[] ItemNameParts(TQuery query) =>
        query is ICacheByValueQuery cacheByValueQuery
            ? new object[]
                {
                        query.GetType().Name,
                        SiteService.CurrentSite.SiteName,
                        CultureContext.CultureCode,
                        $"is-live:{PageBuilderContext.IsLiveMode}",
                        cacheByValueQuery.CacheValueKey
                }
            : new object[]
                {
                        query.GetType().Name,
                        SiteService.CurrentSite.SiteName,
                        CultureContext.CultureCode,
                        $"is-live:{PageBuilderContext.IsLiveMode}"
                };

    /// <inheritdoc />
    public virtual CacheSettings CustomizeCacheSettings(QueryCacheConfiguration config, IQueryHandlerCacheKeysCreator<TQuery, TResponse> creator, TQuery query)
    {
        var cs = new CacheSettings(
            cacheMinutes: config.CacheItemDuration.Minutes,
            useSlidingExpiration: config.IsSlidingExpiration,
            cacheItemNameParts: creator.ItemNameParts(query));

        return cs;
    }
}
