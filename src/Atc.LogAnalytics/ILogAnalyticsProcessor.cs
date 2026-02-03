namespace Atc.LogAnalytics;

/// <summary>
/// Processor for executing Log Analytics queries.
/// </summary>
public interface ILogAnalyticsProcessor
{
    /// <summary>
    /// Executes a query against the configured workspace.
    /// </summary>
    /// <typeparam name="T">The type of the query result.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query results, or null if no results.</returns>
    Task<T[]?> ExecuteWorkspaceQuery<T>(
        ILogAnalyticsQuery<T> query,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Executes a query against the configured workspace with options.
    /// </summary>
    /// <typeparam name="T">The type of the query result.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="options">Query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query results, or null if no results.</returns>
    Task<T[]?> ExecuteWorkspaceQuery<T>(
        ILogAnalyticsQuery<T> query,
        AtcLogAnalyticsQueryOptions? options,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Executes a query against a specific Azure resource.
    /// </summary>
    /// <typeparam name="T">The type of the query result.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="resourceId">The Azure resource ID to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query results, or null if no results.</returns>
    Task<T[]?> ExecuteResourceQuery<T>(
        ILogAnalyticsQuery<T> query,
        ResourceIdentifier resourceId,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Executes a query against a specific Azure resource with options.
    /// </summary>
    /// <typeparam name="T">The type of the query result.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="resourceId">The Azure resource ID to query.</param>
    /// <param name="options">Query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query results, or null if no results.</returns>
    Task<T[]?> ExecuteResourceQuery<T>(
        ILogAnalyticsQuery<T> query,
        ResourceIdentifier resourceId,
        AtcLogAnalyticsQueryOptions? options,
        CancellationToken cancellationToken = default)
        where T : class;
}