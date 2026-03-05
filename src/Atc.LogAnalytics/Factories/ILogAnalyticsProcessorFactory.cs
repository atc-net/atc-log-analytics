namespace Atc.LogAnalytics.Factories;

/// <summary>
/// Factory for creating <see cref="ILogAnalyticsProcessor"/> instances.
/// </summary>
public interface ILogAnalyticsProcessorFactory
{
    /// <summary>
    /// Creates an instance of <see cref="ILogAnalyticsProcessor"/> configured with the specified connection settings.
    /// </summary>
    /// <param name="configurationName">
    /// An optional configuration name that identifies the Log Analytics configuration to be used.
    /// If <see langword="null"/>, the default configuration is utilized.
    /// </param>
    /// <returns>A configured <see cref="ILogAnalyticsProcessor"/>.</returns>
    ILogAnalyticsProcessor Create(string? configurationName = null);
}