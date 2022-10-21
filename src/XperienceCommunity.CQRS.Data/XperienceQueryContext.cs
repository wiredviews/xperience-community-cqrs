using CMS.Base;
using CMS.ContactManagement;
using CMS.SiteProvider;
using XperienceCommunity.PageBuilderUtilities;

namespace XperienceCommunity.CQRS.Data;

public interface IQueryContext
{
    IPageBuilderContext PageBuilderContext { get; }
    ISiteService SiteService { get; }
    ICultureContext CultureContext { get; }
    IContactContext ContactContext { get; }
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
        ISiteService siteService,
        ICultureContext cultureContext,
        IPageBuilderContext pageBuilderContext,
        IContactContext contactContext)
    {
        SiteService = siteService;
        CultureContext = cultureContext;
        PageBuilderContext = pageBuilderContext;
        ContactContext = contactContext;
    }

    public ISiteService SiteService { get; }
    public ICultureContext CultureContext { get; }
    public IPageBuilderContext PageBuilderContext { get; }
    public IContactContext ContactContext { get; }
}

public class XperienceSiteContext : ISiteService
{
    public static string SiteName => SiteContext.CurrentSiteName ?? "";

    public static string SiteDisplayName => SiteContext.CurrentSite?.DisplayName ?? "";

    public static int SiteID => SiteContext.CurrentSiteID;

    public ISiteInfo CurrentSite => throw new NotImplementedException();

    public bool IsLiveSite => throw new NotImplementedException();
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
