using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.CQRS.Core;
using XperienceCommunity.CQRS.Data;
using XperienceCommunity.PageBuilderModeTagHelper;

namespace XperienceCommunity.CQRS.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCQRS(this IServiceCollection services, params Assembly[] assemblies) =>
            services
                .AddSingleton<IQueryContext, XperienceQueryContext>()
                .AddSingleton<ISiteContext, XperienceSiteContext>()
                .AddSingleton<ICultureContext, XperienceCultureContext>()
                .AddSingleton<IPageBuilderContext, XperiencePageBuilderContext>()
                .Configure<RazorCacheConfiguration>(c =>
                {
                    c.CacheExpiration = TimeSpan.FromMinutes(1);
                })
                .AddScoped<RazorCacheService>()
                .AddScoped(s =>
                {
                    return new QueryCacheConfiguration(true, TimeSpan.FromMinutes(5));
                })
                .Scan(s => s
                    .FromAssemblies(assemblies)
                    .AddClasses(c => c.Where(MatchCQRSTypes), true)
                    .AsImplementedInterfaces()
                    .WithScopedLifetime())
                .AddScoped<IQueryDispatcher, QueryDispatcher>()
                .Decorate(typeof(IQueryHandler<,>), typeof(QueryHandlerErrorDecorator<,>))
                .Decorate(typeof(IQueryHandler<,>), typeof(QueryHandlerCacheDecorator<,>))
                .AddScoped<CacheDependenciesStore>()
                .AddScoped<ICacheDependenciesStore>(s => s.GetRequiredService<CacheDependenciesStore>())
                .AddScoped<ICacheDependenciesScope>(s => s.GetRequiredService<CacheDependenciesStore>())
                .AddTransient<OperationServiceFactory>(provider =>
                {
                    return t => provider.GetRequiredService(t);
                });

        private static bool MatchCQRSTypes(Type t)
        {
            if (!t.IsClass || t.IsAbstract)
            {
                return false;
            }

            string name = t.Name;

            return name.EndsWith("QueryHandler", StringComparison.Ordinal) ||
                name.EndsWith("CommandHandler", StringComparison.Ordinal);
        }
    }
}
