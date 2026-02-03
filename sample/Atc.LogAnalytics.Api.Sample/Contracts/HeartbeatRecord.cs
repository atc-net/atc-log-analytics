namespace Atc.LogAnalytics.Api.Sample.Contracts;

public record HeartbeatRecord(
    DateTime TimeGenerated,
    string Computer,
    string Category,
    string OSType,
    string Version,
    string ComputerEnvironment);