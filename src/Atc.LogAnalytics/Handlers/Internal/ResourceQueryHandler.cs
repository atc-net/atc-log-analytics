namespace Atc.LogAnalytics.Handlers.Internal;

/// <summary>
/// Handler for executing resource queries.
/// </summary>
/// <typeparam name="T">The type of the query result.</typeparam>
internal sealed partial class ResourceQueryHandler<T> : IResourceQueryHandler<T>
    where T : class
{
    private readonly LogsQueryClient client;

    public ResourceQueryHandler(
        ILogger<ResourceQueryHandler<T>> logger,
        LogsQueryClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task<T[]?> ExecuteAsync(
        ILogAnalyticsQuery<T> query,
        ResourceIdentifier resourceId,
        AtcLogAnalyticsQueryOptions? options,
        CancellationToken cancellationToken)
    {
        using var activity = LogAnalyticsDiagnostics.Source.StartActivity(
            LogAnalyticsDiagnostics.ActivityNames.ResourceQuery,
            ActivityKind.Client,
            default(ActivityContext));

        activity?.SetTag(LogAnalyticsDiagnostics.TagNames.ResourceId, resourceId.ToString());

        var kql = query.ToKql();
        var queryOptions = options.ToLogsQueryOptions();
        var timeRange = options?.TimeRange ?? LogsQueryTimeRange.All;

        activity?.SetTag(LogAnalyticsDiagnostics.TagNames.DbStatement, kql);

        try
        {
            var response = await client.QueryResourceAsync(
                resourceId,
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
            LogFailed(ex, resourceId.ToString(), ex.Status);
            return null;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            LogUnhandledException(ex, resourceId.ToString());
            return null;
        }
    }
}