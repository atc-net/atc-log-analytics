namespace Atc.LogAnalytics.Extensions.Internal;

/// <summary>
/// Extension methods for <see cref="ILogAnalyticsQuery{T}"/>.
/// </summary>
internal static partial class LogAnalyticsQueryExtensions
{
    /// <summary>
    /// Renders the query as a KQL string by prepending parameter declarations as <c>let</c> statements.
    /// <para>
    /// Parameters are injected using <c>let key = value;</c> concatenation rather than a native
    /// parameterized query API. This is because <see cref="LogsQueryClient"/> does not expose
    /// a parameter binding mechanism (unlike the Kusto SDK's <c>ClientRequestProperties</c>).
    /// All string values are escaped via <see cref="EscapeKqlString"/> and parameter keys are
    /// validated against <see cref="ValidParameterKeyRegex"/> to prevent injection attacks.
    /// </para>
    /// <para>
    /// If the script begins with a <c>declare query_parameters(...)</c> block, that block is stripped
    /// before the <c>let</c> statements are prepended. This lets authors use the standard KQL
    /// <c>declare query_parameters</c> convention for editor tooling and standalone Azure Portal
    /// testing, without clashing with the injected <c>let</c> bindings at runtime.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the query result.</typeparam>
    /// <param name="query">The query containing script and parameters.</param>
    /// <returns>The KQL script with parameter declarations prepended.</returns>
    internal static string ToKql<T>(this ILogAnalyticsQuery<T> query)
        where T : class
    {
        var parameters = query.Parameters;
        if (parameters.Count == 0)
        {
            return query.Script;
        }

        var sb = new StringBuilder();

        foreach (var param in parameters)
        {
            ValidateParameterKey(param.Key);
            var value = FormatKqlValue(param.Value);
            sb.Append(CultureInfo.InvariantCulture, $"let {param.Key} = {value};");
            sb.AppendLine();
        }

        sb.Append(StripDeclareQueryParameters(query.Script));
        return sb.ToString();
    }

    /// <summary>
    /// Removes a leading <c>declare query_parameters(...)</c> block from the script, if present.
    /// Only strips a block that appears at the start of the script (whitespace allowed), matching
    /// the KQL rule that <c>declare query_parameters</c> must be the first statement in the query.
    /// </summary>
    /// <param name="script">The KQL script to inspect.</param>
    /// <returns>The script with any leading <c>declare query_parameters(...)</c> block removed.</returns>
    internal static string StripDeclareQueryParameters(string script)
        => DeclareQueryParametersRegex().Replace(script, string.Empty);

    internal static string FormatKqlValue(object? value)
        => value switch
        {
            null => "dynamic(null)",
            string s => $"\"{EscapeKqlString(s)}\"",
            bool b => b ? "true" : "false",
            DateOnly d => $"datetime({d:O})",
            DateTime dt => $"datetime({dt:O})",
            DateTimeOffset dto => $"datetime({dto:O})",
            TimeSpan ts => $"time({ts})",
            Guid g => $"guid({g})",
            int or long or float or double or decimal => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "0",
            Enum e => $"\"{EscapeKqlString(Enum.GetName(e.GetType(), e) ?? e.ToString())}\"",
            System.Collections.IEnumerable enumerable => FormatKqlDynamic(enumerable),
            _ => $"\"{EscapeKqlString(value.ToString() ?? string.Empty)}\"",
        };

    private static string FormatKqlDynamic(
        System.Collections.IEnumerable enumerable)
    {
        var items = new List<string>();

        foreach (var item in enumerable)
        {
            items.Add(FormatKqlValue(item));
        }

        return $"dynamic([{string.Join(",", items)}])";
    }

    private static string EscapeKqlString(string value)
        => value
            .Replace("\\", @"\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal)
            .Replace("\0", "\\0", StringComparison.Ordinal)
            .Replace("\u2028", "\\u2028", StringComparison.Ordinal)
            .Replace("\u2029", "\\u2029", StringComparison.Ordinal);

    private static void ValidateParameterKey(string key)
    {
        if (!ValidParameterKeyRegex().IsMatch(key))
        {
            throw new ArgumentException(
                $"Parameter key '{key}' is invalid. Keys must start with a letter or underscore and contain only letters, digits, and underscores.",
                nameof(key));
        }
    }

    [GeneratedRegex("^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ValidParameterKeyRegex();

    [GeneratedRegex(@"\A\s*declare\s+query_parameters\s*\([^)]*\)\s*;\s*", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DeclareQueryParametersRegex();
}