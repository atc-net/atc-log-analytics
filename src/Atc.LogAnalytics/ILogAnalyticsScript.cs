namespace Atc.LogAnalytics;

/// <summary>
/// Represents a Log Analytics script that can be executed against a workspace or resource.
/// </summary>
public interface ILogAnalyticsScript
{
    /// <summary>
    /// Gets the KQL script content.
    /// </summary>
    string Script { get; }

    /// <summary>
    /// Gets the parameters to be applied to the script.
    /// </summary>
    IReadOnlyDictionary<string, object?> Parameters { get; }
}