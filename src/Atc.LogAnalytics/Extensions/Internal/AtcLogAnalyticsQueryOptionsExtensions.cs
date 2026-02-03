namespace Atc.LogAnalytics.Extensions.Internal;

/// <summary>
/// Extension methods for <see cref="AtcLogAnalyticsQueryOptions"/>.
/// </summary>
internal static class AtcLogAnalyticsQueryOptionsExtensions
{
    /// <summary>
    /// Converts <see cref="AtcLogAnalyticsQueryOptions"/> to the Azure SDK <see cref="LogsQueryOptions"/>.
    /// </summary>
    /// <param name="options">The options to convert, or <see langword="null"/> for defaults.</param>
    /// <returns>A configured <see cref="LogsQueryOptions"/> instance.</returns>
    internal static LogsQueryOptions ToLogsQueryOptions(
        this AtcLogAnalyticsQueryOptions? options)
    {
        var queryOptions = new LogsQueryOptions();

        if (options is null)
        {
            return queryOptions;
        }

        queryOptions.ServerTimeout = options.ServerTimeout;

        if (options.AdditionalWorkspaces is { Count: > 0 })
        {
            foreach (var workspace in options.AdditionalWorkspaces)
            {
                queryOptions.AdditionalWorkspaces.Add(workspace);
            }
        }

        return queryOptions;
    }
}