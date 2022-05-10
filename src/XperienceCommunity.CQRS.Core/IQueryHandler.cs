namespace XperienceCommunity.CQRS.Core;

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Execute(TQuery query, CancellationToken token = default);
}

public interface IQueryHandler<TQuery, TResponse, TError>
    where TQuery : IQuery<TResponse, TError>
{
    Task<Result<TResponse, TError>> Execute(TQuery query, CancellationToken token = default);
}
