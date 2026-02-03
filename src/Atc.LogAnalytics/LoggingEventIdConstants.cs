namespace Atc.LogAnalytics;

/// <summary>
/// Structured event IDs for logging across the library.
/// </summary>
internal static class LoggingEventIdConstants
{
    internal static class WorkspaceQueryHandler
    {
        internal const int Failed = 10_000;
        internal const int UnhandledException = 10_001;
    }

    internal static class ResourceQueryHandler
    {
        internal const int Failed = 20_000;
        internal const int UnhandledException = 20_001;
    }
}