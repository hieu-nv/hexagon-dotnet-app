# A Comprehensive Guide to Setting Up Datadog Logging in .NET 10 Applications

![Datadog + .NET](https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=1200&h=400&fit=crop)

Effective logging is the backbone of observability in modern applications. When your application runs in production, logs are often the first place you look when something goes wrong. But what if your logs could do more than just tell you *what* happened? What if they could show you the *entire journey* of a request through your system?

In this comprehensive guide, I'll walk you through setting up **Datadog logging** in a .NET 10 application with full **trace correlation**—a powerful feature that allows you to see logs in the context of distributed traces, making debugging exponentially easier.

## What We'll Build

By the end of this guide, you'll have:

- ✅ Structured JSON logging with Serilog
- ✅ Multiple log destinations (console, file, Datadog cloud)
- ✅ Automatic log-trace correlation using OpenTelemetry
- ✅ Local development setup with Datadog agent
- ✅ Production-ready configuration patterns

## Why Datadog + Serilog + OpenTelemetry?

Before diving into implementation, let's understand why this stack is powerful:

**Serilog** provides structured logging with a clean, fluent API. Unlike traditional string-based logging, structured logs are queryable, filterable, and machine-readable.

**Datadog** offers a unified platform for logs, traces, and metrics. Its APM (Application Performance Monitoring) allows you to see logs within trace spans, giving you complete context.

**OpenTelemetry** is the industry standard for distributed tracing. By enriching logs with trace IDs, we can correlate logs with traces seamlessly.

## Architecture Overview

Here's how the pieces fit together:

```
┌────────────────────────┐
│   .NET Application                             │
│   (ASP.NET Core)                               │
└───────┬────────────────┘
                │
                ├─── Console Sink ──────► Terminal (Development)
                │
                ├─── File Sink ────────► logs/app.log (Local)
                │
                └─── Datadog Sink ─────► Datadog Cloud (Production)
                                            │
                                            ▼
                         ┌──────────────────┐
                         │  Datadog Platform                  │
                         │  - Logs Explorer                   │
                         │  - APM Traces                      │
                         │  - Correlation                     │
                         └──────────────────┘
```

For local development, you can optionally run a **Datadog agent** that acts as a local proxy:

```
Application → Local Agent → Datadog Cloud
```

This provides buffering, retry logic, and offline development capability.

## Prerequisites

Before we begin, ensure you have:

