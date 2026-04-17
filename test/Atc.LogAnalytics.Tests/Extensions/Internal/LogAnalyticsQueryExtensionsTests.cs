namespace Atc.LogAnalytics.Tests.Extensions.Internal;

public sealed class LogAnalyticsQueryExtensionsTests
{
    [Theory]
    [InlineData("123abc")]
    [InlineData("valid_key; drop table Users")]
    public void ToKql_InvalidKey_ThrowsArgumentException(string key)
    {
        // Arrange
        var query = new StubQuery(
            "T",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                [key] = "value",
            });

        // Act
        var act = () => query.ToKql();

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToKql_NoParameters_Returns_Script()
    {
        // Arrange
        var query = new StubQuery(
            "Heartbeat | take 10",
            new Dictionary<string, object?>(StringComparer.Ordinal));

        // Act
        var result = query.ToKql();

        // Assert
        result.Should().Be("Heartbeat | take 10");
    }

    [Theory]
    [InlineData("validKey")]
    [InlineData("_private")]
    [InlineData("camelCase123")]
    [InlineData("A")]
    public void ToKql_ValidKeys_DoNotThrow(string key)
    {
        // Arrange
        var query = new StubQuery(
            "T",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                [key] = "value",
            });

        // Act
        var act = () => query.ToKql();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ToKql_WithParameters_Prepends_LetStatements()
    {
        // Arrange
        var query = new StubQuery(
            "Heartbeat | take topCount",
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["computerName"] = "mypc",
                ["topCount"] = 50,
            });

        // Act
        var result = query.ToKql();

