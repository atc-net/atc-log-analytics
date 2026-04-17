[![NuGet Version](https://img.shields.io/nuget/v/atc.loganalytics.svg?logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/atc.loganalytics)

# Introduction

Atc.LogAnalytics is a .NET library that wraps the `Azure.Monitor.Query.Logs` SDK, providing a structured and type-safe approach to querying Azure Log Analytics workspaces.

## Table of Contents

- [Introduction](#introduction)
  - [Table of Contents](#table-of-contents)
  - [Features](#features)
  - [Getting Started](#getting-started)
    - [Installation](#installation)
    - [Basic Usage](#basic-usage)
    - [Configuring with ServiceCollection Extensions](#configuring-with-servicecollection-extensions)
  - [Script Pattern](#script-pattern)
    - [KQL File Convention](#kql-file-convention)
    - [Parameter Mapping](#parameter-mapping)
    - [Result Mapping](#result-mapping)
  - [Workspace vs Resource Queries](#workspace-vs-resource-queries)
  - [Configuration Options](#configuration-options)
    - [AtcLogAnalyticsOptions](#atcloganalyticsoptions)
    - [AtcLogAnalyticsQueryOptions](#atcloganalyticsqueryoptions)
    - [Named Configurations](#named-configurations)
    - [Cross-Workspace Queries](#cross-workspace-queries)
    - [Custom Endpoints](#custom-endpoints)
  - [Authentication](#authentication)
- [Requirements](#requirements)
- [How to contribute](#how-to-contribute)

## Features

- ✅ **Type-safe queries**: Define strongly-typed query results with C# records
- ✅ **Script abstraction**: Store KQL queries in embedded `.kql` files
- ✅ **Workspace and Resource queries**: Support for both workspace-centric and resource-centric query patterns
- ✅ **Dependency injection**: First-class support for Microsoft.Extensions.DependencyInjection
- ✅ **Named configurations**: Support multiple Log Analytics workspaces with named configurations
- ✅ **Parameter binding**: Automatic mapping from C# record properties to KQL variables

## Getting Started

### Installation

Install the NuGet package:

```bash
dotnet add package Atc.LogAnalytics
```

### Basic Usage

Define a query record, a result record, and a KQL file:

```csharp
using Atc.LogAnalytics;

// Query parameters are defined as record properties
public record HeartbeatQuery(string? ComputerName = null, int TopCount = 100)
    : LogAnalyticsQuery<HeartbeatRecord>;

// Result type matches the columns projected in KQL
public record HeartbeatRecord(
    DateTimeOffset TimeGenerated,
    string Computer,
    string OSType,
    string Version);
```

Create a file named `HeartbeatQuery.kql` in the same folder as your query class. Mark it as an embedded resource in your `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Queries\**\*.kql" />
</ItemGroup>
```

```kql
// Parameters from the C# record are available as KQL variables (camelCase)
let computerFilter = iif(isempty(computerName), true, Computer == computerName);
Heartbeat
| where computerFilter
| project TimeGenerated, Computer, OSType, Version
| top topCount by TimeGenerated desc
```

### Configuring with ServiceCollection Extensions

Register Log Analytics services and inject `ILogAnalyticsProcessorFactory` to execute queries:

```csharp
using Atc.LogAnalytics.Extensions;
using Azure.Identity;

builder.Services.ConfigureLogAnalytics(options =>
{
    options.WorkspaceId = builder.Configuration["LogAnalytics:WorkspaceId"]!;
    options.Credential = new DefaultAzureCredential();
});
```

```csharp
public class HeartbeatService
{
    private readonly ILogAnalyticsProcessorFactory processorFactory;

    public HeartbeatService(ILogAnalyticsProcessorFactory processorFactory)
    {
        this.processorFactory = processorFactory;
    }

    public async Task<HeartbeatRecord[]?> GetRecentHeartbeatsAsync(
        string? computerName = null,
        CancellationToken ct = default)
    {
        var processor = processorFactory.Create();
        var query = new HeartbeatQuery(ComputerName: computerName, TopCount: 50);

        return await processor.ExecuteWorkspaceQuery(query, ct);
    }
}
```

## Script Pattern

Queries are defined as C# records with embedded KQL files:

```
ILogAnalyticsScript
├── LogAnalyticsScript (abstract record, loads .kql files)
    └── LogAnalyticsQuery<T> (returns T[])
```

### KQL File Convention

- File name must match the query class name: `MyQuery.kql`
- File must be an embedded resource
- File must be in the same namespace/folder as the query class
- Parameters are referenced in KQL using camelCase naming

#### `declare query_parameters` is supported (and optional)

Azure Log Analytics does not expose a native parameter-binding API on its SDK. To work around that, Atc.LogAnalytics prepends the parameters from your record as `let` statements at runtime (e.g. `let computerName = "mypc";`) before sending the query.

You may *optionally* prefix your `.kql` file with a standard KQL `declare query_parameters(...)` block — the library strips that block before prepending the `let` statements, so there is no conflict. Doing so is useful because:

- IDE/editor tooling (VS Code KQL extension, Kusto Explorer) recognises the parameters.
- The file is standalone-runnable in the Azure Portal's Log Analytics query editor using the declared defaults.

> Note: only a `declare query_parameters(...)` block at the **very start** of the script is stripped (leading whitespace is allowed). A `declare` statement further down the script is left untouched.

Example:

```kql
declare query_parameters (
    computerName:string = "",
    topCount:int = 10
);
Heartbeat
| where isempty(computerName) or Computer == computerName
| top topCount by TimeGenerated desc
| project TimeGenerated, Computer, OSType, Version
```

At runtime, the `declare` block is stripped and the library injects `let computerName = "..."; let topCount = ...;` from the query record's properties.

### Parameter Mapping

| C# Type | KQL Type | Example |
|---------|----------|---------|
| `string` | `string` | `"value"` |
| `null` | `dynamic` | `dynamic(null)` |
| `int`, `long` | `int`, `long` | `100` |
| `bool` | `bool` | `true`, `false` |
| `DateTime`, `DateTimeOffset` | `datetime` | `datetime(2024-01-01)` |
| `TimeSpan` | `timespan` | `1h`, `30m` |
| `Guid` | `guid` | `guid(...)` |
| `Enum` | `string` | `"Warning"` |
| `IEnumerable`, arrays | `dynamic` | `dynamic(["a","b"])` |

### Result Mapping

Column names in your KQL `project` statement should match the property names in your result record (case-insensitive):

```kql
| project TimeGenerated, Computer, OSType, Version
```

```csharp
public record HeartbeatRecord(
    DateTimeOffset TimeGenerated,
    string Computer,
    string OSType,
    string Version);
```

## Workspace vs Resource Queries

Azure Log Analytics supports two query patterns:

**Workspace Query** — Targets a Log Analytics Workspace by its ID (GUID). All resources that send logs to the workspace are queryable. Best for cross-resource analysis, dashboards, and aggregated reporting.

```csharp
var result = await processor.ExecuteWorkspaceQuery(
    new HeartbeatQuery(),
    cancellationToken);
```

**Resource Query** — Targets a specific Azure resource by its ARM resource ID. Only logs from that specific resource are returned. Best for resource-specific troubleshooting and isolated analysis.

```csharp
var resourceId = new ResourceIdentifier(
    "/subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Compute/virtualMachines/{vm}");

var result = await processor.ExecuteResourceQuery(
    new HeartbeatQuery(),
    resourceId,
    cancellationToken);
```

| Scenario | Recommended Pattern |
|----------|---------------------|
| Dashboard showing all VM health | Workspace query |
| Troubleshooting a specific VM issue | Resource query |
| Aggregating logs across multiple apps | Workspace query |
| Analyzing one App Service's exceptions | Resource query |
| Cross-workspace queries | Workspace query with `AdditionalWorkspaces` query option |

## Configuration Options

### AtcLogAnalyticsOptions

| Property | Type | Description |
|----------|------|-------------|
| `WorkspaceId` | `string` | The Log Analytics workspace ID (GUID). Required for workspace queries. |
| `Credential` | `TokenCredential` | Azure credential for authentication. Required. |
| `Endpoint` | `Uri?` | Custom endpoint URI. Defaults to Azure public cloud. |

### AtcLogAnalyticsQueryOptions

Per-query options that can be passed when executing a query.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `TimeRange` | `LogsQueryTimeRange?` | `null` | Time range for the query. If null, query must specify time filter. |
| `ServerTimeout` | `TimeSpan?` | `null` | Server-side query timeout. |
| `AdditionalWorkspaces` | `IList<string>?` | `null` | Additional workspaces for this specific query. |

```csharp
var options = new AtcLogAnalyticsQueryOptions
{
    TimeRange = new LogsQueryTimeRange(TimeSpan.FromDays(7)),
    ServerTimeout = TimeSpan.FromMinutes(5),
};

var result = await processor.ExecuteWorkspaceQuery(query, options, ct);
```

### Named Configurations

Register multiple workspaces with named configurations:

```csharp
// Production workspace
services.ConfigureLogAnalytics(options =>
{
    options.WorkspaceId = "prod-workspace-guid";
    options.Credential = new DefaultAzureCredential();
}, configurationName: "Production");

// Development workspace
services.ConfigureLogAnalytics(options =>
{
    options.WorkspaceId = "dev-workspace-guid";
    options.Credential = new DefaultAzureCredential();
}, configurationName: "Development");
```

```csharp
// Use a named configuration
var processor = processorFactory.Create("Production");
```

### Cross-Workspace Queries

Cross-workspace queries are configured per-query via `AtcLogAnalyticsQueryOptions`:

```csharp
var queryOptions = new AtcLogAnalyticsQueryOptions
{
    AdditionalWorkspaces = new List<string> { "secondary-workspace" }
};

var result = await processor.ExecuteWorkspaceQuery(query, queryOptions, ct);
```

### Custom Endpoints

For Azure Government, China, or private link scenarios:

```csharp
services.ConfigureLogAnalytics(options =>
{
    options.WorkspaceId = "your-workspace-guid";
    options.Credential = new DefaultAzureCredential();

    // Azure Government
    options.Endpoint = new Uri("https://api.loganalytics.us/v1");
});
```

## Authentication

The library uses `Azure.Identity` for authentication. The recommended approach is `DefaultAzureCredential`, which automatically tries multiple authentication methods.

For local development, sign in with Azure CLI:

```bash
az login
az account set --subscription "your-subscription-id"
```

For production, use Managed Identity:

```csharp
// System-assigned managed identity
options.Credential = new ManagedIdentityCredential();

// User-assigned managed identity
options.Credential = new ManagedIdentityCredential("client-id");
```

# Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

# How to contribute

[Contribution Guidelines](https://atc-net.github.io/introduction/about-atc#how-to-contribute)

[Coding Guidelines](https://atc-net.github.io/introduction/about-atc#coding-guidelines)