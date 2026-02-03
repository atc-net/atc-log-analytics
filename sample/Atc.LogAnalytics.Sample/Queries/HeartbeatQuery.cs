namespace Atc.LogAnalytics.Sample.Queries;

/// <summary>
/// Query to retrieve heartbeat records from Log Analytics.
/// </summary>
public record HeartbeatQuery()
    : LogAnalyticsQuery<HeartbeatRecord>;