using System.Threading;
using Ardalis.GuardClauses;
using CMS.SiteProvider;
using XperienceCommunity.PageBuilderUtilities;

namespace XperienceCommunity.CQRS.Data
{
    public interface IQueryContext
    {
        IPageBuilderContext PageBuilderContext { get; }
        ISiteContext SiteContext { get; }
        ICultureContext CultureContext { get; }
    }

    /// <summary>
    /// Abstraction for the current request's site
    /// </summary>
    public interface ISiteContext
    {
        /// <summary>
        /// The site's name (code name)
        /// </summary>
        string SiteName { get; }

        /// <summary>
        /// The site's visual name (display name)
        /// </summary>
        string SiteDisplayName { get; }

        /// <summary>
        /// The site's id
        /// </summary>
        int SiteID { get; }
    }

    /// <summary>
    /// Abstraction for the current request's culture
    /// </summary>
    public interface ICultureContext
    {
        /// <summary>
        /// The culture code
        /// </summary>
        /// <example>
        /// es-ES
        /// </example>
        string CultureCode { get; }

        /// <summary>
        /// The culture short name
        /// </summary>
        /// <example>
        /// Spanish
        /// </example>
        string CultureName { get; }
    }

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

    public class XperienceSiteContext : ISiteContext
    {
        public string SiteName => SiteContext.CurrentSiteName;

        public string SiteDisplayName => SiteContext.CurrentSite?.DisplayName ?? "";

        public int SiteID => SiteContext.CurrentSiteID;
    }

    public class XperienceCultureContext : ICultureContext
    {
        public string CultureCode => Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

        public string CultureName => Thread.CurrentThread.CurrentCulture.Name;
    }
}