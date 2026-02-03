namespace Atc.LogAnalytics.Handlers;

/// <summary>
/// Handler for executing workspace queries.
/// </summary>
/// <typeparam name="T">The type of the query result.</typeparam>
public interface IWorkspaceQueryHandler<T>
    where T : class
{
    /// <summary>
    /// Executes a query against a workspace.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="options">Query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query results.</returns>
    Task<T[]?> ExecuteAsync(
        ILogAnalyticsQuery<T> query,
        AtcLogAnalyticsQueryOptions? options,
        CancellationToken cancellationToken);
}