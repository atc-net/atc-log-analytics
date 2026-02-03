namespace Atc.LogAnalytics.Sample.Queries;

/// <summary>
/// Represents a heartbeat record from Log Analytics.
/// </summary>
/// <param name="TimeGenerated">The time the heartbeat was generated.</param>
/// <param name="Computer">The computer name.</param>
/// <param name="OSType">The operating system type.</param>
/// <param name="Version">The version information.</param>
public record HeartbeatRecord(
    DateTimeOffset TimeGenerated,
    string Computer,
    string OSType,
    string Version);