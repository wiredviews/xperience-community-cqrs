using System.Reflection;
using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.Membership;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Identity;
using XperienceCommunity.CQRS.Web;
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

        services.AddCQRS(assemblies)
            .AddControllersWithViews();

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

        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

        services
            .AddPageTemplateFilters(Assembly.GetExecutingAssembly());

        services.AddScoped<IPasswordHasher<ApplicationUser>, Kentico.Membership.PasswordHasher<ApplicationUser>>();
        services.AddScoped<IMessageService, MessageService>();

        services.AddApplicationIdentity<ApplicationUser, ApplicationRole>(options =>
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
                .AddSignInManager<SignInManager<ApplicationUser>>();

        services.AddAuthorization();
        services.AddAuthentication();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();

        app.UseKentico();

        app.UseCookiePolicy();
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.Kentico().MapRoutes();
        });
    }
}
