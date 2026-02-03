namespace Atc.LogAnalytics.Serialization;

/// <summary>
/// Custom JSON converter for boolean values that handles Kusto's behavior of returning boolean expressions as numeric types.
/// </summary>
/// <remarks>
/// Kusto's tobool(...) function and boolean expressions return sbyte (0 or 1) to .NET instead of true boolean values.
/// This converter ensures that boolean DTO properties can correctly deserialize from both:
/// <list type="bullet">
///   <item><description>Standard JSON boolean tokens (true/false)</description></item>
///   <item><description>Numeric tokens where 0 = false and any non-zero value = true</description></item>
/// </list>
/// This is necessary because the Kusto SDK returns numeric values for boolean columns in query results.
/// </remarks>
internal sealed class LogAnalyticsBooleanJsonConverter : JsonConverter<bool>
{
    /// <inheritdoc />
    public override bool Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => reader.GetInt32() != 0,
            _ => throw new JsonException($"Cannot convert {reader.TokenType} to boolean"),
        };

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        bool value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteBooleanValue(value);
    }
}