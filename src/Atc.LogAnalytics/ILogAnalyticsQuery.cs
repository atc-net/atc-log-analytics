namespace Atc.LogAnalytics;

/// <summary>
/// Represents a Log Analytics query that returns typed results.
/// </summary>
/// <typeparam name="T">The type of the query result.</typeparam>
#pragma warning disable S2326 // Type parameter is used for type inference at compile time
public interface ILogAnalyticsQuery<T> : ILogAnalyticsScript
#pragma warning restore S2326
    where T : class
{
}