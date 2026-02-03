namespace Atc.LogAnalytics.Tests.Serialization;

public sealed class LogAnalyticsBooleanJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new LogAnalyticsBooleanJsonConverter() },
    };

    [Fact]
    public void Read_TrueToken_ReturnsTrue()
    {
        // Arrange
        var json = "true"u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        // Act
        var result = new LogAnalyticsBooleanJsonConverter()
            .Read(ref reader, typeof(bool), Options);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Read_FalseToken_ReturnsFalse()
    {
        // Arrange
        var json = "false"u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        // Act
        var result = new LogAnalyticsBooleanJsonConverter()
            .Read(ref reader, typeof(bool), Options);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Read_NumericZero_ReturnsFalse()
    {
        // Arrange
        var json = "0"u8;
        var reader = new Utf8JsonReader(json);
        reader.Read();

        // Act
        var result = new LogAnalyticsBooleanJsonConverter()
            .Read(ref reader, typeof(bool), Options);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(-1)]
    [InlineData(42)]
    public void Read_NonZeroNumber_ReturnsTrue(int value)
    {
        // Arrange
        var json = System.Text.Encoding.UTF8.GetBytes(value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var reader = new Utf8JsonReader(json);
        reader.Read();

        // Act
        var result = new LogAnalyticsBooleanJsonConverter()
            .Read(ref reader, typeof(bool), Options);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Read_StringToken_ThrowsJsonException()
    {
        // Act
        var act = () => DeserializeBoolean("\"true\"");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_NullToken_ThrowsJsonException()
    {
        // Act
        var act = () => DeserializeBoolean("null");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Write_True_WritesTrueToken()
    {
        // Arrange
        var buffer = new System.Buffers.ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        // Act
        new LogAnalyticsBooleanJsonConverter().Write(writer, true, Options);
        writer.Flush();

        // Assert
        System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan).Should().Be("true");
    }

    [Fact]
    public void Write_False_WritesFalseToken()
    {
        // Arrange
        var buffer = new System.Buffers.ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        // Act
        new LogAnalyticsBooleanJsonConverter().Write(writer, false, Options);
        writer.Flush();

        // Assert
        Encoding.UTF8.GetString(buffer.WrittenSpan).Should().Be("false");
    }

    [Fact]
    public void EndToEnd_Deserialize_NumericBoolean()
    {
        // Arrange - Kusto returns booleans as sbyte 0/1
        const string json = """{"IsActive":1}""";

        // Act
        var result = JsonSerializer.Deserialize<BoolRecord>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public void EndToEnd_Deserialize_StandardBoolean()
    {
        // Arrange
        const string json = """{"IsActive":true}""";

        // Act
        var result = JsonSerializer.Deserialize<BoolRecord>(json, Options);

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public void EndToEnd_Serialize()
    {
        // Arrange
        var record = new BoolRecord(true);

        // Act
        var json = JsonSerializer.Serialize(record, Options);

        // Assert
        json.Should().Contain("true");
    }

    private static bool DeserializeBoolean(string json)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        reader.Read();
        return new LogAnalyticsBooleanJsonConverter().Read(ref reader, typeof(bool), Options);
    }

    private sealed record BoolRecord(bool IsActive);
}