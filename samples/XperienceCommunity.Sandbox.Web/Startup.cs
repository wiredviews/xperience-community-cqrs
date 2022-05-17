using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using XperienceCommunity.Sandbox.Data.Features.Home;

namespace XperienceCommunity.Sandbox.Web;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
            typeof(HomePageQueryHandler).Assembly,
        };

        _ = services
            .AddControllersWithViews()
            .Services
            .AddKentico(features =>
            {
                features.UsePageBuilder(new PageBuilderOptions
                {
                    RegisterDefaultSection = true
                });
                features.UsePageRouting(new PageRoutingOptions
                {
                    EnableAlternativeUrls = true,
                    EnableRouting = true
                });
            })
            .SetAdminCookiesSameSiteNone()
            .Services
            .AddPageTemplateFilters(Assembly.GetExecutingAssembly())
            .Configure<RouteOptions>(options => options.LowercaseUrls = true)
            .AddAuthentication()
            .Services
            .AddXperienceCQRS(assemblies);
    }

    [SuppressMessage("Usage", "ASP0001:Authorization middleware is incorrectly configured", Justification = "<Pending>")]
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            _ = app.UseDeveloperExceptionPage();
        }

        _ = app
            .UseStaticFiles()
            .UseKentico()
            .UseCookiePolicy()
            .UseCors()
            .UseAuthentication()
            .UseEndpoints(endpoints =>
            {
                endpoints.Kentico().MapRoutes();
            });
    }
}
