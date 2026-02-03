namespace Atc.LogAnalytics.Factories;

/// <summary>
/// Factory for creating script handlers.
/// </summary>
public interface IScriptHandlerFactory
{
    /// <summary>
    /// Creates a handler for workspace queries.
    /// </summary>
    /// <typeparam name="T">The type of the query result.</typeparam>
    /// <param name="connectionName">
    /// Optional connection name; the default connection is used when <see langword="null"/>.
    /// </param>
    /// <returns>A workspace query handler.</returns>
    IWorkspaceQueryHandler<T> CreateWorkspaceQueryHandler<T>(
        string? connectionName = null)
        where T : class;

    /// <summary>
    /// Creates a handler for resource queries.
    /// </summary>
    /// <typeparam name="T">The type of the query result.</typeparam>
    /// <param name="connectionName">
    /// Optional connection name; the default connection is used when <see langword="null"/>.
    /// </param>
    /// <returns>A resource query handler.</returns>
    IResourceQueryHandler<T> CreateResourceQueryHandler<T>(
        string? connectionName = null)
        where T : class;
}