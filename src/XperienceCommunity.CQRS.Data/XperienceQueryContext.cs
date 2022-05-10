using CMS.ContactManagement;
using CMS.SiteProvider;
using XperienceCommunity.PageBuilderUtilities;

namespace XperienceCommunity.CQRS.Data
{
    public interface IQueryContext
    {
        IPageBuilderContext PageBuilderContext { get; }
        ISiteContext SiteContext { get; }
        ICultureContext CultureContext { get; }
        IContactContext ContactContext { get; }
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

    public interface IContactContext
    {
        bool HasContact { get; }
        Maybe<XperienceContact> Contact { get; }
    }

    /// <inheritdoc/>
    public class XperienceQueryContext : IQueryContext
    {
        public XperienceQueryContext(
            ISiteContext siteContext,
            ICultureContext cultureContext,
            IPageBuilderContext pageBuilderContext,
            IContactContext contactContext)
        {
            SiteContext = siteContext;
            CultureContext = cultureContext;
            PageBuilderContext = pageBuilderContext;
            ContactContext = contactContext;
        }

        public ISiteContext SiteContext { get; }
        public ICultureContext CultureContext { get; }
        public IPageBuilderContext PageBuilderContext { get; }
        public IContactContext ContactContext { get; }
    }

    public class XperienceSiteContext : ISiteContext
    {
        public string SiteName => SiteContext.CurrentSiteName ?? "";

        public string SiteDisplayName => SiteContext.CurrentSite?.DisplayName ?? "";

        public int SiteID => SiteContext.CurrentSiteID;
    }

    public class XperienceCultureContext : ICultureContext
    {
        public string CultureCode => Thread.CurrentThread.CurrentCulture.Name;

        public string CultureName => Thread.CurrentThread.CurrentCulture.DisplayName;
    }

    public class XperienceContactContext : IContactContext
    {
        public bool HasContact => ContactManagementContext.CurrentContact is not null;

        public Maybe<XperienceContact> Contact
        {
            get
            {
                var contact = ContactManagementContext.CurrentContact;

                if (contact is null)
                {
                    return Maybe<XperienceContact>.None;
                }

                var personaID = contact.ContactPersonaID ?? Maybe<int>.None;

                var groups = contact.ContactGroups.Select(g => new XperienceContactGroup(g.ContactGroupID, g.ContactGroupName));


                return new XperienceContact(contact.ContactID, contact.ContactOwnerUserID, personaID, groups);
            }
        }
    }

    public record XperienceContact(int ID, int UserID, Maybe<int> PersonaID, IEnumerable<XperienceContactGroup> Groups);
    public record XperienceContactGroup(int ID, string Name);
}