namespace Atc.LogAnalytics.Api.Sample.Queries;

public record HeartbeatQuery(
    string? Computer = null)
    : LogAnalyticsQuery<HeartbeatRecord>;