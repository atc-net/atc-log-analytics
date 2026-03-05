namespace Atc.LogAnalytics.Factories.Internal;

/// <summary>
/// Implementation of <see cref="IScriptHandlerFactory"/>.
/// </summary>
internal sealed class ScriptHandlerFactory(
    ILoggerFactory loggerFactory,
    ILogsQueryClientProvider clientProvider)
    : IScriptHandlerFactory
{
    public IWorkspaceQueryHandler<T> CreateWorkspaceQueryHandler<T>(
        string? configurationName = null)
        where T : class
        => new WorkspaceQueryHandler<T>(
            loggerFactory.CreateLogger<WorkspaceQueryHandler<T>>(),
            clientProvider.GetClient(configurationName),
            clientProvider.GetWorkspaceId(configurationName));

    public IResourceQueryHandler<T> CreateResourceQueryHandler<T>(
        string? configurationName = null)
        where T : class
        => new ResourceQueryHandler<T>(
            loggerFactory.CreateLogger<ResourceQueryHandler<T>>(),
            clientProvider.GetClient(configurationName));
}