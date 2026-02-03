namespace Atc.LogAnalytics.Tests;

public sealed class LogAnalyticsScriptTests
{
    [Fact]
    public void Parameters_Should_Return_Record_Properties_In_CamelCase()
    {
        // Arrange
        var query = new TestQuery("TestValue", 42, true);

        // Act
        var parameters = query.Parameters;

        // Assert
        parameters.Should().ContainKey("stringParam");
        parameters.Should().ContainKey("intParam");
        parameters.Should().ContainKey("boolParam");

        parameters["stringParam"].Should().Be("TestValue");
        parameters["intParam"].Should().Be(42);
        parameters["boolParam"].Should().Be(true);
    }

    [Fact]
    public void Parameters_Should_Handle_Null_Values()
    {
        // Arrange
        var query = new TestQueryWithNullable(null, null);

        // Act
        var parameters = query.Parameters;

        // Assert
        parameters.Should().ContainKey("nullableString");
        parameters.Should().ContainKey("nullableInt");

        parameters["nullableString"].Should().BeNull();
        parameters["nullableInt"].Should().BeNull();
    }

    // Test query class for unit testing
    private sealed record TestQuery(
        string StringParam,
        int IntParam,
        bool BoolParam)
        : LogAnalyticsQuery<TestResult>
    {
        protected override string FileExtension => ".test";
    }

    private sealed record TestQueryWithNullable(
        string? NullableString,
        int? NullableInt)
        : LogAnalyticsQuery<TestResult>
    {
        protected override string FileExtension => ".test";
    }

    private sealed record TestResult(
        string Name,
        int Count);
}