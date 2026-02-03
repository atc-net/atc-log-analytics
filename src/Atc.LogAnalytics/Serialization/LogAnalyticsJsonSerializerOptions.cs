namespace Atc.LogAnalytics.Serialization;

/// <summary>
/// Provides shared JSON serializer options for consistent Log Analytics data deserialization across the library.
/// </summary>
internal static class LogAnalyticsJsonSerializerOptions
{
    /// <summary>
    /// Gets the default JSON serializer options configured for Log Analytics data deserialization.
    /// Includes enum string conversion, case-insensitive property matching, and support for reading numbers from strings.
    /// </summary>
    public static JsonSerializerOptions Default { get; } = new()
    {
        Converters = { new JsonStringEnumConverter(), new LogAnalyticsBooleanJsonConverter(), new LogAnalyticsDateOnlyJsonConverter() },
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };
}