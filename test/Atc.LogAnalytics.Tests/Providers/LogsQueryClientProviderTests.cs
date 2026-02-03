namespace Atc.LogAnalytics.Tests.Providers;

public sealed class LogsQueryClientProviderTests
{
    private readonly IOptionsMonitor<AtcLogAnalyticsOptions> optionsMonitor = Substitute.For<IOptionsMonitor<AtcLogAnalyticsOptions>>();

    [Fact]
    public void GetClient_DefaultConfig_ReturnsClient()
    {
        // Arrange
        var options = new AtcLogAnalyticsOptions
        {
            Credential = new DefaultAzureCredential(),
        };

        optionsMonitor
            .Get(string.Empty)
            .Returns(options);

        var provider = new LogsQueryClientProvider(optionsMonitor);

        // Act
        var client = provider.GetClient();

        // Assert
        client.Should().NotBeNull();
        client.Should().BeOfType<LogsQueryClient>();
    }

    [Fact]
    public void GetClient_SameName_ReturnsCachedClient()
    {
        // Arrange
        var options = new AtcLogAnalyticsOptions
        {
            Credential = new DefaultAzureCredential(),
        };

        optionsMonitor
            .Get("cached")
            .Returns(options);

        var provider = new LogsQueryClientProvider(optionsMonitor);

        // Act
        var client1 = provider.GetClient("cached");
        var client2 = provider.GetClient("cached");

        // Assert
        client1.Should().BeSameAs(client2);
    }

    [Fact]
    public void GetClient_DifferentNames_ReturnsDifferentClients()
    {
        // Arrange
        var options1 = new AtcLogAnalyticsOptions
        {
            Credential = new DefaultAzureCredential(),
        };

        var options2 = new AtcLogAnalyticsOptions
        {
            Credential = new DefaultAzureCredential(),
        };

        optionsMonitor
            .Get("config1")
            .Returns(options1);

        optionsMonitor
            .Get("config2")
            .Returns(options2);

        var provider = new LogsQueryClientProvider(optionsMonitor);

        // Act
        var client1 = provider.GetClient("config1");
        var client2 = provider.GetClient("config2");

        // Assert
        client1.Should().NotBeSameAs(client2);
    }

    [Fact]
    public void GetClient_NullCredential_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new AtcLogAnalyticsOptions
        {
            Credential = null,
        };

        optionsMonitor
            .Get("no-cred")
            .Returns(options);

        var provider = new LogsQueryClientProvider(optionsMonitor);

        // Act
        var act = () => provider.GetClient("no-cred");

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*Credential*");
    }

    [Fact]
    public void GetClient_WithEndpoint_ReturnsClient()
    {
        // Arrange
        var options = new AtcLogAnalyticsOptions
        {
            Credential = new DefaultAzureCredential(),
            Endpoint = new Uri("https://custom.endpoint.azure.com"),
        };

        optionsMonitor
            .Get("with-endpoint")
            .Returns(options);

        var provider = new LogsQueryClientProvider(optionsMonitor);

        // Act
        var client = provider.GetClient("with-endpoint");

        // Assert
        client.Should().NotBeNull();
        client.Endpoint.Should().BeEquivalentTo(new Uri("https://custom.endpoint.azure.com"));
    }

    [Fact]
    public void GetClient_CaseInsensitiveName_ReturnsSameClient()
    {
        // Arrange
        var options = new AtcLogAnalyticsOptions
        {
            Credential = new DefaultAzureCredential(),
        };

        optionsMonitor
            .Get("MyConfig")
            .Returns(options);

        var provider = new LogsQueryClientProvider(optionsMonitor);

        // Act
        var client1 = provider.GetClient("MyConfig");
        var client2 = provider.GetClient("myconfig");

        // Assert
        client1.Should().BeSameAs(client2);
    }

    [Fact]
    public void GetWorkspaceId_ReturnsConfiguredWorkspaceId()
    {
        // Arrange
        var options = new AtcLogAnalyticsOptions
        {
            WorkspaceId = "test-workspace-id",
        };

        optionsMonitor
            .Get(string.Empty)
            .Returns(options);

        var provider = new LogsQueryClientProvider(optionsMonitor);

        // Act
        var workspaceId = provider.GetWorkspaceId();

        // Assert
        workspaceId.Should().Be("test-workspace-id");
    }

    [Fact]
    public void GetWorkspaceId_NamedConfig_ReturnsConfiguredWorkspaceId()
    {
        // Arrange
        var options = new AtcLogAnalyticsOptions
        {
            WorkspaceId = "named-workspace-id",
        };

        optionsMonitor
            .Get("MyConfig")
            .Returns(options);

        var provider = new LogsQueryClientProvider(optionsMonitor);

        // Act
        var workspaceId = provider.GetWorkspaceId("MyConfig");

        // Assert
        workspaceId.Should().Be("named-workspace-id");
    }

    [Fact]
    public void GetWorkspaceId_EmptyWorkspaceId_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = new AtcLogAnalyticsOptions
        {
            WorkspaceId = string.Empty,
        };

        optionsMonitor
            .Get(string.Empty)
            .Returns(options);

        var provider = new LogsQueryClientProvider(optionsMonitor);

        // Act
        var act = () => provider.GetWorkspaceId();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*WorkspaceId*");
    }
}