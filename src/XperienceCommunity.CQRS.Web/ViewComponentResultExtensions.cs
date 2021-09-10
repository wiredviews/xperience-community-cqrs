using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Microsoft.AspNetCore.Mvc
{
    public static class ViewComponentResultExtensions
    {
        /// <summary>
        /// Path to the default view that is rendered when Page Builder Widgets and Sections
        /// encounter a failure.
        /// This view can be overridden by creating a file at this path in the consuming application.
        /// </summary>
        public const string ERROR_VIEW_PATH = "~/Components/_ViewComponentError.cshtml";

        /// <summary>
        /// Returns a View to be rendered based on the state of the <paramref name="result"/>, using
        /// <paramref name="viewPath"/> if the result is successful and <see cref="ERROR_VIEW_PATH"/> if the result is failed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="component"></param>
        /// <param name="viewPath"></param>
        /// <returns></returns>
        public static IViewComponentResult Match<T>(this Result<T> result, ViewComponent component, string viewPath) =>
            result.Match(
                v => component.View(viewPath, v),
                error => 
                {
                    component.ModelState.AddModelError("componentError", result.Error);

                    return component.View(ERROR_VIEW_PATH);
                });

        /// <summary>
        /// Returns a View to be rendered based on the state of the <paramref name="result"/>, using
        /// <paramref name="viewPath"/> if the result is successful and <see cref="ERROR_VIEW_PATH"/> if the result is failed.
        /// <paramref name="projection"/> is used to map the <typeparamref name="T"/> value to a <typeparamref name="U"/> value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="result"></param>
        /// <param name="component"></param>
        /// <param name="viewPath"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static IViewComponentResult Match<T, U>(this Result<T> result, ViewComponent component, string viewPath, Func<T, U> projection) =>
            result.Match(
                v => component.View(viewPath, projection(v)),
                error => 
                {
                    component.ModelState.AddModelError("componentError", result.Error);

                    return component.View(ERROR_VIEW_PATH);
                });

        /// <summary>
        /// Returns a View to be rendered based on the state of the <paramref name="resultTask"/>, using
        /// <paramref name="viewPath"/> if the result is successful and <see cref="ERROR_VIEW_PATH"/> if the result is failed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resultTask"></param>
        /// <param name="component"></param>
        /// <param name="viewPath"></param>
        /// <returns></returns>
        public static Task<IViewComponentResult> Match<T>(this Task<Result<T>> resultTask, ViewComponent component, string viewPath) =>
            resultTask.Match(
                v => component.View(viewPath, v),
                error => 
                {
                    component.ModelState.AddModelError("componentError", error);

                    return component.View(ERROR_VIEW_PATH) as IViewComponentResult;
                });

        /// <summary>
        /// Returns a View to be rendered based on the state of the <paramref name="resultTask"/>, using
        /// <paramref name="viewPath"/> if the result is successful and <see cref="ERROR_VIEW_PATH"/> if the result is failed.
        /// <paramref name="projection"/> is used to map the <typeparamref name="T"/> value to a <typeparamref name="U"/> value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="resultTask"></param>
        /// <param name="component"></param>
        /// <param name="viewPath"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static Task<IViewComponentResult> Match<T, U>(this Task<Result<T>> resultTask, ViewComponent component, string viewPath, Func<T, U> projection) =>
            resultTask.Match(
                v => component.View(viewPath, projection(v)),
                error => 
                {
                    component.ModelState.AddModelError("componentError", error);

                    return component.View(ERROR_VIEW_PATH) as IViewComponentResult;
                });
    }
}