using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using XperienceCommunity.CQRS.Core;

namespace XperienceCommunity.CQRS.Data
{
    /// <summary>
    /// Base <see cref="IQueryHandler{TQuery, TResponse}"/> that supports Xperience data caching
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public abstract class CacheableQueryHandler<TQuery, TResponse> :
        IQueryHandlerCacheKeysCreator<TQuery, TResponse>,
        IQueryHandler<TQuery, TResponse>

        where TQuery : IQuery<TResponse>
    {
        public CacheableQueryHandler(IQueryContext context)
        {
            Guard.Against.Null(context, nameof(context));

            Context = context;
        }

        private readonly HashSet<string> dependencyKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        protected IQueryContext Context { get; }
        protected IPageBuilderContext PageBuilderContext => Context.PageBuilderContext;
        protected ISiteContext SiteContext => Context.SiteContext;
        protected ICultureContext CultureContext => Context.CultureContext;

        /// <inheritdoc/>
        public abstract Task<Result<TResponse>> Execute(TQuery query, CancellationToken token = default);

        /// <inheritdoc/>
        public string[] DependencyKeys(TQuery query, Result<TResponse> result)
        {
            foreach (string key in QueryDependencyKeys(query))
            {
                dependencyKeys.Add(key);
            }

            if (result.IsFailure)
            {
                return dependencyKeys.ToArray();
            }

            foreach (string key in ResultDependencyKeys(query, result.Value))
            {
                dependencyKeys.Add(key);
            }

            return dependencyKeys.ToArray();
        }

        /// <summary>
        /// Used to add custom keys to the generated set of cache Dependency Keys, not based on the <typeparamref name="TQuery"/> or <typeparamref name="TResponse"/>
        /// </summary>
        /// <param name="setCustomKeys"></param>
        protected void SetCustomKeys(Action<ICollection<string>> setCustomKeys)
        {
            Guard.Against.Null(setCustomKeys, nameof(setCustomKeys));

            setCustomKeys(dependencyKeys);
        }

        /// <summary>
        /// Overridable to explicitly set the cache dependency keys based on the <typeparamref name="TQuery"/>
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual ICollection<string> QueryDependencyKeys(TQuery query) =>
            dependencyKeys;

        /// <summary>
        /// Overridable to explicitly set the cache dependency keys based on the <typeparamref name="TQuery"/> and <typeparamref name="TResponse"/>
        /// </summary>
        /// <param name="query"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual ICollection<string> ResultDependencyKeys(TQuery query, TResponse value) =>
            dependencyKeys;

        /// <inheritdoc/>
        public virtual object[] ItemNameParts(TQuery query) =>
            query is ICacheByValueQuery cacheByValueQuery
                ? new object[]
                    {
                        query.GetType().Name,
                        SiteContext.SiteName,
                        CultureContext.Code,
                        $"is-live:{PageBuilderContext.IsLiveMode}",
                        cacheByValueQuery.CacheValueKey
                    }
                : new object[]
                    {
                        query.GetType().Name,
                        SiteContext.SiteName,
                        CultureContext.Code,
                        $"is-live:{PageBuilderContext.IsLiveMode}"
                    };
    }
}