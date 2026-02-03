namespace Atc.LogAnalytics.Tests.Handlers.Internal;

public sealed class ResourceQueryHandlerTests
{
    private static readonly ResourceIdentifier TestResourceId =
        new("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/test-rg");

    private readonly LogsQueryClient client;
    private readonly ResourceQueryHandler<TestRecord> sut;

    public ResourceQueryHandlerTests()
    {
        client = Substitute.For<LogsQueryClient>();
        sut = new ResourceQueryHandler<TestRecord>(
            NullLoggerFactory.Instance.CreateLogger<ResourceQueryHandler<TestRecord>>(),
            client);
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
            TestResourceId,
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
            TestResourceId,
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
            .QueryResourceAsync(
                Arg.Any<ResourceIdentifier>(),
                Arg.Any<string>(),
                Arg.Any<LogsQueryTimeRange>(),
                Arg.Any<LogsQueryOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Forbidden"));

        // Act
        var result = await sut.ExecuteAsync(
            query,
            TestResourceId,
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
            .QueryResourceAsync(
                Arg.Any<ResourceIdentifier>(),
                Arg.Any<string>(),
                Arg.Any<LogsQueryTimeRange>(),
                Arg.Any<LogsQueryOptions>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Something went wrong"));

        // Act
        var result = await sut.ExecuteAsync(
            query,
            TestResourceId,
            options: null,
            TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    private void SetupClientResponse(Response<LogsQueryResult> response)
        => client
            .QueryResourceAsync(
                Arg.Any<ResourceIdentifier>(),
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