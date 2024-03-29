using CMS.Helpers;
using Microsoft.Extensions.Options;

namespace XperienceCommunity.CQRS.Data;

/// <summary>
/// Integrates a caching layer into all <see cref="IQueryHandler{TQuery, TResponse}"/>,
/// returning successful results from the cache and populating the <see cref="ICacheDependenciesStore"/>
/// with the related cache dependency keys
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class QueryHandlerCacheDecorator<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> handler;
    private readonly IProgressiveCache cache;

    /// <summary>
    /// Could be 0 or 1 items in this collection
    /// </summary>
    private readonly IEnumerable<IQueryHandlerCacheKeysCreator<TQuery, TResponse>> creators;
    /// <summary>
    /// Could be 0 or 1 items in this collection
    /// </summary>
    private readonly IEnumerable<IQueryHandlerCacheSettingsCustomizer<TQuery, TResponse>> cacheCustomizers;
    private readonly ICacheDependenciesStore store;
    private readonly QueryCacheConfiguration config;

    public QueryHandlerCacheDecorator(
        IQueryHandler<TQuery, TResponse> handler,
        IProgressiveCache cache,
        IEnumerable<IQueryHandlerCacheKeysCreator<TQuery, TResponse>> creators,
        IEnumerable<IQueryHandlerCacheSettingsCustomizer<TQuery, TResponse>> cacheCustomizers,
        ICacheDependenciesStore store,
        IOptions<QueryCacheConfiguration> config)
    {
        this.handler = handler;
        this.cache = cache;
        this.creators = creators;
        this.cacheCustomizers = cacheCustomizers;
        this.store = store;
        this.config = config.Value;
    }

    public async Task<Result<TResponse>> Execute(TQuery query, CancellationToken token = default)
    {
        var creator = creators.FirstOrDefault();

        if (!config.IsEnabled || creator is null)
        {
            return await handler.Execute(query, token);
        }

        var customizer = cacheCustomizers.FirstOrDefault();

        var settings = customizer?.CustomizeCacheSettings(config, creator, query) ?? new CacheSettings(
            cacheMinutes: config.CacheItemDuration.Minutes,
            useSlidingExpiration: true,
            cacheItemNameParts: creator.ItemNameParts(query));

        var entry = await cache.LoadAsync((cs, t) => GetCachedResult(query, store, creator, cs, t), settings, token);

        if (entry.Result.IsSuccess)
        {
            store.Store(entry.Keys);
        }

        return entry.Result;
    }

    private async Task<CacheEntry<TResponse>> GetCachedResult(
        TQuery query,
        ICacheDependenciesStore store,
        IQueryHandlerCacheKeysCreator<TQuery, TResponse> cacheKeysCreator,
        CacheSettings cs, CancellationToken t)
    {
        var result = await handler.Execute(query, t);

        if (result.IsFailure)
        {
            cs.Cached = false;

            store.MarkCacheDisabled();

            return new CacheEntry<TResponse>(result, Array.Empty<string>());
        }

        string[] keys = cacheKeysCreator.DependencyKeys(query, result.Value);

        if (keys.Length == 0)
        {
            cs.Cached = false;

            store.MarkCacheDisabled();

            return new CacheEntry<TResponse>(result, keys);
        }

        store.Store(keys);

        cs.GetCacheDependency = () => CacheHelper.GetCacheDependency(keys);

        return new CacheEntry<TResponse>(result, keys);
    }
}


/// <summary>
/// Represents a cache entry for a successful <see cref="Result{T}"/>
/// including the <paramref name="Keys"/> generated for the result
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="Result"></param>
/// <param name="Keys"></param>
internal record struct CacheEntry<T>(Result<T> Result, string[] Keys);
