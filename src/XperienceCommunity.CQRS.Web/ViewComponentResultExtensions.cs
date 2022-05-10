using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using XperienceCommunity.CQRS.Web.Components;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
/// Represents the failure to build a View Model due to a failed <see cref="Result" />
/// </summary>
public class ViewModelResultException : Exception
{
    public ViewModelResultException(string message) : base(message)
    {
    }

    public ViewModelResultException(string message, string componentType) : base(message) => ComponentType = componentType;

    public string ComponentType { get; } = "";
}

public static class ViewComponentResultExtensions
{
    /// <summary>
    /// Path to the default view that is rendered when Page Builder Widgets and Sections
    /// encounter a failure.
    /// This view can be overridden by creating a file at this path in the consuming application.
    /// </summary>
    /// <remarks>
    /// This view is only ever rendered in the Page Builder Editor or in Page Builder Preview.
    /// </remarks>
    public const string ERROR_VIEW_PATH = "~/Components/_ViewComponentError.cshtml";
    public const string MODEL_STATE_ERROR_KEY = "viewComponentError";

    /// <summary>
    /// Returns a View to be rendered based on the state of the <paramref name="result"/>, using
    /// <paramref name="viewPath"/> if the result is successful and <see cref="ERROR_VIEW_PATH"/> if the result is failed
    /// </summary>
    /// <remarks>
    /// This method is designed to be used with View Components contained in a <see cref="ScopedCacheTagHelper"/>.
    /// The fallback view content will be cached if this is called from a cacheable Page Builder Widget.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="component"></param>
    /// <param name="viewPath"></param>
    /// <returns></returns>
    public static IViewComponentResult ViewWithFallbackOnFailure<T>(this Result<T> result, ViewComponent component, string viewPath)
    {
        if (result.IsSuccess)
        {
            return component.View(viewPath, result.Value);
        }

        component.ModelState.AddModelError(MODEL_STATE_ERROR_KEY, result.Error);

        return component.View(ERROR_VIEW_PATH);
    }


    /// <summary>
    /// Returns a View using the <typeparamref name="T"/> as the model if the result is successful 
    /// and throwing an <see cref="ViewModelResultException" /> if the result is failed.
    /// </summary>
    /// <remarks>
    /// In Page Builder Preview mode this method behaves the same as <see cref="ViewWithFallbackOnFailure{T}(Result{T}, ViewComponent, string)" />.
    /// However, it will throw an exception in Live mode to prevent the result from being cached by a wrapping <see cref="CacheTagHelper" />.
    /// This method is designed to be used with cacheable Page Builder Widgets disabled inside <see cref="WidgetZoneTagHelper"/> wrapped
    /// by <see cref="ErrorBoundaryTagHelper"/>.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="component"></param>
    /// <param name="viewPath"></param>
    /// <param name="componentType"></param>
    /// <returns></returns>
    public static IViewComponentResult ViewWithExceptionOnFailure<T>(this Result<T> result, ViewComponent component, string viewPath, string componentType = "")
    {
        if (result.IsSuccess)
        {
            return component.View(viewPath, result.Value);
        }

        if (component.HttpContext.Kentico().Preview().Enabled)
        {
            component.ModelState.AddModelError(MODEL_STATE_ERROR_KEY, result.Error);

            return component.View(ERROR_VIEW_PATH);
        }

        throw new ViewModelResultException(result.Error, componentType);
    }


    /// <summary>
    /// Returns a View to be rendered based on the state of the <paramref name="result"/>, using
    /// <paramref name="viewPath"/> if the result is successful and <see cref="ERROR_VIEW_PATH"/> if the result is failed.
    /// </summary>
    /// <remarks>
    /// This method is designed to be used with View Components contained in a <see cref="ScopedCacheTagHelper"/>.
    /// The fallback view content will be cached if this is called from a cacheable Page Builder Widget.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="result"></param>
    /// <param name="component"></param>
    /// <param name="viewPath"></param>
    /// <param name="projection">Used to map the <typeparamref name="T"/> value to a <typeparamref name="U"/> value</param>
    /// <returns></returns>
    public static IViewComponentResult ViewWithFallbackOnFailure<T, U>(this Result<T> result, ViewComponent component, string viewPath, Func<T, U> projection)
    {
        if (result.IsSuccess)
        {
            return component.View(viewPath, projection(result.Value));
        }

        component.ModelState.AddModelError(MODEL_STATE_ERROR_KEY, result.Error);

        return component.View(ERROR_VIEW_PATH);
    }


    /// <summary>
    /// Returns a View using the <typeparamref name="T"/> as the model if the result is successful 
    /// and throwing an <see cref="ViewModelResultException" /> if the result is failed.
    /// </summary>
    /// <remarks>
    /// In Page Builder Preview mode this method behaves the same as <see cref="ViewWithFallbackOnFailure{T, U}(Result{T}, ViewComponent, string, Func{T, U})" />.
    /// However, it will throw an exception in Live mode to prevent the result from being cached by a wrapping <see cref="CacheTagHelper" />.
    /// This method is designed to be used with cacheable Page Builder Widgets disabled inside <see cref="WidgetZoneTagHelper"/> wrapped
    /// by <see cref="ErrorBoundaryTagHelper"/>.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="result"></param>
    /// <param name="component"></param>
    /// <param name="viewPath"></param>
    /// <param name="projection">Used to map the <typeparamref name="T"/> value to a <typeparamref name="U"/> value</param>
    /// <param name="componentType"></param>
    /// <returns></returns>
    public static IViewComponentResult ViewWithExceptionOnFailure<T, U>(this Result<T> result, ViewComponent component, string viewPath, Func<T, U> projection, string componentType = "")
    {
        if (result.IsSuccess)
        {
            return component.View(viewPath, projection(result.Value));
        }

        if (component.HttpContext.Kentico().Preview().Enabled)
        {
            component.ModelState.AddModelError(MODEL_STATE_ERROR_KEY, result.Error);

            return component.View(ERROR_VIEW_PATH);
        }

        throw new ViewModelResultException(result.Error, componentType);
    }


