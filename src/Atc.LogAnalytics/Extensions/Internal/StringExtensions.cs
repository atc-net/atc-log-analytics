namespace Atc.LogAnalytics.Extensions.Internal;

/// <summary>
/// String manipulation extension methods.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Converts a PascalCase or camelCase string to camelCase.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The camelCase version of the string.</returns>
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (value.Length == 1)
        {
            return value.ToLowerInvariant();
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}