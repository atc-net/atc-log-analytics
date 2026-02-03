namespace Atc.LogAnalytics.Diagnostics;

/// <summary>
/// OpenTelemetry diagnostics constants and activity source for log analytics operations.
/// </summary>
internal static class LogAnalyticsDiagnostics
{
    /// <summary>
    /// The name used when creating the <see cref="ActivitySource"/>.
    /// </summary>
    internal const string ActivitySourceName = "Atc.LogAnalytics";

    /// <summary>
    /// Shared <see cref="ActivitySource"/> for all log analytics operations.
    /// </summary>
    internal static readonly ActivitySource Source = new(ActivitySourceName, "1.0.0");

    /// <summary>
    /// Activity names following OpenTelemetry semantic conventions.
    /// </summary>
    internal static class ActivityNames
    {
        /// <summary>Activity name for workspace queries.</summary>
        internal const string WorkspaceQuery = "loganalytics.query.workspace";

        /// <summary>Activity name for resource queries.</summary>
        internal const string ResourceQuery = "loganalytics.query.resource";
    }

    /// <summary>
    /// Tag names following OpenTelemetry semantic conventions.
    /// </summary>
    internal static class TagNames
    {
        /// <summary>The KQL query statement.</summary>
        internal const string DbStatement = "db.statement";

        /// <summary>The Log Analytics workspace ID.</summary>
        internal const string WorkspaceId = "loganalytics.workspace.id";

        /// <summary>The Azure resource ID.</summary>
        internal const string ResourceId = "loganalytics.resource.id";

        /// <summary>The number of rows returned by the query.</summary>
        internal const string RowCount = "loganalytics.row_count";
    }
}