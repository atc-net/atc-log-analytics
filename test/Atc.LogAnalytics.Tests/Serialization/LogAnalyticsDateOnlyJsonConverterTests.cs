namespace Atc.LogAnalytics.Tests.Serialization;

public sealed class LogAnalyticsDateOnlyJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new LogAnalyticsDateOnlyJsonConverter() },
    };

    [Fact]
    public void Read_DateOnlyString_ReturnsCorrectDate()
    {
        // Arrange
        var json = "\"2024-01-15\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        // Act
        var result = new LogAnalyticsDateOnlyJsonConverter()
            .Read(ref reader, typeof(DateOnly), Options);

        // Assert
        result.Should().Be(new DateOnly(2024, 1, 15));
    }

    [Fact]
    public void Read_DateTimeString_ReturnsDatePortion()
    {
        // Arrange - Kusto's startofday() returns full datetime
        var json = "\"2024-01-15T00:00:00Z\""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        // Act
        var result = new LogAnalyticsDateOnlyJsonConverter()
            .Read(ref reader, typeof(DateOnly), Options);

        // Assert
        result.Should().Be(new DateOnly(2024, 1, 15));
    }

    [Fact]
    public void Read_NullValue_ThrowsJsonException()
    {
        // Act
        var act = () => DeserializeDateOnly("null");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_InvalidString_ThrowsJsonException()
    {
        // Act
        var act = () => DeserializeDateOnly("\"not-a-date\"");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Write_DateOnly_WritesIso8601Format()
    {
        // Arrange
        var buffer = new System.Buffers.ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        var date = new DateOnly(2024, 1, 15);

        // Act
        new LogAnalyticsDateOnlyJsonConverter().Write(writer, date, Options);
        writer.Flush();

        // Assert
        Encoding.UTF8.GetString(buffer.WrittenSpan).Should().Be("\"2024-01-15\"");
    }

    [Theory]
    [InlineData("2024-01-15T00:00:00Z")]
    [InlineData("2024-01-15")]
    public void EndToEnd_Deserialize(string dateString)
    {
        // Arrange
        var json = $$"""{"ReportDate":"{{dateString}}"}""";

        // Act
        var result = JsonSerializer.Deserialize<DateRecord>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result!.ReportDate.Should().Be(new DateOnly(2024, 1, 15));
    }

    [Fact]
    public void EndToEnd_Serialize()
    {
        // Arrange
        var record = new DateRecord(new DateOnly(2024, 1, 15));

        // Act
        var json = JsonSerializer.Serialize(record, Options);

        // Assert
        json.Should().Contain("2024-01-15");
    }

    private static DateOnly DeserializeDateOnly(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read();

        return new LogAnalyticsDateOnlyJsonConverter().Read(ref reader, typeof(DateOnly), Options);
    }

    private sealed record DateRecord(DateOnly ReportDate);
}