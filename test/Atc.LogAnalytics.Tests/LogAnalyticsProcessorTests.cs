namespace Atc.LogAnalytics.Tests;

public sealed class LogAnalyticsProcessorTests
{
    private readonly IScriptHandlerFactory scriptHandlerFactory;
    private readonly LogAnalyticsProcessor sut;

    public LogAnalyticsProcessorTests()
    {
        scriptHandlerFactory = Substitute.For<IScriptHandlerFactory>();
        sut = new LogAnalyticsProcessor(scriptHandlerFactory, connectionName: null);
    }

    [Fact]
    public Task ExecuteWorkspaceQuery_NullQuery_ThrowsArgumentNullException()
    {
        // Act
        var act = () => sut.ExecuteWorkspaceQuery<TestRecord>(null!);

        // Assert
        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteWorkspaceQuery_ValidQuery_DelegatesToHandler()
    {
        // Arrange
        var query = Substitute.For<ILogAnalyticsQuery<TestRecord>>();
        var handler = Substitute.For<IWorkspaceQueryHandler<TestRecord>>();
        var expected = new[] { new TestRecord("Test", 42) };

        scriptHandlerFactory.CreateWorkspaceQueryHandler<TestRecord>(null).Returns(handler);
        handler.ExecuteAsync(query, null, Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var result = await sut.ExecuteWorkspaceQuery(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(expected);
        scriptHandlerFactory.Received(1).CreateWorkspaceQueryHandler<TestRecord>(null);
        await handler.Received(1).ExecuteAsync(
            query,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkspaceQuery_WithOptions_PassesOptionsToHandler()
    {
        // Arrange
        var query = Substitute.For<ILogAnalyticsQuery<TestRecord>>();
        var handler = Substitute.For<IWorkspaceQueryHandler<TestRecord>>();
        var queryOptions = new AtcLogAnalyticsQueryOptions
        {
            ServerTimeout = TimeSpan.FromMinutes(5),
        };

        scriptHandlerFactory.CreateWorkspaceQueryHandler<TestRecord>(null).Returns(handler);

        // Act
        await sut.ExecuteWorkspaceQuery(query, queryOptions, TestContext.Current.CancellationToken);

        // Assert
        await handler.Received(1).ExecuteAsync(
            query,
            queryOptions,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteWorkspaceQuery_WithConnectionName_PassesConnectionNameToFactory()
    {
        // Arrange
        var processor = new LogAnalyticsProcessor(scriptHandlerFactory, "MyConnection");
        var query = Substitute.For<ILogAnalyticsQuery<TestRecord>>();
        var handler = Substitute.For<IWorkspaceQueryHandler<TestRecord>>();

        scriptHandlerFactory.CreateWorkspaceQueryHandler<TestRecord>("MyConnection").Returns(handler);

        // Act
        await processor.ExecuteWorkspaceQuery(query, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        scriptHandlerFactory.Received(1).CreateWorkspaceQueryHandler<TestRecord>("MyConnection");
    }

    [Fact]
    public Task ExecuteResourceQuery_NullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var resourceId = new Azure.Core.ResourceIdentifier("/subscriptions/sub-id/resourceGroups/rg");

        // Act
        var act = () => sut.ExecuteResourceQuery<TestRecord>(null!, resourceId);

        // Assert
        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public Task ExecuteResourceQuery_NullResourceId_ThrowsArgumentNullException()
    {
        // Arrange
        var query = Substitute.For<ILogAnalyticsQuery<TestRecord>>();

        // Act
        var act = () => sut.ExecuteResourceQuery(query, null!);

        // Assert
        return act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteResourceQuery_ValidQuery_DelegatesToHandler()
    {
        // Arrange
        var query = Substitute.For<ILogAnalyticsQuery<TestRecord>>();
        var handler = Substitute.For<IResourceQueryHandler<TestRecord>>();
        var resourceId = new Azure.Core.ResourceIdentifier("/subscriptions/sub-id/resourceGroups/rg");
        var expected = new[] { new TestRecord("Test", 42) };

        scriptHandlerFactory.CreateResourceQueryHandler<TestRecord>(null).Returns(handler);
        handler.ExecuteAsync(query, resourceId, null, Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var result = await sut.ExecuteResourceQuery(query, resourceId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeSameAs(expected);
        scriptHandlerFactory.Received(1).CreateResourceQueryHandler<TestRecord>(null);
        await handler.Received(1).ExecuteAsync(
            query,
            resourceId,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteResourceQuery_WithOptions_PassesOptionsToHandler()
    {
        // Arrange
        var query = Substitute.For<ILogAnalyticsQuery<TestRecord>>();
        var handler = Substitute.For<IResourceQueryHandler<TestRecord>>();
        var resourceId = new Azure.Core.ResourceIdentifier("/subscriptions/sub-id/resourceGroups/rg");
        var queryOptions = new AtcLogAnalyticsQueryOptions
        {
            ServerTimeout = TimeSpan.FromMinutes(5),
        };

        scriptHandlerFactory.CreateResourceQueryHandler<TestRecord>(null).Returns(handler);

        // Act
        await sut.ExecuteResourceQuery(query, resourceId, queryOptions, TestContext.Current.CancellationToken);

        // Assert
        await handler.Received(1).ExecuteAsync(
            query,
            resourceId,
            queryOptions,
            Arg.Any<CancellationToken>());
    }

    internal sealed record TestRecord(string Name, int Value);
}