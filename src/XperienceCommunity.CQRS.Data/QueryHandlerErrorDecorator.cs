using CMS.Core;
using XperienceCommunity.CQRS.Core;

namespace XperienceCommunity.CQRS.Data;

/// <summary>
/// Ensures that any exceptions thrown by decorated <see cref="IQueryHandler{TQuery, TResponse}"/>
/// are converted into failed <see cref="Result{T, E}"/>.
/// Caught exceptions are logged unless a <see cref="SkipLoggingAttribute"/> is applied to the <typeparamref name="TQuery"/>
/// </summary>
/// <typeparam name="TQuery"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class QueryHandlerErrorDecorator<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> handler;
    private readonly IEventLogService log;
    private readonly ISiteContext context;
    private readonly ISettingsService settings;

    public QueryHandlerErrorDecorator(
        IQueryHandler<TQuery, TResponse> handler,
        IEventLogService log,
        ISiteContext context,
        ISettingsService settings)
    {
        this.handler = handler;
        this.log = log;
        this.context = context;
        this.settings = settings;
    }

    public async Task<Result<TResponse>> Execute(TQuery query, CancellationToken token = default)
    {
        try
        {
            var result = await handler.Execute(query, token);

            if (result.IsFailure && ShouldLog(query) && settings.IsQueryHandlerErrorLoggingEnabled())
            {
                log.LogError(
                    handler.GetType().Name,
                    "QUERY_FAILURE",
                    result.Error,
                    context.SiteID);
            }

            return result;
        }
        /*
         * TaskCancelledExceptions can occur regularly, since they are sent by the browser
         * when a request is aborted mid-flight due to a navigation (for standard GET/POST)
         * or when an XHR has been canceled programmatically.
         * 
         * We don't want to log them
         */
        catch (Exception ex) when (ex is not TaskCanceledException)
        {
            if (ShouldLog(query) && settings.IsQueryHandlerErrorLoggingEnabled())
            {
                log.LogException(
                    handler.GetType().Name,
                    "QUERY_FAILURE",
                    ex,
                    context.SiteID,
                    ex.Message);
            }

            return Result.Failure<TResponse>(ex.Message);
        }
        catch (Exception ex) when (ex is TaskCanceledException cancelEx)
        {
            return Result.Failure<TResponse>(cancelEx.Message);
        }
    }

    private static bool ShouldLog(TQuery query) =>
        query.GetType()
            .GetCustomAttributes(typeof(SkipLoggingAttribute), false)
            .FirstOrDefault() is not SkipLoggingAttribute;
}
