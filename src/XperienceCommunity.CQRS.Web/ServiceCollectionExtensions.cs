using System.Reflection;
using XperienceCommunity.CQRS.Web;
using XperienceCommunity.PageBuilderUtilities;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CQRS types and configuration to DI.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddXperienceCQRS(this IServiceCollection services, IEnumerable<Assembly> assemblies) =>
        services
            .AddSingleton<IQueryContext, XperienceQueryContext>()
            .AddSingleton<ISiteContext, XperienceSiteContext>()
            .AddSingleton<ICultureContext, XperienceCultureContext>()
            .AddSingleton<IPageBuilderContext, XperiencePageBuilderContext>()
            .AddSingleton<IContactContext, XperienceContactContext>()
            .AddOptions<RazorCacheConfiguration>()
            .BindConfiguration("XperienceCommunity:CQRS:Caching:Razor")
            .Services
            .AddScoped<RazorCacheService>()
            .AddOptions<QueryCacheConfiguration>()
            .BindConfiguration("XperienceCommunity:CQRS:Caching:Query")
            .Services
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
