# Xperience Community - CQRS

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.CQRS.Core.svg)](https://www.nuget.org/packages/XperienceCommunity.CQRS.Core)

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.CQRS.Data.svg)](https://www.nuget.org/packages/XperienceCommunity.CQRS.Data)

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.CQRS.Web.svg)](https://www.nuget.org/packages/XperienceCommunity.CQRS.Web)

A CQRS implementation influenced by <https://github.com/jbogard/MediatR/> combined with <https://github.com/vkhorikov/CSharpFunctionalExtensions> for Kentico Xperience applications.

## Dependencies

This package is compatible with ASP.NET Core 5+ and is designed to be used with .NET Core / .NET 5 applications integrated with Kentico Xperience 13.0.

## How to Use?

1. This library is separated into (3) NuGet packages. Each package is associated with a different part of the [Onion Architecture](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/).

- `XperienceCommunity.CQRS.Core` - abstractions and implementations for the Domain, Persistence, and Presentation layers to implement.

- `XperienceCommunity.CQRS.Data` - decorators and base classes for data access implementations.

- `XperienceCommunity.CQRS.Web` - presentation layer cache and service collection registration utilities

1. First, install the NuGet package(s) in your ASP.NET Core projects

   ```bash
   dotnet add package XperienceCommunity.CQRS.Web
   ```

1. Create a new implementation of `IQuery<T>`

   ```csharp
   public record HomePageQuery : IQuery<HomePageQueryData>;
   public record HomePageQueryData(string Title, Maybe<string> BodyHTML);
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

                    return new HomePageQueryData(homePage.Fields.Title, bodyHTML);
                });

        protected override void AddDependencyKeys(
            HomePageQuery query,
            HomePageQueryData response,
            ICacheDependencyKeysBuilder builder) =>
            builder.PageType(HomePage.CLASS_NAME);
   }
   ```

1. Register the library's dependencies with the service collection

   ```csharp
   public class Startup
   {
       public void ConfigureServices(IServiceCollection services)
       {
           services.AddCQRS(typeof(HomePageQueryHandler).Assembly);

           // ...
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
               .Match(this, "_HomePage", data => new HomePageViewModel(data));
   }
   ```

## How Does It Work?

- TODO

## References

### Kentico Xperience

- [Kentico 12: Design Patterns Part 12 - Database Query Caching Patterns](https://dev.to/seangwright/kentico-12-design-patterns-part-12-database-query-caching-patterns-43hc)
- [Kentico Xperience Design Patterns: MVC is Dead, Long Live PTVC](https://dev.to/seangwright/kentico-xperience-design-patterns-mvc-is-dead-long-live-ptvc-4635)
