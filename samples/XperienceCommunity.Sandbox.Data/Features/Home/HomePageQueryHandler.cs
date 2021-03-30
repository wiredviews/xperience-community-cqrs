using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMS.DocumentEngine;
using CSharpFunctionalExtensions;
using FluentCacheKeys;
using Kentico.Content.Web.Mvc;
using XperienceCommunity.CQRS.Core;
using XperienceCommunity.CQRS.Data;
using XperienceCommunity.Sandbox.Core.Features.Home;

namespace XperienceCommunity.Sandbox.Data.Features.Home
{
    public class HomePageQueryHandler : CacheableQueryHandler<HomePageQuery, HomePageQueryResponse>
    {
        private readonly IPageAttachmentUrlRetriever retriever;
        private readonly IAttachmentInfoProvider provider;

        public HomePageQueryHandler(IQueryContext context, IPageAttachmentUrlRetriever retriever, IAttachmentInfoProvider provider) : base(context)
        {
            this.retriever = retriever;
            this.provider = provider;
        }

        public override Task<Result<HomePageQueryResponse>> Execute(HomePageQuery query, CancellationToken token = default) =>
            DocumentHelper.GetDocuments()
                .Types("Sandbox.HomePage")
                .TopN(1)
                .FirstOrNoneAsync(token)
                .Map(async n =>
                {
                    var image = await GetImage(n, token);

                    Maybe<string> bodyHtml = string.IsNullOrWhiteSpace(n.GetStringValue("HomePage", ""))
                        ? Maybe<string>.None
                        : n.GetStringValue("HomePage", "");

                    return new HomePageQueryResponse(n.GetStringValue("HomePageTitle", ""), bodyHtml, image);
                })
                .ToResult("Could not find any [Sandbox.HomePage]");

        private async Task<Maybe<HomePageImageResponse>> GetImage(TreeNode node, CancellationToken token)
        {
            var imageGuid = node.GetGuidValue("HomePageImage", default);

            var attachment = await provider.GetAsync(imageGuid, SiteContext.SiteID, token);

            if (attachment is null)
            {
                return Maybe<HomePageImageResponse>.None;
            }

            var url = retriever.Retrieve(attachment);

            return url is null
                ? Maybe<HomePageImageResponse>.None
                : new HomePageImageResponse(imageGuid, url.RelativePath);
        }

        protected override ICollection<string> QueryDependencyKeys(HomePageQuery query)
        {
            var keys = base.QueryDependencyKeys(query);

            keys.Add(FluentCacheKey.ForPages().OfSite(SiteContext.SiteName).OfClassName("Sandbox.HomePage"));

            return keys;
        }

        protected override ICollection<string> ResultDependencyKeys(HomePageQuery query, HomePageQueryResponse value)
        {
            var keys = base.ResultDependencyKeys(query, value);

            value.Image.Execute(i =>
            {
                keys.Add(FluentCacheKey.ForAttachment().WithGuid(i.ImageGuid));
            });

            return keys;
        }
    }
}