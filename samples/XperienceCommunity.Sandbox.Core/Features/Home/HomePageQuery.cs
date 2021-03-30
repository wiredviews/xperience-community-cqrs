using System;
using CSharpFunctionalExtensions;
using XperienceCommunity.CQRS.Core;

namespace XperienceCommunity.Sandbox.Core.Features.Home
{
    public class HomePageQuery : IQuery<HomePageQueryResponse> { }

    public record HomePageQueryResponse(string Title, Maybe<string> BodyHTML, Maybe<HomePageImageResponse> Image) { }

    public record HomePageImageResponse(Guid ImageGuid, string ImagePath) { }
}