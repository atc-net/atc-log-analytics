namespace Atc.LogAnalytics;

/// <inheritdoc />
internal sealed class LogAnalyticsProcessor : ILogAnalyticsProcessor
{
    private readonly IScriptHandlerFactory factory;

    public LogAnalyticsProcessor(
        IScriptHandlerFactory factory,
        string? configurationName)
    {
        this.factory = factory;
        ConfigurationName = configurationName;
    }

    public string? ConfigurationName { get; }

    public Task<T[]?> ExecuteWorkspaceQuery<T>(
        ILogAnalyticsQuery<T> query,
        CancellationToken cancellationToken = default)
        where T : class
        => ExecuteWorkspaceQuery(
            query,
            options: null,
            cancellationToken);

    public Task<T[]?> ExecuteWorkspaceQuery<T>(
        ILogAnalyticsQuery<T> query,
        AtcLogAnalyticsQueryOptions? options,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(query);

        var handler = factory.CreateWorkspaceQueryHandler<T>(ConfigurationName);
        return handler.ExecuteAsync(query, options, cancellationToken);
    }

    public Task<T[]?> ExecuteResourceQuery<T>(
        ILogAnalyticsQuery<T> query,
        ResourceIdentifier resourceId,
        CancellationToken cancellationToken = default)
        where T : class
        => ExecuteResourceQuery(
            query,
            resourceId,
            options: null,
            cancellationToken);

    public Task<T[]?> ExecuteResourceQuery<T>(
        ILogAnalyticsQuery<T> query,
        ResourceIdentifier resourceId,
        AtcLogAnalyticsQueryOptions? options,
        CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(resourceId);

        var handler = factory.CreateResourceQueryHandler<T>(ConfigurationName);
        return handler.ExecuteAsync(query, resourceId, options, cancellationToken);
    }
}