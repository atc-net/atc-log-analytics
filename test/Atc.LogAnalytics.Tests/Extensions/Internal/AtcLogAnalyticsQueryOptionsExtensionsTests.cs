namespace Atc.LogAnalytics.Tests.Extensions.Internal;

public sealed class AtcLogAnalyticsQueryOptionsExtensionsTests
{
    [Fact]
    public void ToLogsQueryOptions_Null_Returns_Default()
    {
        // Arrange
        AtcLogAnalyticsQueryOptions? options = null;

        // Act
        var result = options.ToLogsQueryOptions();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ToLogsQueryOptions_CopiesAllProperties()
    {
        // Arrange
        var options = new AtcLogAnalyticsQueryOptions
        {
            ServerTimeout = TimeSpan.FromMinutes(5),
            AdditionalWorkspaces = ["ws-1", "ws-2"],
        };

        // Act
        var result = options.ToLogsQueryOptions();

        // Assert
        result.ServerTimeout.Should().Be(TimeSpan.FromMinutes(5));
        result.AdditionalWorkspaces.Should().Contain("ws-1");
        result.AdditionalWorkspaces.Should().Contain("ws-2");
    }
}