    /// <summary>
    /// Returns a View to be rendered based on the state of the <paramref name="resultTask"/>, using
    /// <paramref name="viewPath"/> if the result is successful and <see cref="ERROR_VIEW_PATH"/> if the result is failed
    /// </summary>
    /// <remarks>
    /// This method is designed to be used with View Components contained in a <see cref="ScopedCacheTagHelper"/>.
    /// The fallback view content will be cached if this is called from a cacheable Page Builder Widget.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="resultTask"></param>
    /// <param name="component"></param>
    /// <param name="viewPath"></param>
    /// <returns></returns>
    public static async Task<IViewComponentResult> ViewWithFallbackOnFailure<T>(this Task<Result<T>> resultTask, ViewComponent component, string viewPath)
    {
        var result = await resultTask;

        if (result.IsSuccess)
        {
            return component.View(viewPath, result.Value);
        }

        component.ModelState.AddModelError(MODEL_STATE_ERROR_KEY, result.Error);

        return component.View(ERROR_VIEW_PATH);
    }

    /// <summary>
    /// Returns a View using the <typeparamref name="T"/> as the model if the result is successful 
    /// and throwing an <see cref="ViewModelResultException" /> if the result is failed.
    /// </summary>
    /// <remarks>
    /// In Page Builder Preview mode this method behaves the same as <see cref="ViewWithFallbackOnFailure{T}(Task{Result{T}}, ViewComponent, string)" />.
    /// However, it will throw an exception in Live mode to prevent the result from being cached by a wrapping <see cref="CacheTagHelper" />.
    /// This method is designed to be used with cacheable Page Builder Widgets disabled inside <see cref="WidgetZoneTagHelper"/> wrapped
    /// by <see cref="ErrorBoundaryTagHelper"/>.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="resultTask"></param>
    /// <param name="component"></param>
    /// <param name="viewPath"></param>
    /// <param name="componentType"></param>
    /// <returns></returns>
    public static async Task<IViewComponentResult> ViewWithExceptionOnFailure<T>(this Task<Result<T>> resultTask, ViewComponent component, string viewPath, string componentType = "")
    {
        var result = await resultTask;

        if (result.IsSuccess)
        {
            return component.View(viewPath, result.Value);
        }

        if (component.HttpContext.Kentico().Preview().Enabled)
        {
            component.ModelState.AddModelError(MODEL_STATE_ERROR_KEY, result.Error);

            return component.View(ERROR_VIEW_PATH);
        }

        throw new ViewModelResultException(result.Error, componentType);
    }

    /// <summary>
    /// Returns a View to be rendered based on the state of the <paramref name="resultTask"/>, using
    /// <paramref name="viewPath"/> if the result is successful and <see cref="ERROR_VIEW_PATH"/> if the result is failed
    /// </summary>
    /// <remarks>
    /// This method is designed to be used with View Components contained in a <see cref="ScopedCacheTagHelper"/>.
    /// The fallback view content will be cached if this is called from a cacheable Page Builder Widget.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="resultTask"></param>
    /// <param name="component"></param>
    /// <param name="viewPath"></param>
    /// <param name="projection">Used to map the <typeparamref name="T"/> value to a <typeparamref name="U"/> value</param>
    /// <returns></returns>
    public static async Task<IViewComponentResult> ViewWithFallbackOnFailure<T, U>(this Task<Result<T>> resultTask, ViewComponent component, string viewPath, Func<T, U> projection)
    {
        var result = await resultTask;

        if (result.IsSuccess)
        {
            return component.View(viewPath, projection(result.Value));
        }

        component.ModelState.AddModelError(MODEL_STATE_ERROR_KEY, result.Error);

        return component.View(ERROR_VIEW_PATH);
    }


    /// <summary>
    /// Returns a View using the <typeparamref name="T"/> as the model if the result is successful 
    /// and throwing an <see cref="ViewModelResultException" /> if the result is failed.
    /// </summary>
    /// <remarks>
    /// In Page Builder Preview mode this method behaves the same as <see cref="ViewWithFallbackOnFailure{T, U}(Task{Result{T}}, ViewComponent, string, Func{T, U})" />.
    /// However, it will throw an exception in Live mode to prevent the result from being cached by a wrapping <see cref="CacheTagHelper" />.
    /// This method is designed to be used with cacheable Page Builder Widgets disabled inside <see cref="WidgetZoneTagHelper"/> wrapped
    /// by <see cref="ErrorBoundaryTagHelper"/>.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="resultTask"></param>
    /// <param name="component"></param>
    /// <param name="viewPath"></param>
    /// <param name="projection">Used to map the <typeparamref name="T"/> value to a <typeparamref name="U"/> value</param>
    /// <param name="componentType"></param>
    /// <returns></returns>
    public static async Task<IViewComponentResult> ViewWithExceptionOnFailure<T, U>(this Task<Result<T>> resultTask, ViewComponent component, string viewPath, Func<T, U> projection, string componentType = "")
    {
        var result = await resultTask;

        if (result.IsSuccess)
        {
            return component.View(viewPath, projection(result.Value));
        }

        if (component.HttpContext.Kentico().Preview().Enabled)
        {
            component.ModelState.AddModelError(MODEL_STATE_ERROR_KEY, result.Error);

            return component.View(ERROR_VIEW_PATH);
        }

        throw new ViewModelResultException(result.Error, componentType);
    }
}
