namespace Atc.LogAnalytics.Extensions.Internal;

/// <summary>
/// Extension methods for <see cref="Response{LogsQueryResult}"/>.
/// </summary>
internal static class LogsQueryResponseExtensions
{
    /// <summary>
    /// Extracts typed results from a Log Analytics query response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize rows into.</typeparam>
    /// <param name="response">The query response.</param>
    /// <returns>An array of <typeparamref name="T"/> or <see langword="null"/> when no rows are returned.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the query failed.</exception>
    internal static T[]? ToResults<T>(this Response<LogsQueryResult> response)
        where T : class
    {
        var result = response.Value;

        if (result.Status == LogsQueryResultStatus.Failure)
        {
            throw new InvalidOperationException($"Query failed: {result.Error?.Message}");
        }

        if (result.Status == LogsQueryResultStatus.PartialFailure)
        {
            Activity.Current?.SetTag("loganalytics.partial_failure", true);

            Activity.Current?.AddEvent(new ActivityEvent(
                "partial_failure",
                tags: new ActivityTagsCollection { { "error.message", result.Error?.Message } }));
        }

        var table = result.Table;
        return table.Rows.Count == 0
            ? null
            : table.ReadObjects<T>();
    }
}