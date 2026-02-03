namespace Atc.LogAnalytics.Handlers;

/// <summary>
/// Handler for executing resource queries.
/// </summary>
/// <typeparam name="T">The type of the query result.</typeparam>
public interface IResourceQueryHandler<T>
    where T : class
{
    /// <summary>
    /// Executes a query against a resource.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="resourceId">The resource ID.</param>
    /// <param name="options">Query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query results.</returns>
    Task<T[]?> ExecuteAsync(
        ILogAnalyticsQuery<T> query,
        ResourceIdentifier resourceId,
        AtcLogAnalyticsQueryOptions? options,
        CancellationToken cancellationToken);
}