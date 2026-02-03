namespace Atc.LogAnalytics.Factories;

/// <summary>
/// Factory for creating <see cref="ILogAnalyticsProcessor"/> instances.
/// </summary>
public interface ILogAnalyticsProcessorFactory
{
    /// <summary>
    /// Creates an instance of <see cref="ILogAnalyticsProcessor"/> configured with the specified connection settings.
    /// </summary>
    /// <param name="connectionName">
    /// An optional connection name that identifies the Log Analytics connection to be used.
    /// If <see langword="null"/>, the default connection is utilized.
    /// </param>
    /// <returns>A configured <see cref="ILogAnalyticsProcessor"/>.</returns>
    ILogAnalyticsProcessor Create(string? connectionName = null);
}