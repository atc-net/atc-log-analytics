namespace Atc.LogAnalytics.Providers.Internal;

/// <summary>
/// Implementation of <see cref="ILogsQueryClientProvider"/>.
/// </summary>
internal sealed class LogsQueryClientProvider : ILogsQueryClientProvider
{
    private readonly IOptionsMonitor<AtcLogAnalyticsOptions> monitor;
    private readonly ConcurrentDictionary<string, LogsQueryClient> clients = new(StringComparer.OrdinalIgnoreCase);

    public LogsQueryClientProvider(
        IOptionsMonitor<AtcLogAnalyticsOptions> monitor)
        => this.monitor = monitor;

    public LogsQueryClient GetClient(string? configurationName = null)
        => clients.GetOrAdd(
            configurationName ?? string.Empty,
            static (name, monitor) => CreateClient(monitor.Get(name)),
            monitor);

    public string GetWorkspaceId(string? configurationName = null)
    {
        var options = monitor.Get(configurationName ?? string.Empty);

        if (string.IsNullOrEmpty(options.WorkspaceId))
        {
            throw new InvalidOperationException(
                "WorkspaceId is not configured. " +
                "Please configure WorkspaceId in AtcLogAnalyticsOptions.");
        }

        return options.WorkspaceId;
    }

    private static LogsQueryClient CreateClient(AtcLogAnalyticsOptions options)
    {
        if (options.Credential is null)
        {
            throw new InvalidOperationException(
                "Credential is not configured. " +
                "Please configure Credential in AtcLogAnalyticsOptions.");
        }

        return options.Endpoint is not null
            ? new LogsQueryClient(options.Endpoint, options.Credential)
            : new LogsQueryClient(options.Credential);
    }
}