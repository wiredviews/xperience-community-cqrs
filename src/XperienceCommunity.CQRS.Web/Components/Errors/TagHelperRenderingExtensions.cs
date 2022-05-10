using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace XperienceCommunity.CQRS.Web.Components.Errors;

public static class TagHelperRenderingExtensions
{
    /// <summary>
    /// Renders an exception's error message using <see cref="ViewComponentResultExtensions.ERROR_VIEW_PATH" /> as a Partial View
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <remarks>
    /// See: https://stackoverflow.com/a/35298897/939634
    /// </remarks>
    public static async Task<IHtmlContent> RenderErrorMessage(this IHtmlHelper helper, ViewContext context)
    {
        if (helper is not IViewContextAware contextAware)
        {
            return HtmlString.Empty;
        }

        contextAware.Contextualize(context);

        return await helper.PartialAsync(ViewComponentResultExtensions.ERROR_VIEW_PATH);
    }
}
