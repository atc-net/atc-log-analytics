namespace Atc.LogAnalytics.Handlers.Internal;

internal sealed partial class ResourceQueryHandler<T>
{
    private readonly ILogger<ResourceQueryHandler<T>> logger;

    [LoggerMessage(
        EventId = LoggingEventIdConstants.ResourceQueryHandler.Failed,
        Level = LogLevel.Error,
        Message = "Resource query failed on {ResourceId} with status {StatusCode}")]
    private partial void LogFailed(
        Exception exception,
        string resourceId,
        int statusCode);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.ResourceQueryHandler.UnhandledException,
        Level = LogLevel.Error,
        Message = "Resource query encountered an unhandled exception on {ResourceId}")]
    private partial void LogUnhandledException(
        Exception exception,
        string resourceId);
}