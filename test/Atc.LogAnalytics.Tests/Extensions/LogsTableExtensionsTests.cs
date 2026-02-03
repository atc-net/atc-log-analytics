namespace Atc.LogAnalytics.Tests.Extensions;

public sealed class LogsTableExtensionsTests
{
    [Fact]
    public void ReadObjects_BasicStringAndInt_DeserializesCorrectly()
    {
        // Arrange
        var columns = new[]
        {
            LogsQueryModelFactory.LogsTableColumn("Name", LogsColumnType.String),
            LogsQueryModelFactory.LogsTableColumn("Count", LogsColumnType.Int),
        };

        var rows = new[]
        {
            LogsQueryModelFactory.LogsTableRow(columns, ["Alice", 42]),
        };

        var table = LogsQueryModelFactory.LogsTable("PrimaryResult", columns, rows);

        // Act
        var result = table.ReadObjects<SimpleRecord>();

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Alice");
        result[0].Count.Should().Be(42);
    }

    [Fact]
    public void ReadObjects_MultipleRows_ReturnsAll()
    {
        // Arrange
        var columns = new[]
        {
            LogsQueryModelFactory.LogsTableColumn("Name", LogsColumnType.String),
        };

        var rows = new[]
        {
            LogsQueryModelFactory.LogsTableRow(columns, ["Alice"]),
            LogsQueryModelFactory.LogsTableRow(columns, ["Bob"]),
            LogsQueryModelFactory.LogsTableRow(columns, ["Charlie"]),
        };

        var table = LogsQueryModelFactory.LogsTable("PrimaryResult", columns, rows);

        // Act
        var result = table.ReadObjects<NameRecord>();

        // Assert
        result.Should().HaveCount(3);
        result.Select(x => x.Name).Should().ContainInOrder("Alice", "Bob", "Charlie");
    }

    [Fact]
    public void ReadObjects_NullColumnValue_DeserializesAsNull()
    {
        // Arrange
        var columns = new[]
        {
            LogsQueryModelFactory.LogsTableColumn("Name", LogsColumnType.String),
            LogsQueryModelFactory.LogsTableColumn("Description", LogsColumnType.String),
        };

        var rows = new[]
        {
            LogsQueryModelFactory.LogsTableRow(columns, ["Alice", null!]),
        };

        var table = LogsQueryModelFactory.LogsTable("PrimaryResult", columns, rows);

        // Act
        var result = table.ReadObjects<NullableRecord>();

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Alice");
        result[0].Description.Should().BeNull();
    }

    [Fact]
    public void ReadObjects_WithCustomOptions_UsesProvidedOptions()
    {
        // Arrange — column name is lowercase, record property is PascalCase
        var columns = new[]
        {
            LogsQueryModelFactory.LogsTableColumn("name", LogsColumnType.String),
        };

        var rows = new[]
        {
            LogsQueryModelFactory.LogsTableRow(columns, ["Alice"]),
        };

        var table = LogsQueryModelFactory.LogsTable("PrimaryResult", columns, rows);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var result = table.ReadObjects<NameRecord>(options);

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Alice");
    }

    [Fact]
    public void ReadObjects_LongColumn_DeserializesCorrectly()
    {
        // Arrange
        var columns = new[]
        {
            LogsQueryModelFactory.LogsTableColumn("Value", LogsColumnType.Long),
        };

        var rows = new[]
        {
            LogsQueryModelFactory.LogsTableRow(columns, [9_999_999_999L]),
        };

        var table = LogsQueryModelFactory.LogsTable("PrimaryResult", columns, rows);

        // Act
        var result = table.ReadObjects<LongRecord>();

        // Assert
        result.Should().ContainSingle();
        result[0].Value.Should().Be(9_999_999_999L);
    }

    [Fact]
    public void ReadObjects_RealColumn_DeserializesCorrectly()
    {
        // Arrange
        var columns = new[]
        {
            LogsQueryModelFactory.LogsTableColumn("Value", LogsColumnType.Real),
        };

        var rows = new[]
        {
            LogsQueryModelFactory.LogsTableRow(columns, [3.14]),
        };

        var table = LogsQueryModelFactory.LogsTable("PrimaryResult", columns, rows);

        // Act
        var result = table.ReadObjects<DoubleRecord>();

        // Assert
        result.Should().ContainSingle();
        result[0].Value.Should().BeApproximately(3.14, 0.001);
    }

    [Fact]
    public void ReadObjects_JsonElementValue_DeserializesCorrectly()
    {
        // Arrange
        var jsonElement = JsonDocument.Parse("\"hello\"").RootElement.Clone();

        var columns = new[]
        {
            LogsQueryModelFactory.LogsTableColumn("Name", LogsColumnType.String),
            LogsQueryModelFactory.LogsTableColumn("Description", LogsColumnType.String),
        };

        var rows = new[]
        {
            LogsQueryModelFactory.LogsTableRow(columns, ["Alice", jsonElement]),
        };

        var table = LogsQueryModelFactory.LogsTable("PrimaryResult", columns, rows);

        // Act
        var result = table.ReadObjects<NullableRecord>();

        // Assert
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Alice");
        result[0].Description.Should().Be("hello");
    }

    private sealed record SimpleRecord(
        string Name,
        int Count);

    private sealed record NameRecord(string Name);

    private sealed record NullableRecord(
        string Name,
        string? Description);

    private sealed record LongRecord(long Value);

    private sealed record DoubleRecord(double Value);
}