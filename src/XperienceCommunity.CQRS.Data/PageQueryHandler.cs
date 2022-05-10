using Kentico.Content.Web.Mvc;

namespace XperienceCommunity.CQRS.Data;

/// <summary>
/// Base class for simplifying querying for a single specific Page
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public abstract class PageQueryHandler<TQuery, TResponse> : CacheableQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    protected readonly IPageRetriever PageRetriever;

    protected PageQueryHandler(IQueryContext context, IPageRetriever retriever) : base(context) =>
        PageRetriever = retriever;
}

/// <summary>
/// Base <see cref="IQueryHandler{TQuery, TResponse}"/> for querying documents by <see cref="TreeNode.NodeAliasPath"/>
/// </summary>
/// <typeparam name="TDocument"></typeparam>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public abstract class PagePathQueryHandler<TDocument, TQuery, TResponse> : PageQueryHandler<TQuery, TResponse>
    where TDocument : TreeNode, new()
    where TQuery : NodeAliasPathQuery<TResponse>
{
    protected PagePathQueryHandler(IQueryContext context, IPageRetriever retriever) : base(context, retriever) { }

    /// <summary>
    /// Retrieves a single page based on <typeparamref name="TQuery"/> with a customizable <see cref="DocumentQuery{TDocument}"/>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="documentQueryModifier">An action which can modify the pre-filtered query</param>
    /// <param name="token"></param>
    /// <returns></returns>
    protected Task<Result<TDocument>> GetPage(TQuery query, Action<DocumentQuery<TDocument>> documentQueryModifier, CancellationToken token = default) =>
        PageRetriever.RetrieveAsync<TDocument>(
            q => documentQueryModifier(q.Path(query.NodeAliasPath, PathTypeEnum.Explicit).TopN(1)), cancellationToken: token)
            .TryFirst()
            .ToResult($"Could not find {typeof(TDocument).Name} page at [{query.NodeAliasPath}]");

    /// <summary>
    /// Retrieves a single page based on <typeparamref name="TQuery"/>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    protected Task<Result<TDocument>> GetPage(TQuery query, CancellationToken token = default) =>
        PageRetriever.RetrieveAsync<TDocument>(
            q => q.Path(query.NodeAliasPath, PathTypeEnum.Explicit), cancellationToken: token)
            .TryFirst()
            .ToResult($"Could not find {typeof(TDocument).Name} page at [{query.NodeAliasPath}]");

    /// <summary>
    /// Sets default cache dependency for queried Page
    /// </summary>
    /// <param name="query"></param>
    /// <param name="response"></param>
    /// <param name="builder"></param>
    protected override ICacheDependencyKeysBuilder AddDependencyKeys(TQuery query, TResponse response, ICacheDependencyKeysBuilder builder) =>
        builder.PagePath(query.NodeAliasPath, PathTypeEnum.Explicit);
}

/// <summary>
/// Base <see cref="IQueryHandler{TQuery, TResponse}"/> for querying documents by <see cref="TreeNode.NodeGUID"/>
/// </summary>
/// <typeparam name="TDocument"></typeparam>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public abstract class PageNodeQueryHandler<TDocument, TQuery, TResponse> : PageQueryHandler<TQuery, TResponse>
    where TDocument : TreeNode, new()
    where TQuery : NodeGuidQuery<TResponse>
{
    protected PageNodeQueryHandler(IQueryContext context, IPageRetriever retriever) : base(context, retriever) { }

    /// <summary>
    /// Retrieves a single page based on <typeparamref name="TQuery"/> with a customizable <see cref="DocumentQuery{TDocument}"/>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="documentQueryModifier">An action which can modify the pre-filtered query</param>
    /// <param name="token"></param>
    /// <returns></returns>
    protected Task<Result<TDocument>> GetPage(TQuery query, Action<DocumentQuery<TDocument>> documentQueryModifier, CancellationToken token = default) =>
        PageRetriever.RetrieveAsync<TDocument>(
            q => documentQueryModifier(q.WhereEquals(nameof(TreeNode.NodeGUID), query.NodeGuid).TopN(1)),
            cancellationToken: token)
            .TryFirst()
            .ToResult($"Could not find {typeof(TDocument).Name} page [{query.NodeGuid}]");

    /// <summary>
    /// Retrieves a single page based on <typeparamref name="TQuery"/>
    /// </summary>
    /// <param name="query"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    protected Task<Result<TDocument>> GetPage(TQuery query, CancellationToken token = default) =>
        PageRetriever.RetrieveAsync<TDocument>(q => q.WhereEquals(nameof(TreeNode.NodeGUID), query.NodeGuid).TopN(1), cancellationToken: token)
            .TryFirst()
            .ToResult($"Could not find {typeof(TDocument).Name} page [{query.NodeGuid}]");

    /// <summary>
    /// Sets default cache dependency for queried Page
    /// </summary>
    /// <param name="query"></param>
    /// <param name="response"></param>
    /// <param name="builder"></param>
    protected override ICacheDependencyKeysBuilder AddDependencyKeys(TQuery query, TResponse response, ICacheDependencyKeysBuilder builder) =>
        builder.Node(query.NodeGuid);
}

