using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Membership;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Identity;
using XperienceCommunity.Sandbox.Core.Features.Home;
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
            typeof(HomePageQuery).Assembly,
        };

        _ = services
            .AddXperienceCQRS()
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
            .AddScoped<IPasswordHasher<ApplicationUser>, Kentico.Membership.PasswordHasher<ApplicationUser>>()
            .AddScoped<IMessageService, MessageService>()
            .AddApplicationIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Note: These settings are effective only when password policies are turned off in the administration settings.
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 0;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
            })
            .AddApplicationDefaultTokenProviders()
            .AddUserStore<ApplicationUserStore<ApplicationUser>>()
            .AddRoleStore<ApplicationRoleStore<ApplicationRole>>()
            .AddUserManager<ApplicationUserManager<ApplicationUser>>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .Services
            .AddAuthorization()
            .AddAuthentication();
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
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.Kentico().MapRoutes();
            });
    }
}
