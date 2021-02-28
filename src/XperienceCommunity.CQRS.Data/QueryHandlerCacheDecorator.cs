using System;
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
        private readonly QueryCacheConfiguration config;

        public QueryHandlerCacheDecorator(
            IQueryHandler<TQuery, TResponse> handler,
            IProgressiveCache cache,
            QueryCacheConfiguration config)
        {
            Guard.Against.Null(handler, nameof(handler));
            Guard.Against.Null(cache, nameof(cache));
            Guard.Against.Null(config, nameof(config));

            this.handler = handler;
            this.cache = cache;
            this.config = config;
        }

        public Task<Result<TResponse>> Execute(TQuery query, CancellationToken token = default)
        {
            if (!config.IsEnabled)
            {
                return handler.Execute(query, token);
            }

            if (handler is not IQueryHandlerCacheKeysCreator<TQuery, TResponse> cacheKeysCreator)
            {
                return handler.Execute(query, token);
            }

            var settings = new CacheSettings(
                cacheMinutes: config.CacheItemDuration.Minutes,
                useSlidingExpiration: true,
                cacheItemNameParts: cacheKeysCreator.ItemNameParts(query));

            return cache.LoadAsync(async (cs, t) =>
            {
                var result = await handler.Execute(query, t);

                if (cs.Cached)
                {
                    cs.GetCacheDependency = () => CacheHelper.GetCacheDependency(cacheKeysCreator.DependencyKeys(query, result));
                }

                return result;
            }, settings, token);
        }
    }
}