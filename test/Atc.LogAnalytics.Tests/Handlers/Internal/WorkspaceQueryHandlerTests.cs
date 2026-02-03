namespace Atc.LogAnalytics.Tests.Handlers.Internal;

public sealed class WorkspaceQueryHandlerTests
{
    private const string WorkspaceId = "test-workspace-id";

    private readonly LogsQueryClient client;
    private readonly WorkspaceQueryHandler<TestRecord> sut;

    public WorkspaceQueryHandlerTests()
    {
        client = Substitute.For<LogsQueryClient>();
        sut = new WorkspaceQueryHandler<TestRecord>(
            NullLoggerFactory.Instance.CreateLogger<WorkspaceQueryHandler<TestRecord>>(),
            client,
            WorkspaceId);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulQuery_ReturnsResults()
    {
        // Arrange
        var query = CreateQuery();
        SetupClientResponse(CreateResponse([["Alice", 42]]));

        // Act
        var result = await sut.ExecuteAsync(
            query,
            options: null,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainSingle();
        result![0].Name.Should().Be("Alice");
        result[0].Value.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyTable_ReturnsNull()
    {
        // Arrange
        var query = CreateQuery();
        SetupClientResponse(CreateResponse([]));

        // Act
        var result = await sut.ExecuteAsync(
            query,
            options: null,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_RequestFailedException_ReturnsNull()
    {
        // Arrange
        var query = CreateQuery();

        client
            .QueryWorkspaceAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LogsQueryTimeRange>(),
                Arg.Any<LogsQueryOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Not found"));

        // Act
        var result = await sut.ExecuteAsync(
            query,
            options: null,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_UnhandledException_ReturnsNull()
    {
        // Arrange
        var query = CreateQuery();

        client
            .QueryWorkspaceAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LogsQueryTimeRange>(),
                Arg.Any<LogsQueryOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Something went wrong"));

        // Act
        var result = await sut.ExecuteAsync(
            query,
            options: null,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    private void SetupClientResponse(Response<LogsQueryResult> response)
        => client
            .QueryWorkspaceAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LogsQueryTimeRange>(),
                Arg.Any<LogsQueryOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

    private static ILogAnalyticsQuery<TestRecord> CreateQuery()
    {
        var query = Substitute.For<ILogAnalyticsQuery<TestRecord>>();
        query.Script.Returns("TestTable | take 10");
        query.Parameters.Returns(new Dictionary<string, object?>(StringComparer.Ordinal));

        return query;
    }

    private static Response<LogsQueryResult> CreateResponse(
        object[][] rowValues)
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
        var empty = BinaryData.FromString("{}");
        var queryResult = LogsQueryModelFactory.LogsQueryResult([table], empty, empty, empty);

        return Response.FromValue(queryResult, Substitute.For<Response>());
    }

    internal sealed record TestRecord(
        string Name,
        int Value);
}