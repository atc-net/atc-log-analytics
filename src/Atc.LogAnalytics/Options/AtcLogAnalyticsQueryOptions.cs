namespace Atc.LogAnalytics.Options;

/// <summary>
/// Per-query options for Log Analytics queries.
/// </summary>
public sealed class AtcLogAnalyticsQueryOptions
{
    /// <summary>
    /// Gets or sets the time range for the query.
    /// If null, the query must specify its own time filter.
    /// </summary>
    public LogsQueryTimeRange? TimeRange { get; set; }

    /// <summary>
    /// Gets or sets the server-side query timeout.
    /// </summary>
    public TimeSpan? ServerTimeout { get; set; }

    /// <summary>
    /// Gets or sets additional workspace IDs for this specific query.
    /// </summary>
    public IList<string>? AdditionalWorkspaces { get; init; }
}