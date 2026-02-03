namespace Atc.LogAnalytics.Tests;

public sealed class LogAnalyticsScriptResourceTests
{
    [Fact]
    public void Script_LoadsFromEmbeddedResource()
    {
        // Arrange
        var script = new TestScript("value");

        // Act
        var kql = script.Script;

        // Assert
        kql.Should().Contain("TestTable");
        kql.Should().Contain("| where Name == name");
    }

    [Fact]
    public void Script_CachesResult()
    {
        // Arrange
        var script1 = new TestScript("a");
        var script2 = new TestScript("b");

        // Act
        var kql1 = script1.Script;
        var kql2 = script2.Script;

        // Assert
        kql1.Should().BeSameAs(kql2);
    }

    [Fact]
    public void Script_MissingResource_ThrowsInvalidOperationException()
    {
        // Arrange
        var script = new MissingResourceScript();

        // Act
        var act = () => script.Script;

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*embedded resource*");
    }

    [Fact]
    public void Parameters_ReturnsRecordProperties()
    {
        // Arrange
        var script = new TestScript("hello");

        // Act
        var parameters = script.Parameters;

        // Assert
        parameters.Should().ContainKey("name");
        parameters["name"].Should().Be("hello");
    }

    [Fact]
    public void Parameters_ExcludesBaseProperties()
    {
        // Arrange
        var script = new TestScript("value");

        // Act
        var parameters = script.Parameters;

        // Assert
        parameters.Should().NotContainKey("Script");
        parameters.Should().NotContainKey("Parameters");
        parameters.Should().NotContainKey("FileExtension");
    }

    private sealed record TestScript(string Name) : LogAnalyticsScript
    {
        protected override string FileExtension => ".kql";
    }

    private sealed record MissingResourceScript() : LogAnalyticsScript
    {
        protected override string FileExtension => ".nonexistent";
    }
}