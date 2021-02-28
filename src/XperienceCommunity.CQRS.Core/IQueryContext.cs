namespace XperienceCommunity.CQRS.Core
{
    public interface IQueryContext
    {
         IPageBuilderContext PageBuilderContext { get; }
         ISiteContext SiteContext { get; }
         ICultureContext CultureContext { get; }
    }

    /// <summary>
    /// Abstraction for the current request's page builder environment
    /// </summary>
    public interface IPageBuilderContext
    {
        /// <summary>
        /// True if either <see cref="IsLivePreviewMode"/> or <see cref="IsEditMode"/> is true. Also the opposite of <see cref="IsLiveMode"/>
        /// </summary>
        bool IsPreviewMode { get; }

        /// <summary>
        /// True if <see cref="IsLivePreviewMode"/> and <see cref="IsEditMode"/> is false. Also the opposite of <see cref="IsPreviewMode"/>
        /// </summary>
        bool IsLiveMode { get; }

        /// <summary>
        /// True if the current request is being made for a preview version of the Page with editing disabled
        /// </summary>
        bool IsLivePreviewMode { get; }

        /// <summary>
        /// True if the current request is being made for the Page Builder experience
        /// </summary>
        bool IsEditMode { get; }

        /// <summary>
        /// The current Mode as a <see cref="PageBuilderMode" /> value
        /// </summary>
        PageBuilderMode Mode { get; }

        /// <summary>
        /// The value of <see cref="Mode" /> as a string
        /// </summary>
        string ModeName();
    }

    /// <summary>
    /// The various states that a request for a Page can be in, in relation to the Page Builder
    /// </summary>
    public enum PageBuilderMode
    {
        Live,
        LivePreview,
        Edit
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
        string Code { get; }

        /// <summary>
        /// The culture short name
        /// </summary>
        /// <example>
        /// Spanish
        /// </example>
        string Name { get; }
    }
}