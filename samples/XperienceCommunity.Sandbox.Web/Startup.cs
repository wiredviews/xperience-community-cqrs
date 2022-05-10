using System.Reflection;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using XperienceCommunity.CQRS.Web;
using XperienceCommunity.Sandbox.Data.Features.Home;

namespace XperienceCommunity.Sandbox.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly(),
                typeof(HomePageQueryHandler).Assembly
            };

            services.AddCQRS(assemblies);

            services.AddKentico(features =>
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
                .SetAdminCookiesSameSiteNone();

            services
                .AddPageTemplateFilters(Assembly.GetExecutingAssembly());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseKentico();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Kentico().MapRoutes();
            });
        }
    }
}
