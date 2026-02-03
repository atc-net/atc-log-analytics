namespace Atc.LogAnalytics;

/// <summary>
/// Abstract base record for Log Analytics queries that return typed results.
/// </summary>
/// <typeparam name="T">The type of the query result.</typeparam>
public abstract record LogAnalyticsQuery<T> : LogAnalyticsScript, ILogAnalyticsQuery<T>
    where T : class
{
}