1. **.NET 10 SDK** installed
2. **Datadog account** (free trial available at [datadoghq.com](https://www.datadoghq.com/))
3. **Datadog API key** (from Organization Settings → API Keys)
4. **Docker or Podman** (optional, for local agent)

## Step 1: Install Required NuGet Packages

Add these packages to your ASP.NET Core project:

```bash
dotnet add package Serilog.AspNetCore --version 10.0.0
dotnet add package Serilog.Sinks.Datadog.Logs --version 0.6.0
dotnet add package Serilog.Sinks.File --version 7.0.0
dotnet add package Serilog.Enrichers.Span --version 3.1.0
dotnet add package DotNetEnv --version 3.2.0
```

**What each package does:**

- `Serilog.AspNetCore`: Core Serilog integration for ASP.NET Core
- `Serilog.Sinks.Datadog.Logs`: Direct HTTP logging to Datadog cloud
- `Serilog.Sinks.File`: File-based logging with rolling files
- `Serilog.Enrichers.Span`: Extracts OpenTelemetry trace context
- `DotNetEnv`: Load environment variables from `.env` files

## Step 2: Create the OpenTelemetry Trace Enricher

This is the secret sauce that enables log-trace correlation. Create a new file `Logging/OpenTelemetryTraceEnricher.cs`:

```csharp
using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace App.Api.Logging;

/// <summary>
/// Serilog enricher that adds OpenTelemetry trace context to log events.
/// This enables correlation between logs and APM traces in Datadog.
/// </summary>
internal sealed class OpenTelemetryTraceEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity == null)
        {
            return;
        }

        // Add W3C trace IDs (hex format)
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString())
        );
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString())
        );

        // Add Datadog-specific trace IDs (decimal format)
        // Datadog expects the lower 64 bits of the trace ID as a decimal number
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty(
                "dd.trace_id",
                Convert.ToUInt64(activity.TraceId.ToString().Substring(16), 16).ToString()
            )
        );
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty(
                "dd.span_id",
                Convert.ToUInt64(activity.SpanId.ToString(), 16).ToString()
            )
        );
    }
}
```

**Why this matters:** OpenTelemetry uses hexadecimal trace IDs (W3C format), but Datadog expects decimal trace IDs. This enricher converts between formats, enabling seamless correlation.

## Step 3: Configure Serilog in Program.cs

Now let's wire everything together in `Program.cs`:

```csharp
using App.Api.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Datadog.Logs;

// Load environment variables from .env file
DotNetEnv.Env.Load();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog with multiple sinks
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.With(new OpenTelemetryTraceEnricher()) // 🔥 This enables trace correlation
    .Enrich.WithProperty("Application", "App.Api")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithProperty("service", "hexagon-dotnet-app")
    .WriteTo.Console(new JsonFormatter(renderMessage: true))
    .WriteTo.File(
        new JsonFormatter(renderMessage: true),
        path: "logs/app.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1)
    )
    .WriteTo.DatadogLogs(
        apiKey: Environment.GetEnvironmentVariable("DD_API_KEY"),
        source: "csharp",
        service: "hexagon-dotnet-app",
        host: Environment.MachineName,
        tags: new[] { 
            $"env:{builder.Environment.EnvironmentName}", 
            "version:1.0.0" 
        },
        configuration: new DatadogConfiguration { 
            Url = "https://http-intake.logs.us5.datadoghq.com" 
        }
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Rest of your application setup...
WebApplication app = builder.Build();

try
{
    Log.Information("Starting application");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
```

## Step 4: Configure Environment Variables

Create a `.env` file in your project root:

```bash
# Datadog Configuration
DD_API_KEY=your_datadog_api_key_here
DD_SITE=us5.datadoghq.com
DD_ENV=development
DD_SERVICE=hexagon-dotnet-app
DD_VERSION=1.0.0

# OpenTelemetry Configuration (for traces)
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
OTEL_SERVICE_NAME=hexagon-dotnet-app
```

⚠️ **Security Note:** Never commit `.env` files with real API keys to version control! Add `.env` to your `.gitignore`:

```bash
echo ".env" >> .gitignore
```

## Step 5: Understanding Log Output

With this configuration, each log entry becomes a rich JSON object:

```json
{
  "@t": "2026-02-25T10:30:45.1234567Z",
  "@mt": "Processing request for {Endpoint}",
  "@l": "Information",
  "Endpoint": "/api/v1/todos",
  "Application": "App.Api",
  "Environment": "Development",
  "service": "hexagon-dotnet-app",
  "TraceId": "0af7651916cd43dd8448eb211c80319c",
  "SpanId": "a1b2c3d4e5f6g7h8",
  "dd.trace_id": "9553762204513133084",
  "dd.span_id": "11644864782168674360"
}
```

Notice the four trace-related fields:
- `TraceId`, `SpanId`: OpenTelemetry format (hex)
- `dd.trace_id`, `dd.span_id`: Datadog format (decimal)

These fields enable Datadog to link logs with traces automatically.

## Step 6: Setting Up Local Datadog Agent (Optional)

For local development, running a Datadog agent provides several benefits:

- 📦 **Buffering:** Logs are queued locally, reducing network calls
- 🔄 **Retry logic:** Automatic retries if Datadog cloud is unreachable
- 🚀 **Performance:** Offloads HTTP requests from your app
- 💻 **Offline dev:** Continue logging even without internet

Create a script `scripts/datadog-agent.sh`:

```bash
#!/bin/bash

# Start Datadog agent with OpenTelemetry support
docker run -d \
  --name dd-agent \
  -e DD_API_KEY="${DD_API_KEY}" \
  -e DD_SITE="${DD_SITE:-us5.datadoghq.com}" \
  -e DD_APM_ENABLED=true \
  -e DD_LOGS_ENABLED=true \
  -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_HTTP_ENDPOINT=0.0.0.0:4318 \
  -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT=0.0.0.0:4317 \
  -p 4318:4318 \
  -p 4317:4317 \
  -p 8126:8126 \
  -p 8125:8125/udp \
  datadog/agent:latest
```

Run it:

```bash
chmod +x scripts/datadog-agent.sh
./scripts/datadog-agent.sh
```

## Step 7: Testing Your Setup

### Generate Test Logs

Create a simple endpoint to test logging:

```csharp
app.MapGet("/test-logging", (ILogger<Program> logger) =>
{
    logger.LogInformation("This is an info log");
    logger.LogWarning("This is a warning log");
    logger.LogError("This is an error log");
    
    return Results.Ok(new { message = "Logs sent!" });
});
```

### Call the Endpoint

```bash
curl http://localhost:5112/test-logging
```

### Verify Logs Locally

Check the console output and log file:

```bash
tail -f logs/app.log
```

You should see structured JSON logs with trace IDs.

### Verify in Datadog

1. Go to [Datadog Logs Explorer](https://app.datadoghq.com/logs)
2. Search for: `service:hexagon-dotnet-app`
3. Wait 1-2 minutes for logs to appear (slight ingestion delay)

## Step 8: Log-Trace Correlation in Action

The real power emerges when you view logs within traces:

1. Navigate to [Datadog APM](https://app.datadoghq.com/apm/traces)
2. Find a trace for your test endpoint
3. Click on a span
4. Look for the **Logs** tab

You'll see all logs that occurred during that span, automatically linked by `trace_id` and `span_id`. This means:

- 🔍 **Contextual debugging:** See exactly what was logged during a slow request
- 🔗 **Cross-service correlation:** For microservices, see logs from all services
- ⏱️ **Timeline view:** Logs are placed on the trace timeline

## Advanced: Custom Log Properties

Enhance debugging by adding custom properties:

```csharp
using (LogContext.PushProperty("UserId", userId))
using (LogContext.PushProperty("Operation", "CreateTodo"))
{
    logger.LogInformation("User requested todo creation");
    
    // Your business logic here
    
    logger.LogInformation("Todo created successfully with ID {TodoId}", todo.Id);
}
```

These properties become searchable in Datadog:

```
service:hexagon-dotnet-app UserId:12345 Operation:CreateTodo
```

## Production Considerations

### 1. Log Sampling

For high-throughput applications, consider log sampling:

```csharp
.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
.MinimumLevel.Override("System", LogEventLevel.Warning)
```

### 2. Sensitive Data Filtering

Never log sensitive information:

```csharp
.Destructure.ByTransforming<User>(u => new {
    u.Id,
    Email = "***REDACTED***", // Don't log PII
    u.Username
})
```

### 3. Performance Impact

Logging to Datadog cloud adds ~5-10ms per request. For critical paths:

- Use `LogLevel.Information` or higher in production
- Leverage the local agent for buffering
- Consider async logging for non-critical logs

### 4. Cost Optimization

Datadog charges by log volume:

- Set appropriate retention policies (7-15 days for most logs)
- Use log exclusion filters for noisy logs
- Archive old logs to S3 for compliance

## Troubleshooting Common Issues

### Logs Not Appearing in Datadog

**Check 1:** Verify API key is set:
```bash
echo $DD_API_KEY
```

**Check 2:** Test Datadog endpoint directly:
```bash
curl -X POST "https://http-intake.logs.us5.datadoghq.com/api/v2/logs" \
  -H "Content-Type: application/json" \
  -H "DD-API-KEY: $DD_API_KEY" \
  -d '[{"message":"test","service":"hexagon-dotnet-app"}]'
```

Should return: `{"status":"ok"}`

**Check 3:** Verify network connectivity (check firewall rules for corporate networks)

### Trace IDs Not Appearing

Ensure OpenTelemetry instrumentation is configured:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
    );
```

### Logs and Traces Not Correlating

Verify the enricher is adding `dd.trace_id` and `dd.span_id`:

```bash
# Check log file for these fields
cat logs/app.log | grep "dd.trace_id"
```

## Real-World Example: Debugging a Slow Request

Let's see this in action with a realistic scenario:

**Problem:** Users report slow response times on the `/api/v1/pokemon/{id}` endpoint.

**Step 1:** Search Datadog APM for slow traces:
```
service:hexagon-dotnet-app resource_name:/api/v1/pokemon/* @duration:>2s
```

**Step 2:** Open a slow trace and view the timeline

**Step 3:** Click on the database span → View Logs

**Result:** You see logs showing:
```
[Warning] Database query took 3.2s
[Info] Query: SELECT * FROM pokemon WHERE id = @p0
[Error] Database connection pool exhausted
```

This immediately reveals the root cause: connection pool exhaustion during a slow query.

## Best Practices Summary

✅ **DO:**
- Use structured logging with Serilog
- Enable log-trace correlation with OpenTelemetry
- Set up multiple sinks (console for dev, Datadog for prod)
- Add meaningful context properties
- Use appropriate log levels
- Flush logs on application shutdown

❌ **DON'T:**
- Log sensitive data (passwords, tokens, PII)
- Over-log in tight loops
- Use string interpolation instead of structured logging
- Commit API keys to version control
- Ignore log retention costs

## Conclusion

Setting up Datadog logging in .NET 10 with full trace correlation provides unparalleled observability into your application. The combination of Serilog's structured logging, Datadog's unified platform, and OpenTelemetry's distributed tracing creates a powerful debugging toolkit.

The investment in proper logging infrastructure pays dividends when production incidents occur. Instead of piecing together scattered logs, you get a complete picture of what happened, when, and why—all in context.

## Additional Resources

- [Serilog Documentation](https://serilog.net/)
- [Datadog .NET APM Guide](https://docs.datadoghq.com/tracing/setup_overview/setup/dotnet/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [W3C Trace Context Specification](https://www.w3.org/TR/trace-context/)

## Complete Sample Code

The complete working example from this guide is available on GitHub:
```
https://github.com/yourusername/hexagon-dotnet-app
```

---

*Have questions or feedback? Leave a comment below or connect with me on [Twitter](https://twitter.com/yourhandle).*

*Found this helpful? Give it a clap 👏 and share with your team!*
