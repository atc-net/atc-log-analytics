namespace Atc.LogAnalytics.Options;

/// <summary>
/// Configuration options for the Log Analytics client.
/// </summary>
public sealed class AtcLogAnalyticsOptions
{
    /// <summary>
    /// Gets or sets the Log Analytics workspace ID (GUID).
    /// Required for workspace queries.
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure credential for authentication.
    /// </summary>
    public TokenCredential? Credential { get; set; }

    /// <summary>
    /// Gets or sets a custom endpoint URI.
    /// When <see langword="null"/>, the Azure SDK uses the public cloud endpoint.
    /// </summary>
    public Uri? Endpoint { get; set; }
}