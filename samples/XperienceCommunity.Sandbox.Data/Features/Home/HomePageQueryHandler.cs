using CMS.DocumentEngine.Types.Sandbox;
using Kentico.Content.Web.Mvc;
using XperienceCommunity.CQRS.Data;
using XperienceCommunity.Sandbox.Core.Features.Home;

namespace XperienceCommunity.Sandbox.Data.Features.Home;

public class HomePageQueryHandler : CacheableQueryHandler<HomePageQuery, HomePageQueryData>
{
    private readonly IPageRetriever pageRetriever;
    private readonly IPageAttachmentUrlRetriever urlRetriever;

    public HomePageQueryHandler(
        IQueryContext context,
        IPageRetriever pageRetriever,
        IPageAttachmentUrlRetriever urlRetriever) : base(context)
    {
        this.pageRetriever = pageRetriever;
        this.urlRetriever = urlRetriever;
    }

    public override Task<Result<HomePageQueryData>> Execute(HomePageQuery query, CancellationToken token = default) =>
        pageRetriever.RetrieveAsync<HomePage>(
                q => q.TopN(1),
                cancellationToken: token)
            .TryFirst()
            .ToResult($"Could not find any [{HomePage.CLASS_NAME}]")
            .Map(p =>
            {
                var image = GetImage(p);

                var descriptionHTML = string.IsNullOrWhiteSpace(p.Fields.DescriptionHTML)
                    ? Maybe<string>.None
                    : p.Fields.DescriptionHTML;

                return new HomePageQueryData(p.Fields.Title, descriptionHTML, image);
            });


    private Maybe<HomePageImageData> GetImage(HomePage page)
    {
        var image = page.Fields.HeroImage;

        if (image is null)
        {
            return Maybe<HomePageImageData>.None;
        }

        var url = urlRetriever.Retrieve(image);

        return url is null
            ? Maybe<HomePageImageData>.None
            : new HomePageImageData(image.AttachmentGUID, url.RelativePath);
    }

    protected override ICacheDependencyKeysBuilder AddDependencyKeys(HomePageQuery query, HomePageQueryData response, ICacheDependencyKeysBuilder builder) =>
        builder
            .SitePageType(HomePage.CLASS_NAME)
            .Attachment(response.Image.Map(i => i.ImageGuid).GetValueOrDefault());
}
