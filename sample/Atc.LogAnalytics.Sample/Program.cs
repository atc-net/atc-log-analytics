// Build the host
var builder = Host.CreateApplicationBuilder(args);

// Configure Log Analytics
// Replace with your actual workspace ID or set via configuration
var workspaceId = Environment.GetEnvironmentVariable("LOG_ANALYTICS_WORKSPACE_ID")
    ?? "xxx";

builder.Services.ConfigureLogAnalytics(options =>
{
    options.WorkspaceId = workspaceId;
    options.Credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        TenantId = "xxx",
    });
});

var host = builder.Build();

// Get the processor and execute a query
var processor = host.Services.GetRequiredService<ILogAnalyticsProcessor>();

Console.WriteLine("Atc.LogAnalytics Sample Application");
Console.WriteLine("===================================");
Console.WriteLine();

try
{
    Console.WriteLine("Executing HeartbeatQuery (last 24 hours)...");
    Console.WriteLine();

    var query = new HeartbeatQuery();
    var queryOptions = new AtcLogAnalyticsQueryOptions
    {
        TimeRange = new LogsQueryTimeRange(TimeSpan.FromHours(24)),
        ServerTimeout = TimeSpan.FromMinutes(2),
    };

    var results = await processor.ExecuteWorkspaceQuery(query, queryOptions);

    if (results is null || results.Length == 0)
    {
        Console.WriteLine("No results found.");
    }
    else
    {
        Console.WriteLine($"Found {results.Length} heartbeat records:");
        Console.WriteLine();

        foreach (var record in results)
        {
            Console.WriteLine($"  Computer: {record.Computer}");
            Console.WriteLine($"  OS Type:  {record.OSType}");
            Console.WriteLine($"  Version:  {record.Version}");
            Console.WriteLine($"  Time:     {record.TimeGenerated:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();
        }
    }
}
catch (InvalidOperationException ex) when (ex.Message.Contains("WorkspaceId", StringComparison.Ordinal))
{
    Console.WriteLine("Error: WorkspaceId is not configured.");
    Console.WriteLine("Please set the LOG_ANALYTICS_WORKSPACE_ID environment variable.");
}
catch (Exception ex) when (ex is Azure.RequestFailedException or AuthenticationFailedException)
{
    Console.WriteLine($"Error executing query: {ex.Message}");
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();