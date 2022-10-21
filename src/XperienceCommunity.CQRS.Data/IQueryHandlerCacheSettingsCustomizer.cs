using CMS.Helpers;

namespace XperienceCommunity.CQRS.Data;

/// <summary>
/// Implementing type can customize cache settings for the <see cref="IQuery{TResponse}" />
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IQueryHandlerCacheSettingsCustomizer<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Creates <see cref="CacheSettings" /> based on the <see cref="CacheSettings.CacheItemNameParts" /> returned by <paramref name="creator" />
    /// the global <paramref name="config" /> and, if needed, the <paramref name="query" />
    /// </summary>
    /// <param name="config"></param>
    /// <param name="creator"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    CacheSettings CustomizeCacheSettings(QueryCacheConfiguration config, IQueryHandlerCacheKeysCreator<TQuery, TResponse> creator, TQuery query);
}

/// <summary>
/// Implementing type can customize cache settings for the <see cref="IQuery{TResponse, TError}" />
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <typeparam name="TError"></typeparam>
public interface IQueryHandlerCacheSettingsCustomizer<TQuery, TResponse, TError>
    where TQuery : IQuery<TResponse, TError>
{
    /// <summary>
    /// Creates <see cref="CacheSettings" /> based on the <see cref="CacheSettings.CacheItemNameParts" /> returned by <paramref name="creator" />
    /// the global <paramref name="config" /> and, if needed, the <paramref name="query" />
    /// </summary>
    /// <param name="config"></param>
    /// <param name="creator"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    CacheSettings CustomizeCacheSettings(QueryCacheConfiguration config, IQueryHandlerCacheKeysCreator<TQuery, TResponse, TError> creator, TQuery query);
}
