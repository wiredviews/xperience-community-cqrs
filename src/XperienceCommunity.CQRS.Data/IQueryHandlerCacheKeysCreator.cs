namespace XperienceCommunity.CQRS.Data;

/// <summary>
/// Implementing type can generate cache dependency keys and cache item names for <see cref="IQuery{TResponse}"/>
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IQueryHandlerCacheKeysCreator<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Creates cache dependency keys from the <paramref name="query"/> and <paramref name="response"/>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    string[] DependencyKeys(TQuery query, TResponse response);

    /// <summary>
    /// Creates the name of the cached data from the <paramref name="query"/>
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    object[] ItemNameParts(TQuery query);
}

/// <summary>
/// Implementing type can generate cache dependency keys and cache item names for <see cref="IQuery{TResponse,TError}"/>
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <typeparam name="TError"></typeparam>
public interface IQueryHandlerCacheKeysCreator<TQuery, TResponse, TError> where TQuery : IQuery<TResponse, TError>
{
    /// <summary>
    /// Creates cache dependency keys from the <paramref name="query"/> and <paramref name="response"/>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    string[] DependencyKeys(TQuery query, TResponse response);

    /// <summary>
    /// Creates the name of the cached data from the <paramref name="query"/>
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    object[] ItemNameParts(TQuery query);
}
