namespace Atc.LogAnalytics.Factories.Internal;

/// <inheritdoc />
internal sealed class LogAnalyticsProcessorFactory(
    IScriptHandlerFactory scriptHandlerFactory)
    : ILogAnalyticsProcessorFactory
{
    /// <inheritdoc />
    public ILogAnalyticsProcessor Create(string? connectionName = null)
        => new LogAnalyticsProcessor(
            scriptHandlerFactory,
            connectionName);
}