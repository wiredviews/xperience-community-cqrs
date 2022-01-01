using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using CMS.Helpers;
using CSharpFunctionalExtensions;
using XperienceCommunity.CQRS.Core;

namespace XperienceCommunity.CQRS.Data
{
    public class QueryCacheConfiguration
    {
        public QueryCacheConfiguration(bool isEnabled, TimeSpan cacheItemDuration)
        {
            IsEnabled = isEnabled;
            CacheItemDuration = cacheItemDuration;
        }

        public bool IsEnabled { get; }
        public TimeSpan CacheItemDuration { get; }
    }

    public class QueryHandlerCacheDecorator<TQuery, TResponse> : IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
        private readonly IQueryHandler<TQuery, TResponse> handler;
        private readonly IProgressiveCache cache;
        private readonly IEnumerable<IQueryHandlerCacheKeysCreator<TQuery, TResponse>> creators;
        private readonly ICacheDependenciesStore store;
        private readonly QueryCacheConfiguration config;

        public QueryHandlerCacheDecorator(
            IQueryHandler<TQuery, TResponse> handler,
            IProgressiveCache cache,
            IEnumerable<IQueryHandlerCacheKeysCreator<TQuery, TResponse>> creators,
            ICacheDependenciesStore store,
            QueryCacheConfiguration config)
        {
            Guard.Against.Null(handler, nameof(handler));
            Guard.Against.Null(cache, nameof(cache));
            Guard.Against.Null(creators, nameof(creators));
            Guard.Against.Null(store, nameof(store));
            Guard.Against.Null(config, nameof(config));

            this.handler = handler;
            this.cache = cache;
            this.creators = creators;
            this.store = store;
            this.config = config;
        }

        public Task<Result<TResponse>> Execute(TQuery query, CancellationToken token = default)
        {
            var creator = creators.FirstOrDefault();

            if (!config.IsEnabled || creator is null)
            {
                return handler.Execute(query, token);
            }

            var settings = new CacheSettings(
                cacheMinutes: config.CacheItemDuration.Minutes,
                useSlidingExpiration: true,
                cacheItemNameParts: creator.ItemNameParts(query));

            return cache.LoadAsync(async (cs, t) =>
            {
                var result = await handler.Execute(query, t);

                if (result.IsFailure)
                {
                    cs.Cached = false;
                }

                if (cs.Cached)
                {
                    string[] keys = creator.DependencyKeys(query, result.Value);

                    store.Store(keys);

                    cs.GetCacheDependency = () => CacheHelper.GetCacheDependency(keys);
                }

                return result;
            }, settings, token);
        }
    }
}