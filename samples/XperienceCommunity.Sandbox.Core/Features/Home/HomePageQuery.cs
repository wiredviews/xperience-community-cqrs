using XperienceCommunity.CQRS.Core;

namespace XperienceCommunity.Sandbox.Core.Features.Home;

public record HomePageQuery(int DocumentID) : IQuery<HomePageQueryData>, ICacheByValueQuery
{
    public string CacheValueKey => DocumentID.ToString();
}

public record HomePageQueryData(string Title, Maybe<string> BodyHTML, Maybe<HomePageImageData> Image);
public record HomePageImageData(Guid ImageGuid, string ImagePath);
