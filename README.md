# Xperience Community - CQRS

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.CQRS.Core.svg)](https://www.nuget.org/packages/XperienceCommunity.CQRS.Core)

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.CQRS.Data.svg)](https://www.nuget.org/packages/XperienceCommunity.CQRS.Data)

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.CQRS.Web.svg)](https://www.nuget.org/packages/XperienceCommunity.CQRS.Web)

A CQRS implementation influenced by <https://github.com/jbogard/MediatR/>
combined with <https://github.com/vkhorikov/CSharpFunctionalExtensions> Maybe and Result monads for Kentico Xperience applications.

## Dependencies

This package is compatible with ASP.NET Core 6+ and is designed to be used with Kentico Xperience 13.0

> Note: This library requires [Kentico Xperience 13: Refresh 1](https://docs.xperience.io/release-notes-xperience-13#ReleasenotesXperience13-Ref1)
> (Hotfix 13.0.16 and later).

## How to Use?

1. This library is separated into (3) NuGet packages.
   Each package is associated with a different part of the [Onion Architecture](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/).
   You can install all of these into your web application project or isolate your Kentico Xperience types behind abstractions.

   - `XperienceCommunity.CQRS.Core`

     - Abstractions and implementations for the Domain, Persistence, and Presentation layers to implement.
     - Typically the project consuming this package doesn't have any reference to Kentico Xperience packages or APIs.

   - `XperienceCommunity.CQRS.Data`

     - Decorators and base classes for data access implementations.
     - The project consuming this package also uses Kentico Xperience data access APIs.

   - `XperienceCommunity.CQRS.Web`
     - Presentation layer cache and service collection registration utilities.
     - The project consuming this package is where your presentation logic is located (Razor/View Components/Controllers).

1. First, install the NuGet package(s) in your ASP.NET Core projects (see the example project under [/samples](https://github.com/wiredviews/xperience-community-cqrs/tree/main/samples))

   ```bash
   dotnet add package XperienceCommunity.CQRS.Core
   dotnet add package XperienceCommunity.CQRS.Data
   dotnet add package XperienceCommunity.CQRS.Web
   ```

1. Create a new implementation of `IQuery<T>`

   ```csharp
   public record HomePageQuery : IQuery<HomePageQueryData>;
   public record HomePageQueryData(int DocumentID, string Title, Maybe<string> BodyHTML);
   ```

1. Create a new implementation of `IQueryHandler<TQuery, TResponse>`

   ```csharp
   public class HomePageQueryHandler : CacheableQueryHandler<HomePageQuery, HomePageQueryData>
   {
       private readonly IPageRetriever retriever;

       public HomePageQueryHandler(IPageRetriever retriever) => this.retriever = retriever;

       public override Task<Result<HomePageQueryData>> Execute(HomePageQuery query, CancellationToken token) =>
           pageRetriever.RetrieveAsync<HomePage>(q => q.TopN(1), cancellationToken: token)
               .TryFirst()
               .ToResult($"Could not find any [{HomePage.CLASS_NAME}]")
               .Map(homePage =>
               {
                   var bodyHTML = string.IsNullOrWhiteSpace(p.Fields.BodyHTML)
                       ? Maybe<string>.None
                       : p.Fields.BodyHTML;

                   return new HomePageQueryData(homePage.DocumentID, homePage.Fields.Title, bodyHTML);
               });

       protected override ICacheDependencyKeysBuilder AddDependencyKeys(
           HomePageQuery query,
           HomePageQueryData response,
           ICacheDependencyKeysBuilder builder) =>
           builder.Page(response.DocumentID);
   }
   ```

   > Note: To be identified by the library, all Query and Command handler classes must be named
   > with the suffix `QueryHandler` or `CommandHandler`.

1. Register the library's dependencies with the service collection

   ```csharp
   public class Startup
   {
       public void ConfigureServices(IServiceCollection services)
       {
           services.AddXperienceCQRS(typeof(HomePageQueryHandler).Assembly);

           // ...
       }
   }
   ```

1. (Optional) Configure cache settings through `appsettings.json`:

   ```json
   "XperienceCommunity": {
       "CQRS": {
           "Caching": {
                "Razor": {
                    "CacheSlidingExpiration": "00:02:00",
                    "CacheAbsoluteExpiration": "00:06:00",
                    "IsEnabled": true
                },
                "Query": {
                    "CacheItemDuration": "00:10:00",
                    "IsEnabled": true
                }
           }
       }
   }
   ```

   > Note: If using `dotnet watch` for local development, it's recommended to disable Razor and Query caching because
   > .NET's hot reloading does not know about the memory caching and changes to code might not be
   > reflected due to cached information.

1. (Optional) Configure cache settings through dependency injection:

   ```csharp
   public class Startup
   {
      public void ConfigureServices(IServiceCollection services)
      {
          services
            .AddXperienceCQRS(typeof(HomePageQueryHandler).Assembly)
            .Configure<RazorCacheConfiguration>(c =>
            {
                c.IsEnabled = false;
            })
            .Configure<QueryCacheConfiguration>(c =>
            {
                c.CacheItemDuration = TimeSpan.FromMinutes(20);
            });
      }
   }
   ```

1. Use queries in your MVC Controllers, application services, or View Components

   ```csharp
   public class HomePageViewComponent : ViewComponent
   {
       private readonly IQueryDispatcher dispatcher;

       public HomePageViewComponent(IQueryDispatcher dispatcher) =>
           this.dispatcher = dispatcher;

       public Task<IViewComponentResult> Invoke() =>
           dispatcher.Dispatch(new HomePageQuery(), HttpContext.RequestAborted)
               .ViewWithFallbackOnFailure(this, "HomePage", data => new HomePageViewModel(data));
   }
   ```

   ```razor
   @using Microsoft.AspNetCore.Html
   @using XperienceCommunity.Sandbox.Web.Features.Home.Components
   @model HomePageViewModel

   <h1>@Model.Title</h1>

   @Model.BodyHTML.GetValueOrDefault(HtmlString.Empty)

   @if (Model.ImagePath.HasValue)
   {
       <img src="@Model.ImagePath.GetValueOrThrow()" alt="@Model.Title" />
   }
   ```

## How Does It Work?

This library's primary goal is to isolate data access into individual operations, with explicit and type-safe
cache item names and dependencies. It models both content and operations with `Maybe` and `Result` monads.

Most Kentico Xperience 13.0 sites focus on data retrieval for the ASP.NET Core application and this library focuses on supporting
robust data access. It also encourages data submission/modification operations
(ex: commerce, external system integrations, user data management) to be separated from data retrieval.

## Contributing

To build this project, you must have v6.0.300 or higher
of the [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed.

## References

### Kentico Xperience

- [Kentico 12: Design Patterns Part 12 - Database Query Caching Patterns](https://dev.to/seangwright/kentico-12-design-patterns-part-12-database-query-caching-patterns-43hc)
- [Kentico Xperience Design Patterns: MVC is Dead, Long Live PTVC](https://dev.to/seangwright/kentico-xperience-design-patterns-mvc-is-dead-long-live-ptvc-4635)
