using XperienceCommunity.CQRS.Core;

namespace XperienceCommunity.Sandbox.Core.Features.Home;

public record HomePageQuery : IQuery<HomePageQueryData>;
public record HomePageQueryData(string Title, Maybe<string> BodyHTML, Maybe<HomePageImageData> Image);
public record HomePageImageData(Guid ImageGuid, string ImagePath);
