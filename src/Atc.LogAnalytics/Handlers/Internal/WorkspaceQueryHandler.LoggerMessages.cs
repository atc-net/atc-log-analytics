namespace Atc.LogAnalytics.Handlers.Internal;

internal sealed partial class WorkspaceQueryHandler<T>
{
    private readonly ILogger<WorkspaceQueryHandler<T>> logger;

    [LoggerMessage(
        EventId = LoggingEventIdConstants.WorkspaceQueryHandler.Failed,
        Level = LogLevel.Error,
        Message = "Workspace query failed on {WorkspaceId} with status {StatusCode}")]
    private partial void LogFailed(
        Exception exception,
        string workspaceId,
        int statusCode);

    [LoggerMessage(
        EventId = LoggingEventIdConstants.WorkspaceQueryHandler.UnhandledException,
        Level = LogLevel.Error,
        Message = "Workspace query encountered an unhandled exception on {WorkspaceId}")]
    private partial void LogUnhandledException(
        Exception exception,
        string workspaceId);
}