        // Assert
        result.Should().Contain("let computerName = \"mypc\";");
        result.Should().Contain("let topCount = 50;");
        result.Should().EndWith("Heartbeat | take topCount");
    }

    [Fact]
    public void ToKql_WithDeclareQueryParameters_StripsDeclareBeforePrependingLets()
    {
        // Arrange — a script using the standard KQL "declare query_parameters" convention.
        // The library must strip that block before prepending its own let statements, otherwise
        // Kusto rejects the query with "Let with the same name was already used in current context".
        var script =
            """
            declare query_parameters (
                computer:string = "",
                topCount:int = 10
            );
            Heartbeat
            | where isempty(computer) or Computer == computer
            | take topCount
            """;
        var query = new StubQuery(
            script,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["computer"] = "mypc",
                ["topCount"] = 50,
            });

        // Act
        var result = query.ToKql();

        // Assert
        result.Should().Contain("let computer = \"mypc\";");
        result.Should().Contain("let topCount = 50;");
        result.Should().NotContain("declare query_parameters");
        result.Should().Contain("Heartbeat");
    }

    [Theory]
    [InlineData(
        """
        declare query_parameters (computer:string = "");
        Heartbeat | take 10
        """,
        "Heartbeat | take 10")]
    [InlineData(
        """
        declare query_parameters (
            computer:string = "",
            topCount:int = 10
        );
        Heartbeat | take topCount
        """,
        "Heartbeat | take topCount")]
    [InlineData("   declare query_parameters(a:string=\"\");\nT | take 1", "T | take 1")]
    [InlineData("DECLARE Query_Parameters(a:string=\"\");\nT | take 1", "T | take 1")]
    public void StripDeclareQueryParameters_RemovesLeadingDeclareBlock(
        string input,
        string expectedBody)
    {
        // Act
        var result = LogAnalyticsQueryExtensions.StripDeclareQueryParameters(input);

        // Assert
        result.Should().Be(expectedBody);
    }

    [Fact]
    public void StripDeclareQueryParameters_NoDeclare_ReturnsScriptUnchanged()
    {
        // Arrange
        const string script = "Heartbeat | take 10";

        // Act
        var result = LogAnalyticsQueryExtensions.StripDeclareQueryParameters(script);

        // Assert
        result.Should().Be(script);
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void FormatKqlValue_Bool_Returns_Lowercase(
        bool input,
        string expected)
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatKqlValue_DateOnly_Returns_DatetimeFormat()
    {
        // Arrange
        var d = new DateOnly(2024, 6, 15);

        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(d);

        // Assert
        result.Should().StartWith("datetime(");
        result.Should().EndWith(")");
        result.Should().Contain("2024-06-15");
    }

    [Fact]
    public void FormatKqlValue_DateTime_Returns_DatetimeFormat()
    {
        // Arrange
        var dt = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(dt);

        // Assert
        result.Should().StartWith("datetime(");
        result.Should().EndWith(")");
        result.Should().Contain("2024-06-15");
    }

    [Fact]
    public void FormatKqlValue_DateTimeOffset_Returns_DatetimeFormat_With_Timezone()
    {
        // Arrange
        var dto = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.FromHours(2));

        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(dto);

        // Assert
        result.Should().StartWith("datetime(");
        result.Should().EndWith(")");
        result.Should().Contain("2024-06-15");
        result.Should().Contain("+02:00");
    }

    [Fact]
    public void FormatKqlValue_Decimal_Returns_InvariantNumber()
    {
        // Act — decimal can't be used in InlineData
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(99.99m);

        // Assert
        result.Should().Be("99.99");
    }

    [Fact]
    public void FormatKqlValue_Enum_Returns_StringName()
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(DayOfWeek.Monday);

        // Assert
        result.Should().Be("\"Monday\"");
    }

    [Fact]
    public void FormatKqlValue_Guid_Returns_GuidFormat()
    {
        // Arrange
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789abc");

        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(guid);

        // Assert
        result.Should().Be("guid(12345678-1234-1234-1234-123456789abc)");
    }

    [Fact]
    public void FormatKqlValue_IntArray_Returns_DynamicArray()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };

        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(array);

        // Assert
        result.Should().Be("dynamic([1,2,3])");
    }

    [Fact]
    public void FormatKqlValue_Null_Returns_DynamicNull()
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(null);

        // Assert
        result.Should().Be("dynamic(null)");
    }

    [Theory]
    [InlineData(42, "42")]
    [InlineData(0, "0")]
    [InlineData(-1, "-1")]
    [InlineData(9876543210L, "9876543210")]
    [InlineData(3.14, "3.14")]
    [InlineData(1.5f, "1.5")]
    public void FormatKqlValue_Number_Returns_InvariantString(
        object input,
        string expected)
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatKqlValue_StringList_Returns_DynamicArray()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c" };

        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(list);

        // Assert
        result.Should().Be("dynamic([\"a\",\"b\",\"c\"])");
    }

    [Theory]
    [InlineData("hello", "\"hello\"")]
    [InlineData("", "\"\"")]
    [InlineData("it's a test", "\"it's a test\"")]
    public void FormatKqlValue_String_Returns_Quoted(
        string input,
        string expected)
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatKqlValue_String_With_Backslash_Escapes()
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(@"path\to\file");

        // Assert
        result.Should().Be("""
                           "path\\to\\file"
                           """);
    }

    [Fact]
    public void FormatKqlValue_String_With_BackslashAndQuote_DoubleEscape()
    {
        // Arrange - both backslash and quote in same string
        var input = "path\\to\\\"file\"";

        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(input);

        // Assert - backslashes escaped first, then quotes
        result.Should().Be("\"path\\\\to\\\\\\\"file\\\"\"");
    }

    [Fact]
    public void FormatKqlValue_String_With_CarriageReturn_Escapes()
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue("line1\rline2");

        // Assert
        result.Should().Be(@"""line1\rline2""");
    }

    [Fact]
    public void FormatKqlValue_String_With_Newline_Escapes()
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue("line1\nline2");

        // Assert
        result.Should().Be("""
                           "line1\nline2"
                           """);
    }

    [Fact]
    public void FormatKqlValue_String_With_NullByte_Escapes()
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue("before\0after");

        // Assert
        result.Should().Be("\"before\\0after\"");
    }

    [Fact]
    public void FormatKqlValue_String_With_Quote_Escapes()
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue("say \"hi\"");

        // Assert
        result.Should().Be("""
                           "say \"hi\""
                           """);
    }

    [Fact]
    public void FormatKqlValue_String_With_Tab_Escapes()
    {
        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue("col1\tcol2");

        // Assert
        result.Should().Be("""
                           "col1\tcol2"
                           """);
    }

    [Fact]
    public void FormatKqlValue_String_With_UnicodeLsPs_Escapes()
    {
        // Act
        var resultLs = LogAnalyticsQueryExtensions.FormatKqlValue("a\u2028b");
        var resultPs = LogAnalyticsQueryExtensions.FormatKqlValue("a\u2029b");

        // Assert
        resultLs.Should().Be("\"a\\u2028b\"");
        resultPs.Should().Be("\"a\\u2029b\"");
    }

    [Fact]
    public void FormatKqlValue_TimeSpan_Returns_TimeFormat()
    {
        // Arrange
        var ts = TimeSpan.FromHours(2.5);

        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(ts);

        // Assert
        result.Should().StartWith("time(");
        result.Should().EndWith(")");
    }

    [Fact]
    public void FormatKqlValue_UnknownType_Returns_StringEscaped()
    {
        // Arrange
        var uri = new Uri("https://example.com");

        // Act
        var result = LogAnalyticsQueryExtensions.FormatKqlValue(uri);

        // Assert
        result.Should().Be("\"https://example.com/\"");
    }

    private sealed class StubQuery(
        string script,
        IReadOnlyDictionary<string, object?> parameters)
        : ILogAnalyticsQuery<object>
    {
        public string Script => script;

        public IReadOnlyDictionary<string, object?> Parameters => parameters;
    }
}