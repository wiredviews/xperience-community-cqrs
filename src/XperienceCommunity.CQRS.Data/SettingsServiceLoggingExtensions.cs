using CMS.Helpers;

namespace CMS.Core;

public static class SettingsServiceLoggingExtensions
{
    /// <summary>
    /// If true, logging of any exceptions caught by an error boundary will be logged to the Xperience Event Log.
    /// </summary>
    /// <param name="settingsService"></param>
    /// <remarks>Defaults to false</remarks>
    /// <returns></returns>
    public static bool IsErrorBoundaryErrorLoggingEnabled(this ISettingsService settingsService) =>
        ValidationHelper.GetBoolean(settingsService["Logging_ErrorBoundaryErrorLoggingEnabled"], false);

    /// <summary>
    /// If true, logging of any exceptions or failed <see cref="Result{T}"/> will be logged to the Xperience Event Log.
    /// </summary>
    /// <param name="settingsService"></param>
    /// <remarks>Defaults to true</remarks>
    /// <returns></returns>
    public static bool IsQueryHandlerErrorLoggingEnabled(this ISettingsService settingsService) =>
        ValidationHelper.GetBoolean(settingsService["Logging_QueryHandlerErrorLoggingEnabled"], true);
}
