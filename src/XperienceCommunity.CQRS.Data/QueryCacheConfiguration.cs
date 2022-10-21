using CMS.Base;
using CMS.Core;

namespace XperienceCommunity.CQRS.Data;

/// <summary>
/// Configuration values used for <see cref="QueryHandlerCacheDecorator{TQuery,TResponse}"/> caching
/// </summary>
public class QueryCacheConfiguration
{
    public bool IsEnabled { get; set; } = true;
    public bool IsSlidingExpiration { get; set; } = true;
    public TimeSpan CacheItemDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Retrieves query caching configuration from Xperience's database.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="siteService"></param>
    /// <returns></returns>
    /// <remarks>
    /// Looks for the site specific "CMSCacheMinutes" setting to set the <see cref="CacheItemDuration"/> and <see cref="IsEnabled"/> values
    /// </remarks>
    public static QueryCacheConfiguration FromXperienceSettings(ISettingsService settings, ISiteService siteService)
    {
        int minutes = settings[$"{siteService.CurrentSite.SiteName}.CMSCacheMinutes"].ToInteger(0);

        bool isEnabled = minutes > 0;

        return new QueryCacheConfiguration
        {
            IsEnabled = isEnabled,
            CacheItemDuration = TimeSpan.FromMinutes(minutes)
        };
    }
}
