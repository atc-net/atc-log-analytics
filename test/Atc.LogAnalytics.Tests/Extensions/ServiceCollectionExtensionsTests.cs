namespace Atc.LogAnalytics.Tests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigureLogAnalytics_Should_Register_Required_Services()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ConfigureLogAnalytics(options =>
        {
            options.WorkspaceId = "test-workspace-id";
            options.Credential = new DefaultAzureCredential();
        });

        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<ILogAnalyticsProcessorFactory>().Should().NotBeNull();
        provider.GetService<IScriptHandlerFactory>().Should().NotBeNull();
        provider.GetService<ILogAnalyticsProcessor>().Should().NotBeNull();
    }

    [Fact]
    public void ConfigureLogAnalytics_Should_Configure_Default_Options()
    {
        // Arrange
        var services = new ServiceCollection();
        const string workspaceId = "test-workspace-guid";

        // Act
        services.ConfigureLogAnalytics(options =>
        {
            options.WorkspaceId = workspaceId;
            options.Credential = new DefaultAzureCredential();
        });

        var provider = services.BuildServiceProvider();
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<AtcLogAnalyticsOptions>>();
        var options = optionsMonitor.CurrentValue;

        // Assert
        options.WorkspaceId.Should().Be(workspaceId);
        options.Credential.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureLogAnalytics_With_Named_Configuration_Should_Register_Multiple_Configs()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ConfigureLogAnalytics(
            options => options.WorkspaceId = "prod-workspace",
            configurationName: "Production");

        services.ConfigureLogAnalytics(
            options => options.WorkspaceId = "dev-workspace",
            configurationName: "Development");

        var provider = services.BuildServiceProvider();
        var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<AtcLogAnalyticsOptions>>();

        // Assert
        optionsMonitor.Get("Production").WorkspaceId.Should().Be("prod-workspace");
        optionsMonitor.Get("Development").WorkspaceId.Should().Be("dev-workspace");
    }
}