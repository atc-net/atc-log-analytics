namespace Atc.LogAnalytics.Tests.Factories;

public sealed class LogAnalyticsProcessorFactoryTests
{
    [Fact]
    public void Create_Should_Return_New_Processor_Each_Time()
    {
        // Arrange
        var scriptHandlerFactory = Substitute.For<IScriptHandlerFactory>();
        var factory = new LogAnalyticsProcessorFactory(scriptHandlerFactory);

        // Act
        var processor1 = factory.Create("TestConfig");
        var processor2 = factory.Create("TestConfig");

        // Assert
        processor1.Should().NotBeSameAs(processor2);
    }
}