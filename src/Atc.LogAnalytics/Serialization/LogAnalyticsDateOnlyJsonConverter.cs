namespace Atc.LogAnalytics.Serialization;

/// <summary>
/// Custom JSON converter for <see cref="DateOnly"/> values that handles Kusto's behavior of returning date values as full datetime strings.
/// </summary>
/// <remarks>
/// Kusto has no native date-only type — functions like <c>startofday()</c> and <c>bin(Timestamp, 1d)</c> still return
/// <c>datetime</c> values (e.g. <c>2024-01-15T00:00:00Z</c>). This converter allows C# result types to use
/// <see cref="DateOnly"/> properties that correctly deserialize from both:
/// <list type="bullet">
///   <item><description>Date-only strings (e.g. <c>"2024-01-15"</c>)</description></item>
///   <item><description>Full datetime strings (e.g. <c>"2024-01-15T00:00:00Z"</c>)</description></item>
/// </list>
/// </remarks>
internal sealed class LogAnalyticsDateOnlyJsonConverter : JsonConverter<DateOnly>
{
    /// <inheritdoc />
    public override DateOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString()
            ?? throw new JsonException(
                $"Unable to convert null to {nameof(DateOnly)}.");

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, out var dateOnly))
        {
            return dateOnly;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, out var dateTimeOffset))
        {
            return DateOnly.FromDateTime(dateTimeOffset.DateTime);
        }

        throw new JsonException(
            $"Unable to convert \"{value}\" to {nameof(DateOnly)}.");
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        DateOnly value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture));
    }
}