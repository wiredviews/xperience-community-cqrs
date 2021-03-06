using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace XperienceCommunity.CQRS.Core
{
    /// <summary>
    /// Service for dispatching <see cref="IQuery{TResponse}"/> and <see cref="IQuery{TResponse, TError}"/> along
    /// with their synchronous counterparts.
    /// </summary>
    public interface IQueryDispatcher
    {
        /// <summary>
        /// Dispatches an asynchronous <see cref="IQuery{TResponse}"/>
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="query"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Result<TResponse>> Dispatch<TResponse>(IQuery<TResponse> query, CancellationToken token = default);

        /// <summary>
        /// Dispatches an asychronous <see cref="IQuery{TResponse, TError}"/>
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TError"></typeparam>
        /// <param name="query"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Result<TResponse, TError>> Dispatch<TResponse, TError>(IQuery<TResponse, TError> query, CancellationToken token = default);
    }

    /// <inheritdoc />
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly OperationServiceFactory serviceFactory;
        private static readonly ConcurrentDictionary<Type, object> queryHandlers = new ConcurrentDictionary<Type, object>();

        public QueryDispatcher(OperationServiceFactory serviceFactory)
        {
            Guard.Against.Null(serviceFactory, nameof(serviceFactory));

            this.serviceFactory = serviceFactory;
        }

        public Task<Result<TResponse>> Dispatch<TResponse>(IQuery<TResponse> query, CancellationToken token = default)
        {
            Guard.Against.Null(query, nameof(query));

            var queryType = query.GetType();

            var handler = (QueryHandlerWrapper<TResponse>)queryHandlers.GetOrAdd(queryType,
                static t => (HandlerBase)(Activator.CreateInstance(typeof(QueryHandlerWrapperImpl<,>).MakeGenericType(t, typeof(TResponse)))
                ?? throw new InvalidOperationException($"Could not create wrapper type for {t}")));

            return handler.Dispatch(query, serviceFactory, token);
        }

        public Task<Result<TResponse, TError>> Dispatch<TResponse, TError>(IQuery<TResponse, TError> query, CancellationToken token = default)
        {
            Guard.Against.Null(query, nameof(query));

            var queryType = query.GetType();

            var handler = (QueryHandlerWrapper<TResponse, TError>)queryHandlers.GetOrAdd(queryType,
                static t => (HandlerBase)(Activator.CreateInstance(typeof(QueryHandlerWrapperImpl<,,>).MakeGenericType(t, typeof(TResponse), typeof(TError)))
                ?? throw new InvalidOperationException($"Could not create wrapper type for {t}")));

            return handler.Dispatch(query, serviceFactory, token);
        }
    }

    internal abstract class QueryHandlerWrapper<TResponse> : HandlerBase
    {
        public abstract Task<Result<TResponse>> Dispatch(IQuery<TResponse> query, OperationServiceFactory serviceFactory, CancellationToken token = default);
    }

    internal class QueryHandlerWrapperImpl<TQuery, TResponse> : QueryHandlerWrapper<TResponse>
        where TQuery : IQuery<TResponse>
    {
        public override Task<Result<TResponse>> Dispatch(IQuery<TResponse> query, OperationServiceFactory serviceFactory, CancellationToken token = default) =>
            GetHandler<IQueryHandler<TQuery, TResponse>>(serviceFactory).Execute((TQuery)query, token);
    }

    internal abstract class QueryHandlerWrapper<TResponse, TError> : HandlerBase
    {
        public abstract Task<Result<TResponse, TError>> Dispatch(IQuery<TResponse, TError> query, OperationServiceFactory serviceFactory, CancellationToken token = default);
    }

    internal class QueryHandlerWrapperImpl<TQuery, TResponse, TError> : QueryHandlerWrapper<TResponse, TError>
        where TQuery : IQuery<TResponse, TError>
    {
        public override Task<Result<TResponse, TError>> Dispatch(IQuery<TResponse, TError> query, OperationServiceFactory serviceFactory, CancellationToken token = default) =>
            GetHandler<IQueryHandler<TQuery, TResponse, TError>>(serviceFactory).Execute((TQuery)query, token);
    }
}