using Ardalis.GuardClauses;
using XperienceCommunity.CQRS.Core;

namespace XperienceCommunity.CQRS.Data
{
    /// <inheritdoc/>
    public class XperienceQueryContext : IQueryContext
    {
        public XperienceQueryContext(
            ISiteContext siteContext,
            ICultureContext cultureContext,
            IPageBuilderContext pageBuilderContext)
        {
            Guard.Against.Null(siteContext, nameof(siteContext));
            Guard.Against.Null(cultureContext, nameof(cultureContext));
            Guard.Against.Null(pageBuilderContext, nameof(pageBuilderContext));

            SiteContext = siteContext;
            CultureContext = cultureContext;
            PageBuilderContext = pageBuilderContext;
        }

        public ISiteContext SiteContext { get; }
        public ICultureContext CultureContext { get; }
        public IPageBuilderContext PageBuilderContext { get; }
    }
}