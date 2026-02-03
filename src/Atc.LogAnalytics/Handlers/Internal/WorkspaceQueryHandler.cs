namespace Atc.LogAnalytics.Handlers.Internal;

/// <summary>
/// Handler for executing workspace queries.
/// </summary>
/// <typeparam name="T">The type of the query result.</typeparam>
internal sealed partial class WorkspaceQueryHandler<T> : IWorkspaceQueryHandler<T>
    where T : class
{
    private readonly LogsQueryClient client;
    private readonly string workspaceId;

    public WorkspaceQueryHandler(
        ILogger<WorkspaceQueryHandler<T>> logger,
        LogsQueryClient client,
        string workspaceId)
    {
        this.logger = logger;
        this.client = client;
        this.workspaceId = workspaceId;
    }

    public async Task<T[]?> ExecuteAsync(
        ILogAnalyticsQuery<T> query,
        AtcLogAnalyticsQueryOptions? options,
        CancellationToken cancellationToken)
    {
        using var activity = LogAnalyticsDiagnostics.Source.StartActivity(
            LogAnalyticsDiagnostics.ActivityNames.WorkspaceQuery,
            ActivityKind.Client,
            default(ActivityContext));

        activity?.SetTag(LogAnalyticsDiagnostics.TagNames.WorkspaceId, workspaceId);

        var kql = query.ToKql();
        var queryOptions = options.ToLogsQueryOptions();
        var timeRange = options?.TimeRange ?? LogsQueryTimeRange.All;

        activity?.SetTag(LogAnalyticsDiagnostics.TagNames.DbStatement, kql);

        try
        {
            var response = await client.QueryWorkspaceAsync(
                workspaceId,
                kql,
                timeRange,
                queryOptions,
                cancellationToken);

            var result = response.ToResults<T>();

            activity?.SetTag(LogAnalyticsDiagnostics.TagNames.RowCount, result?.Length ?? 0);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return result;
        }
        catch (RequestFailedException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            LogFailed(ex, workspaceId, ex.Status);
            return null;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            LogUnhandledException(ex, workspaceId);
            return null;
        }
    }
}