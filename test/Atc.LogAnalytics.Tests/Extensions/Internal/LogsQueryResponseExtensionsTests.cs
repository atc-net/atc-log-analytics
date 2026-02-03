namespace Atc.LogAnalytics.Tests.Extensions.Internal;

public sealed class LogsQueryResponseExtensionsTests
{
    [Fact]
    public void ToResults_FailureStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var response = CreateResponse(
            LogsQueryResultStatus.Failure,
            [],
            "Query timed out");

        // Act
        var act = () => response.ToResults<TestRecord>();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*Query timed out*");
    }

    [Fact]
    public void ToResults_PartialFailure_ReturnsResults()
    {
        // Arrange
        var response = CreateResponse(
            LogsQueryResultStatus.PartialFailure,
            [["Alice", 42]],
            "Partial error");

        // Act
        var result = response.ToResults<TestRecord>();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainSingle();
        result![0].Name.Should().Be("Alice");
        result[0].Value.Should().Be(42);
    }

    [Fact]
    public void ToResults_SuccessEmptyTable_ReturnsNull()
    {
        // Arrange
        var response = CreateResponse(LogsQueryResultStatus.Success, []);

        // Act
        var result = response.ToResults<TestRecord>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ToResults_SuccessWithRows_ReturnsDeserializedArray()
    {
        // Arrange
        var response = CreateResponse(
            LogsQueryResultStatus.Success,
            [["Alice", 1], ["Bob", 2]]);

        // Act
        var result = response.ToResults<TestRecord>();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Name.Should().Be("Alice");
        result[1].Name.Should().Be("Bob");
    }

    private static Response<LogsQueryResult> CreateResponse(
        LogsQueryResultStatus status,
        object[][] rowValues,
        string? errorMessage = null)
    {
        var columns = new[]
        {
            LogsQueryModelFactory.LogsTableColumn("Name", LogsColumnType.String),
            LogsQueryModelFactory.LogsTableColumn("Value", LogsColumnType.Int),
        };

        var rows = rowValues
            .Select(r => LogsQueryModelFactory.LogsTableRow(columns, r))
            .ToArray();

        var table = LogsQueryModelFactory.LogsTable("PrimaryResult", columns, rows);

        var errorJson = errorMessage is not null
            ? $"{{\"code\":\"Error\",\"message\":\"{errorMessage}\"}}"
            : "{}";

        var empty = BinaryData.FromString("{}");
        var queryResult = LogsQueryModelFactory.LogsQueryResult(
            [table],
            BinaryData.FromString(errorJson),
            empty,
            empty);

        // The factory always sets Status to Success; use reflection to override
        if (status != LogsQueryResultStatus.Success)
        {
            typeof(LogsQueryResult)
                .GetField("<Status>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(queryResult, status);
        }

        return Response.FromValue(queryResult, Substitute.For<Response>());
    }

    private sealed record TestRecord(
        string Name,
        int Value);
}