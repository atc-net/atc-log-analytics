namespace Atc.LogAnalytics.Tests.Extensions;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("PascalCase", "pascalCase")]
    [InlineData("AlreadyCamelCase", "alreadyCamelCase")]
    [InlineData("A", "a")]
    [InlineData("ABC", "aBC")]
    [InlineData("WorkspaceId", "workspaceId")]
    [InlineData("TopCount", "topCount")]
    public void ToCamelCase_Should_Convert_First_Char_To_Lowercase(
        string input,
        string expected)
    {
        // Act
        var result = input.ToCamelCase();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ToCamelCase_Should_Handle_Empty_Or_Null_String(string? input)
    {
        // Act
        var result = input.ToCamelCase();

        // Assert
        result.Should().Be(input);
    }
}