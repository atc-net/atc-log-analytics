namespace Atc.LogAnalytics.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures Log Analytics services with the specified options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action.</param>
    /// <param name="configurationName">
    /// An optional name for the options instance. If provided, the named options will be registered.
    /// </param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureLogAnalytics(
        this IServiceCollection services,
        Action<AtcLogAnalyticsOptions> configure,
        string? configurationName = null)
    {
        if (string.IsNullOrWhiteSpace(configurationName))
        {
            services.AddOptions<AtcLogAnalyticsOptions>().Configure(configure);
        }
        else
        {
            services.AddOptions<AtcLogAnalyticsOptions>(configurationName).Configure(configure);
        }

        return services.AddLogAnalyticsServices();
    }

    private static IServiceCollection AddLogAnalyticsServices(
        this IServiceCollection services)
    {
        services.AddLogging();

        return services
            .AddSingleton<ILogsQueryClientProvider, LogsQueryClientProvider>()
            .AddSingleton<IScriptHandlerFactory, ScriptHandlerFactory>()
            .AddSingleton<ILogAnalyticsProcessorFactory, LogAnalyticsProcessorFactory>()
            .AddSingleton(s => s.GetRequiredService<ILogAnalyticsProcessorFactory>().Create());
    }
}