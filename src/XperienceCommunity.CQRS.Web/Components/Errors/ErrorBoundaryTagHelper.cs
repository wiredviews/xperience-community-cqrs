using CMS.Core;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using XperienceCommunity.CQRS.Web.Components.Errors;
using XperienceCommunity.PageBuilderUtilities;

namespace XperienceCommunity.CQRS.Web.Components;

/// <summary>
/// Creates an error boundary for any child contents of the tag.
/// </summary>
/// <remarks>
/// Should wrap <see cref="WidgetZoneTagHelper" /> in Page Builder Section View Components
/// to prevent exceptions throw by Widgets from breaking the rendering of the page.
/// Caching of Widget content is controlled by Xperience's <see cref="CacheConfiguration" />
/// and _ViewComponentDispatcher.cshtml view and uses the ASP.NET Core <see cref="CacheTagHelper"/>
/// which only skips caching when exceptions are thrown.
/// </remarks>
[HtmlTargetElement("error-boundary", TagStructure = TagStructure.NormalOrSelfClosing)]
public class ErrorBoundaryTagHelper : TagHelper
{
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    private readonly IPageBuilderContext pageBuilderContext;
    private readonly IHtmlHelper helper;
    private readonly IEventLogService log;
    private readonly ISettingsService settings;

    public ErrorBoundaryTagHelper(
        IPageBuilderContext pageBuilderContext,
        IHtmlHelper helper,
        IEventLogService log,
        ISettingsService settings)
    {
        this.pageBuilderContext = pageBuilderContext;
        this.helper = helper;
        this.log = log;
        this.settings = settings;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.SuppressOutput();

        try
        {
            var content = await output.GetChildContentAsync();

            _ = output.Content.SetHtmlContent(content);
        }
        catch (Exception ex)
        {
            if (!pageBuilderContext.IsLiveMode)
            {
                var content = await helper.RenderErrorMessage(ViewContext);

                _ = output.Content.SetHtmlContent(content);
            }

            if (!settings.IsErrorBoundaryErrorLoggingEnabled())
            {
                return;
            }

            if (ex is ViewModelResultException resultEx && !string.IsNullOrWhiteSpace(resultEx.ComponentType))
            {
                log.LogException(nameof(ErrorBoundaryTagHelper), "RENDERING_FAILURE", ex, additionalMessage: $"Component Type: [{resultEx.ComponentType}]");
            }
            else
            {
                log.LogException(nameof(ErrorBoundaryTagHelper), "RENDERING_FAILURE", ex);
            }
        }
    }
}
