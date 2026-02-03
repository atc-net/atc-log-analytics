namespace Atc.LogAnalytics.Api.Sample.Contracts;

public record IotEdgeModuleLogRecord(
    DateTime TimeGenerated,
    string IotHub,
    string Device,
    string Module,
    string Stream,
    string LogLevel,
    string Text);