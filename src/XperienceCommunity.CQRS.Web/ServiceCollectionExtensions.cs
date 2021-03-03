using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.CQRS.Core;
using XperienceCommunity.CQRS.Data;

namespace XperienceCommunity.CQRS.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCQRS(this IServiceCollection services, params Assembly[] assemblies) =>
            services
                .AddSingleton<IQueryContext, XperienceQueryContext>()
                .AddScoped(s =>
                {
                    return new QueryCacheConfiguration(true, TimeSpan.FromMinutes(5));
                })
                .Scan(s => s
                    .FromAssemblies(assemblies)
                    .AddClasses(c => c.Where(MatchCQRSTypes), true)
                    .AsImplementedInterfaces())
                .Decorate(typeof(IQueryHandler<,>), typeof(QueryHandlerErrorDecorator<,>))
                .Decorate(typeof(IQueryHandler<,>), typeof(QueryHandlerCacheDecorator<,>))
                .AddScoped<CacheDependenciesStore>()
                .AddScoped<ICacheDependenciesStore>(s => s.GetRequiredService<CacheDependenciesStore>())
                .AddScoped<ICacheDependenciesScope>(s => s.GetRequiredService<CacheDependenciesStore>());

        private static bool MatchCQRSTypes(Type t)
        {
            if (!t.IsClass || t.IsAbstract)
            {
                return false;
            }

            string name = t.Name;

            return name.EndsWith("QueryHandler", StringComparison.Ordinal) ||
                name.EndsWith("CommandHandler", StringComparison.Ordinal) ||
                name.EndsWith("Dispatcher", StringComparison.Ordinal);
        }
    }
}
