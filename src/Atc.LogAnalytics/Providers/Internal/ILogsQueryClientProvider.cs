namespace Atc.LogAnalytics.Providers.Internal;

/// <summary>
/// Provider for <see cref="LogsQueryClient"/> instances.
/// </summary>
internal interface ILogsQueryClientProvider
{
    /// <summary>
    /// Gets a LogsQueryClient for the specified connection.
    /// </summary>
    /// <param name="connectionName">Optional connection name. Uses the default connection when null.</param>
    /// <returns>A configured <see cref="LogsQueryClient"/>.</returns>
    LogsQueryClient GetClient(string? connectionName = null);

    /// <summary>
    /// Gets the workspace ID for the specified connection.
    /// </summary>
    /// <param name="connectionName">Optional connection name. Uses the default connection when null.</param>
    /// <returns>The configured workspace ID.</returns>
    string GetWorkspaceId(string? connectionName = null);
}