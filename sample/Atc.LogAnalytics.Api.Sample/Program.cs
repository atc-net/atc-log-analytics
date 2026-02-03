var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureLogAnalytics(
    o =>
    {
        o.WorkspaceId = builder.Configuration["LogAnalytics:WorkspaceId"] ?? string.Empty;
        o.Credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            SharedTokenCacheTenantId = builder.Configuration["LogAnalytics:TenantId"],
            VisualStudioTenantId = builder.Configuration["LogAnalytics:TenantId"],
            VisualStudioCodeTenantId = builder.Configuration["LogAnalytics:TenantId"],
            ExcludeManagedIdentityCredential = true,
            ExcludeAzurePowerShellCredential = true,
            ExcludeInteractiveBrowserCredential = true,
            ExcludeEnvironmentCredential = true,
        });
    });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

app.ConfigureScalar();

app.UseHttpsRedirection();

app.MapGet(
        "/heartbeats",
        async (
            [FromQuery] string? computer,
            [FromQuery] int? hours,
            ILogAnalyticsProcessor processor,
            CancellationToken cancellationToken) =>
        {
            var options = hours.HasValue
                ? new AtcLogAnalyticsQueryOptions { TimeRange = new LogsQueryTimeRange(TimeSpan.FromHours(hours.Value)) }
                : null;

            return await processor.ExecuteWorkspaceQuery(
                new HeartbeatQuery(computer),
                options,
                cancellationToken);
        })
    .WithName("GetHeartbeats")
    .WithDescription("Get heartbeat records with optional computer filter and time range");

app.MapGet(
        "/iot-edge-module-logs",
        async (
                ILogAnalyticsProcessor processor,
                CancellationToken cancellationToken)
            => await processor
                .ExecuteWorkspaceQuery(
                    new IotEdgeModuleLogsQuery(),
                    cancellationToken))
    .WithName("GetIotEdgeModuleLogs")
    .WithDescription("Get iot edge module logs");

await app.RunAsync();