namespace Atc.LogAnalytics.Tests.Serialization;

public sealed class LogAnalyticsJsonSerializerOptionsTests
{
    [Fact]
    public void Default_PropertyNameCaseInsensitive_IsTrue()
        => LogAnalyticsJsonSerializerOptions.Default.PropertyNameCaseInsensitive.Should().BeTrue();

    [Fact]
    public void Default_NumberHandling_AllowsReadingFromString()
        => LogAnalyticsJsonSerializerOptions.Default.NumberHandling.Should().HaveFlag(JsonNumberHandling.AllowReadingFromString);

    [Fact]
    public void Default_CanDeserialize_NumberFromString()
    {
        // Arrange - Azure Monitor sometimes returns numbers as strings
        const string json = """{"value":"42"}""";

        // Act
        var result = JsonSerializer.Deserialize<NumberRecord>(json, LogAnalyticsJsonSerializerOptions.Default);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(42);
    }

    [Fact]
    public void Default_HasEnumConverter()
        => LogAnalyticsJsonSerializerOptions.Default.Converters.Should().ContainItemsAssignableTo<JsonStringEnumConverter>();

    [Fact]
    public void Default_HasBooleanConverter()
        => LogAnalyticsJsonSerializerOptions.Default.Converters.Should().ContainItemsAssignableTo<LogAnalyticsBooleanJsonConverter>();

    [Fact]
    public void Default_HasDateOnlyConverter()
        => LogAnalyticsJsonSerializerOptions.Default.Converters.Should().ContainItemsAssignableTo<LogAnalyticsDateOnlyJsonConverter>();

    private sealed record NumberRecord(int Value);
}