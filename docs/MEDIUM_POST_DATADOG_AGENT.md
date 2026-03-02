# Setting Up Datadog Observability in .NET 10 Using a Local Agent

![Datadog Agent Architecture](https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=1200&h=400&fit=crop)

When building production-ready applications, having a robust observability stack isn't optional—it's essential. But setting up logging, tracing, and metrics can feel overwhelming, especially when juggling multiple services and environments.

What if I told you there's a better way? Instead of sending telemetry directly from your application to the cloud, you can use a **local Datadog agent** that acts as a smart proxy, buffering data, handling retries, and giving you local visibility before anything hits production.

In this comprehensive guide, I'll show you how to set up a **Datadog agent-based observability stack** for .NET 10 applications, covering logs, traces, and metrics—all with automatic correlation.

## Table of Contents

1. [Why Use a Local Datadog Agent?](#why-use-a-local-datadog-agent)
2. [Architecture Overview](#architecture-overview)
3. [Prerequisites](#prerequisites)
4. [Quick Start (5 Minutes)](#quick-start-5-minutes)
5. [Step 1: Setting Up the Datadog Agent](#step-1-setting-up-the-datadog-agent)
6. [Step 2: Configure Your .NET Application](#step-2-configure-your-net-application)
7. [Step 3: Sending Logs to the Agent](#step-3-sending-logs-to-the-agent)
8. [Step 4: Testing Your Setup](#step-4-testing-your-setup)
9. [Step 5: Advanced Agent Features](#step-5-advanced-agent-features)
10. [Debugging Common Issues](#debugging-common-issues)
11. [Performance Considerations](#performance-considerations)
12. [Production Deployment](#production-deployment)

## Why Use a Local Datadog Agent?

Before diving into implementation, let's understand why the agent-based approach is superior to direct cloud integration:

### The Two Architectures

**Direct to Cloud (One-Hop):**

```
Application → Datadog Cloud
```

**Agent-Based (Two-Hop):**

```
Application → Local Agent → Datadog Cloud
```

### Benefits of the Agent-Based Approach

| Feature                 | Direct to Cloud            | With Local Agent            |
| ----------------------- | -------------------------- | --------------------------- |
| **Offline Development** | ❌ Requires internet       | ✅ Works offline            |
| **Network Reliability** | ❌ Drops data on failures  | ✅ Buffers and retries      |
| **Local Debugging**     | ❌ No local visibility     | ✅ View metrics/status via CLI (Note: Full trace UI needs Datadog Cloud) |
| **Performance**         | ⚠️ Blocks on network calls | ✅ Async with buffering     |
| **Multiple Protocols**  | ❌ Limited                 | ✅ OTLP, StatsD, native APM |
| **Data Processing**     | ❌ None                    | ✅ Agent-side filtering     |
| **Cost Control**        | ⚠️ Send everything         | ✅ Filter before sending    |

The agent becomes especially valuable in development environments where:

- Internet connectivity is unreliable
- You want to test without affecting production metrics
- You need to debug telemetry before it leaves your machine
- You're working with multiple services that need unified collection

### Comparison with Other Solutions

For .NET developers familiar with other observability platforms:

| Feature | Datadog Agent | Application Insights | Elastic APM |
|---------|---------------|---------------------|------------|
| **Local Agent** | ✅ Yes | ❌ No (direct to cloud) | ✅ Yes |
| **Offline Development** | ✅ Yes | ❌ No | ✅ Yes (with local ES) |
| **Multi-Cloud Support** | ✅ All clouds | ⚠️ Azure-optimized | ✅ All clouds |
| **Cost Model** | Per GB ingested | Per GB + node count | Self-hosted or cloud |
| **Trace Retention** | 15 days (configurable) | 90 days | Configurable |
| **Learning Curve** | Medium | Low (if using Azure) | Medium-High |
| **K8s Native** | ✅ DaemonSet pattern | ⚠️ Sidecar only | ✅ DaemonSet pattern |

## Architecture Overview

Here's how all the pieces fit together:

```
┌─────────────────────────────────────────────────┐
│                                         .NET Application                                         │
│                                        (ASP.NET Core API)                                        │
│                                                                                                  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │
│  │       Serilog Logs       │  │   OpenTelemetry Traces   │  │       .NET Metrics       │  │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  │
└────────┼───────────────┼───────────────┼────────┘
                  │                              │                              │
                  │ JSON/File                    │ OTLP/HTTP                    │ StatsD
                  │                              │ :4318                        │ :8125
                  ▼                              ▼                              ▼
┌─────────────────────────────────────────────────┐
│                                          Datadog Agent                                           │
│                                           (localhost)                                            │
│                                                                                                  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │
│  │        Log Tailer        │  │      OTLP Receiver       │  │    DogStatsD Receiver    │  │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘  │
│                │                              │                              │                │
│                └───────────────┼───────────────┘                │
│                                                │                                                │
│                          ┌──────────▼──────────┐                          │
│                          │   Unified Forwarder                      │                          │
│                          │   📦 Batching                            │                          │
│                          │   🗜️ Compression                         │                          │
│                          │   🔄 Retry Logic                         │                          │
│                          └─────────┬───────────┘                          │
└───────────────────────┼─────────────────────────┘
                                                │
                                                │ HTTPS (with API Key)
                                                │
                                                ▼
                        ┌────────────────────────┐
                        │                 Datadog Cloud.                 │
                        │              (us5.datadoghq.com)               │
                        │                                                │
                        │  📊 Logs Explorer                              │
                        │  📈 APM Traces                                 │
                        │  📉 Metrics                                    │
                        └────────────────────────┘
```

## Prerequisites

**Estimated setup time:** 30-45 minutes

Before we begin, ensure you have:

- ✅ **.NET 10 SDK** installed
- ✅ **Docker or Podman** (for running the agent)
- ✅ **Datadog account** ([free trial available](https://www.datadoghq.com/))
- ✅ **Datadog API key** (optional for local-only development)

## Quick Start (5 Minutes)

Want to get started immediately? Here's the fast path:

```bash
# 1. Start the agent (optional: set DD_API_KEY to send to cloud)
export DD_API_KEY=your_key_here  # Optional
./scripts/datadog-agent.sh

# 2. Run your app
dotnet run --project src/App.Api

# 3. Verify agent is receiving data
docker exec dd-agent agent status

# 4. Generate test traffic
curl http://localhost:5112/health
```

Now let's dive into the details...

## Step 1: Setting Up the Datadog Agent

### Create the Agent Configuration

First, create the agent configuration file at `scripts/datadog-agent/datadog.yaml`:

```yaml
## Datadog Agent Configuration

# API key (set via environment variable)
api_key: ${DD_API_KEY}

# Datadog site (adjust for your region)
site: us5.datadoghq.com

# Agent hostname
hostname: hexagon-dotnet-local

# Log level for debugging
log_level: info

##################################
## APM Configuration
##################################

apm_config:
  enabled: true
  apm_non_local_traffic: false
  analyzed_spans:
    enabled: true

##################################
## OTLP Configuration
##################################

otlp_config:
  receiver:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318 # This is where your app sends traces

##################################
## Logs Configuration
##################################

logs_enabled: true
logs_config:
  container_collect_all: false
  auto_multi_line_detection: true

##################################
## DogStatsD (Metrics)
##################################

dogstatsd_non_local_traffic: true
dogstatsd_port: 8125
```

**Key configurations explained:**

- **OTLP endpoints (4317/4318):** Where OpenTelemetry sends traces
- **DogStatsD (8125):** For custom metrics
- **APM enabled:** Native Datadog APM on port 8126
- **Logs enabled:** Agent can tail log files or collect from containers

### Create the Agent Startup Script

Create `scripts/datadog-agent.sh`:

```bash
#!/bin/bash

set -e

# Detect container runtime (Docker or Podman)
if command -v docker &> /dev/null; then
    CONTAINER_RUNTIME="docker"
elif command -v podman &> /dev/null; then
    CONTAINER_RUNTIME="podman"
else
    echo "❌ Error: Neither Docker nor Podman is installed"
    exit 1
fi

echo "🐕 Starting Datadog Agent using ${CONTAINER_RUNTIME}..."

# Stop existing agent if running
if ${CONTAINER_RUNTIME} ps -a --format '{{.Names}}' | grep -q '^dd-agent$'; then
    echo "🛑 Stopping existing dd-agent container..."
    ${CONTAINER_RUNTIME} stop dd-agent || true
    ${CONTAINER_RUNTIME} rm dd-agent || true
fi

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Check for API key
if [ -z "${DD_API_KEY}" ]; then
    echo "⚠️  Warning: DD_API_KEY not set"
    echo "   Agent will work locally but won't forward to Datadog cloud"
    echo "   To set it: export DD_API_KEY=your-api-key"
    echo ""
fi

# Run the agent
${CONTAINER_RUNTIME} run -d \
    --name dd-agent \
    --hostname hexagon-dotnet-local \
    -e DD_API_KEY="${DD_API_KEY:-dummy-key-for-local-dev}" \
    -e DD_SITE="us5.datadoghq.com" \
    -e DD_ENV="development" \
    -e DD_SERVICE="hexagon-dotnet-app" \
    -e DD_APM_ENABLED=true \
    -e DD_LOGS_ENABLED=true \
    -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_GRPC_ENDPOINT="0.0.0.0:4317" \
    -e DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_HTTP_ENDPOINT="0.0.0.0:4318" \
    -p 8125:8125/udp \
    -p 8126:8126 \
    -p 4317:4317 \
    -p 4318:4318 \
    -v "${SCRIPT_DIR}/datadog-agent/datadog.yaml:/etc/datadog-agent/datadog.yaml:ro" \
    -v "${SCRIPT_DIR}/../src/App.Api/logs:/app/logs:ro" \
    gcr.io/datadoghq/agent:latest

echo ""
echo "✅ Datadog agent started successfully!"
echo ""
echo "📊 Agent Endpoints:"
echo "   - OTLP HTTP:      localhost:4318  (OpenTelemetry traces)"
echo "   - OTLP gRPC:      localhost:4317  (OpenTelemetry traces)"
echo "   - StatsD:         localhost:8125  (Metrics)"
echo "   - APM:            localhost:8126  (Native Datadog APM)"
echo ""
echo "🔍 Useful commands:"
echo "   Check status:  ${CONTAINER_RUNTIME} exec dd-agent agent status"
echo "   View logs:     ${CONTAINER_RUNTIME} logs -f dd-agent"
echo "   Stop agent:    ${CONTAINER_RUNTIME} stop dd-agent"
echo ""
```

**Key improvements in this script:**
- ✅ Automatic Docker/Podman detection
- ✅ Proper error handling and validation
- ✅ Helpful status messages and next steps
- ✅ Handles existing containers gracefully

Make it executable:

```bash
chmod +x scripts/datadog-agent.sh
```

**Important Note:** The script uses environment variables for configuration instead of mounting a `datadog.yaml` file. This is simpler and more flexible for development. If you need advanced agent configuration, you can create a custom `datadog.yaml` and mount it with:

```bash
-v "${SCRIPT_DIR}/datadog-agent/datadog.yaml:/etc/datadog-agent/datadog.yaml:ro" \
```

### Start the Agent

```bash
./scripts/datadog-agent.sh
```

Expected output:

```
🐕 Starting Datadog Agent using docker...
✅ Datadog agent started successfully!

📊 Agent Endpoints:
   - OTLP HTTP:      localhost:4318  (OpenTelemetry - HTTP/Protobuf)
   - OTLP gRPC:      localhost:4317  (OpenTelemetry - gRPC)
   - StatsD:         localhost:8125  (Metrics)
   - APM:            localhost:8126  (Native Datadog APM)

💡 OTLP Protocol Choice:
   HTTP: Easier debugging, works through proxies, firewall-friendly
   gRPC: Better performance, lower overhead, ideal for production
```

> **Note:** OTLP (OpenTelemetry Protocol) comes in two flavors. Use HTTP for development (simpler debugging with tools like curl) and consider gRPC for production (better performance). The agent accepts both simultaneously.

### Verify Agent Health

```bash
# Check container is running
docker ps | grep dd-agent

# Check agent status (takes ~10 seconds to initialize)
docker exec dd-agent agent status
```

Look for these sections in the status output:

- ✅ **Forwarder:** Should show "Running"
- ✅ **OTLP:** Should show endpoints and received traces
- ✅ **DogStatsD:** Should show listening on port 8125

## Step 2: Configure Your .NET Application

### Install Required Packages

```bash
# OpenTelemetry tracing
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore

# Resource detection
dotnet add package OpenTelemetry.Resources.Host

# Logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Formatting.Compact

# Configuration
dotnet add package DotNetEnv
```

### Create Environment Configuration

Create a `.env` file in your project root:

```bash
# Datadog Configuration
DD_API_KEY=your_api_key_here
DD_SITE=us5.datadoghq.com
DD_ENV=development
DD_SERVICE=hexagon-dotnet-app
DD_VERSION=1.0.0

# OpenTelemetry Configuration (points to local agent)
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_SERVICE_NAME=hexagon-dotnet-app
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=development,service.version=1.0.0
```

⚠️ **Important:** Add `.env` to your `.gitignore`:

```bash
echo ".env" >> .gitignore
```

### Configure OpenTelemetry in Program.cs

```csharp
using DotNetEnv;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Json;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog (we'll detail this next)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "hexagon-dotnet-app",
            serviceVersion: Environment.GetEnvironmentVariable("DD_VERSION") ?? "1.0.0")
        .AddAttributes(new[]
        {
            new KeyValuePair<string, object>("deployment.environment",
                Environment.GetEnvironmentVariable("DD_ENV") ?? "development")
        })
        .AddEnvironmentVariableDetector()) // Reads OTEL_RESOURCE_ATTRIBUTES env var
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Enrich = (activity, eventName, rawObject) =>
            {
                if (eventName == "OnStartActivity" && rawObject is HttpRequest request)
                {
                    activity.SetTag("http.method", request.Method);
                    activity.SetTag("http.url", request.Path.Value);
                    activity.SetTag("http.request_id", request.HttpContext.TraceIdentifier);
                }
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.SetDbStatementForStoredProcedure = true;
        })
        .AddOtlpExporter(options =>
        {
            // Note: The OpenTelemetry SDK also reads OTEL_ vars automatically.
            // We set it explicitly here for clarity in the tutorial.
            options.Endpoint = new Uri(
                Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                ?? "http://localhost:4318");
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
        }));

var app = builder.Build();

// Enable automatic HTTP request logging
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
    };
});

// Minimal API endpoints
app.MapGet("/", () => "Hello from .NET 10!");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

Log.Information("Starting Hexagon .NET App with Datadog agent integration");

app.Run();
```

## Step 3: Sending Logs to the Agent

The agent can collect logs in multiple ways. Let's use file tailing as it's the most reliable for local development.

### Configure Serilog with File Output

Update your `Program.cs` with enhanced Serilog configuration:

```csharp
using Serilog;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", "hexagon-dotnet-app")
    .Enrich.WithProperty("env", Environment.GetEnvironmentVariable("DD_ENV") ?? "development")
    .Enrich.WithProperty("version", Environment.GetEnvironmentVariable("DD_VERSION") ?? "1.0.0")
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.File(
        new JsonFormatter(),
        path: "logs/app.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(10) // Batch for performance
    )
    .CreateLogger();
```

### Enable Log Collection in Agent

**Note:** The basic agent script uses environment variables for configuration. For log file tailing, you need to mount both the logs directory and a custom configuration.

**Option 1: Simple Approach (Recommended for Development)**

Just use console and file logging from your app. The agent will collect container logs automatically if you're running in Docker/Kubernetes.

**Option 2: Advanced File Tailing**

If you want the agent to tail log files directly:

1. Create `scripts/datadog-agent/conf.d/app-logs.d/conf.yaml`:

```yaml
logs:
  - type: file
    path: /app/logs/app*.log
    service: hexagon-dotnet-app
    source: csharp
    tags:
      - env:development
```

2. Update your agent startup script to mount the config:

```bash
# Add these volume mounts to the docker run command:
-v "${PROJECT_ROOT}/src/App.Api/logs:/app/logs:ro" \
-v "${SCRIPT_DIR}/datadog-agent/conf.d:/etc/datadog-agent/conf.d:ro" \
```

3. Restart the agent:

```bash
docker restart dd-agent
```

## Step 4: Testing Your Setup

### Start Your Application

```bash
dotnet run --project src/App.Api
```

### Generate Test Traffic

```bash
# Health check
curl http://localhost:5112/health

# Generate some load
for i in {1..10}; do
  curl http://localhost:5112/
  sleep 0.5
done
```

### Verify Data in Agent

#### Check Agent Status

```bash
docker exec dd-agent agent status
```

Look for:

**OTLP Section:**

```
=========
OTLP
=========

  Total Traces: 10
  Total Spans: 15
  Trace Endpoint: http://localhost:4318
```

**Logs Section:**

```
==========
Logs Agent
==========

  Sending compressed logs in HTTPS to: https://http-intake.logs.us5.datadoghq.com

  app-logs
  --------
    Type: file
    Path: /app/logs/app*.log
    Status: OK
    Lines Sent: 25
```

**Metrics Section:**

```
=========
DogStatsD
=========

  Packets received: 150
  Metrics processed: 120
```

#### View Agent Logs

```bash
# Watch agent logs in real-time
docker logs -f dd-agent

# Filter for specific data types
docker logs dd-agent | grep "OTLP"
docker logs dd-agent | grep "Logs"
docker logs dd-agent | grep "StatsD"
```

### Verify in Datadog Cloud (If API Key Provided)

If you set `DD_API_KEY`, data will be forwarded to Datadog cloud:

**Traces:**

1. Go to [APM Traces](https://app.datadoghq.com/apm/traces)
2. Filter: `service:hexagon-dotnet-app env:development`

**Logs:**

1. Go to [Logs Explorer](https://app.datadoghq.com/logs)
2. Search: `service:hexagon-dotnet-app`

**Metrics:**

1. Go to [Metrics Explorer](https://app.datadoghq.com/metric/explorer)
2. Search: `hexagon-dotnet-app`

## Step 5: Advanced Agent Features

### Custom Metrics with DogStatsD

Send custom metrics using the StatsD protocol:

```bash
dotnet add package DogStatsD-CSharp-Client
```

**Configure in Program.cs:**

```csharp
using StatsdClient;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure DogStatsD client (do this early in startup)
var dogstatsdConfig = new DogStatsdConfig
{
    StatsdServerName = "localhost",
    StatsdPort = 8125,
    Prefix = "hexagon"
};

DogStatsd.Configure(dogstatsdConfig);
// Note: In modern .NET apps, using builder.Services.AddDogStatsd(dogstatsdConfig) 
// and injecting IDogStatsd is often preferred to make testing easier.

// ... rest of configuration

var app = builder.Build();

// Send metrics in endpoints
app.MapGet("/metrics-test", () =>
{
    using (DogStatsd.StartTimer("api.response_time"))
    {
        DogStatsd.Increment("api.request", tags: new[] { "endpoint:metrics-test" });
        DogStatsd.Gauge("api.active_connections", 42);
        
        // Simulate work
        Thread.Sleep(100);
        
        return Results.Ok(new { message = "Metrics sent to agent!" });
    }
});

// Don't forget to dispose on shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    DogStatsd.Dispose();
});
```

### Agent-Side Filtering

Reduce costs by filtering data before it reaches Datadog cloud.

Edit `scripts/datadog-agent/datadog.yaml`:

```yaml
apm_config:
  enabled: true
  # Filter out health check traces
  filter_tags:
    reject:
      - "http.url:/health"

  # Sample traces (keep 10%)
  analyzed_rate_by_service:
    "hexagon-dotnet-app|*": 0.1

logs_config:
  # Exclude debug logs
  processing_rules:
    - type: exclude_at_match
      name: exclude_debug_logs
      pattern: "level:debug"
```

### Multiple Environment Support

Run different agent configurations per environment:

```bash
# Development agent (localhost)
DD_ENV=development ./scripts/datadog-agent.sh

# Staging agent (with different API key)
DD_API_KEY=${STAGING_API_KEY} DD_ENV=staging ./scripts/datadog-agent.sh
```

## Debugging Common Issues

### Pre-Flight Checklist

Before diving into troubleshooting, verify these basics:

- [ ] Agent container is running: `docker ps | grep dd-agent`
- [ ] Required ports are exposed: `docker port dd-agent`
- [ ] App can reach agent: `curl -v http://localhost:4318`
- [ ] Agent status shows no errors: `docker exec dd-agent agent status`
- [ ] API key is set (if using cloud): `echo $DD_API_KEY`
- [ ] Logs directory exists: `ls -la src/App.Api/logs/`
- [ ] No firewall blocking ports: `netstat -an | grep -E '4318|8125|8126'`
- [ ] Container logs show no errors: `docker logs dd-agent | grep ERROR`

### Issue: Agent Not Receiving Traces

**Symptom:** `agent status` shows 0 traces received

**Solution:**

1. Verify endpoint configuration:

```bash
# In your app
echo $OTEL_EXPORTER_OTLP_ENDPOINT
# Should be: http://localhost:4318

# Check agent is listening
netstat -an | grep 4318
lsof -i :4318
```

2. Test OTLP endpoint directly:

```bash
curl -v http://localhost:4318/v1/traces
# Should return: 404 page not found (endpoint exists but doesn't accept GET)
```

3. Check agent logs for errors:

```bash
docker logs dd-agent | grep -i error
```

### Issue: Logs Not Appearing

**Symptom:** Agent status shows 0 log lines sent

**Solution:**

1. Verify log file exists and is being written:

```bash
ls -lh src/App.Api/logs/
tail -f src/App.Api/logs/app.log
```

2. Check volume mount is correct:

```bash
docker exec dd-agent ls -l /app/logs/
```

3. Verify log collection config:

```bash
docker exec dd-agent agent configcheck | grep -A 10 "logs"
```

### Issue: High Memory Usage

**Symptom:** Agent consuming >500MB RAM

**Solution:**

Limit agent resources in `docker run`:

```bash
docker run \
  --memory="256m" \
  --cpus="0.5" \
  ...
```

Or configure batch sizes in `datadog.yaml`:

```yaml
forwarder_timeout: 10
forwarder_retry_queue_payloads_max_size: 10485760 # 10MB
```

## Performance Considerations

### Agent Performance Impact

The agent itself uses:

- **CPU:** ~2-5% on modern hardware
- **Memory:** 300-500MB typical workload (can be higher with many integrations)
- **Network:** Batches and compresses data (80% reduction)
- **Disk:** Minimal (buffers in memory, uses /tmp for staging)

### Application Performance Impact

Using a local agent vs direct cloud:

- **Latency:** <1ms to agent vs 50-200ms to cloud
- **Blocking:** Non-blocking with async sends
- **Reliability:** Buffers during cloud outages

### Optimization Tips

1. **Use sampling for high-volume traces:**

```csharp
using OpenTelemetry.Trace;

.AddAspNetCoreInstrumentation(options =>
{
    // Use parent-based sampling to respect upstream decisions
    options.Sampler = new ParentBasedSampler(
        new TraceIdRatioBasedSampler(0.1)  // 10% sampling for root spans
    );
})
```

2. **Batch logs efficiently:**

```csharp
.WriteTo.File(
    flushToDiskInterval: TimeSpan.FromSeconds(5), // Batch writes
    shared: true // Allow multiple processes
)
```

3. **Filter noisy endpoints:**

```yaml
apm_config:
  filter_tags:
    reject:
      - "http.url:/health"
      - "http.url:/metrics"
```

### Cost Estimation

Understanding Datadog costs helps you optimize your observability spend:

**Monthly Cost Estimates (as of 2026):**

| Metric | Small App (10K req/day) | Medium App (1M req/day) | Large App (10M req/day) |
|--------|------------------------|-------------------------|------------------------|
| **Traces** | ~5GB → $12/mo | ~50GB (10% sampled) → $120/mo | ~500GB (1% sampled) → $1,200/mo |
| **Logs** | ~10GB → $10/mo | ~100GB (filtered) → $100/mo | ~1TB (aggressive filtering) → $1,000/mo |
| **Metrics** | ~250 custom → $15/mo | ~1,000 custom → $60/mo | ~5,000 custom → $300/mo |
| **Total** | **~$37/mo** | **~$280/mo** | **~$2,500/mo** |

**Cost Optimization Strategies:**

1. **Use agent-side filtering** (covered in Step 5)
2. **Implement smart sampling** (health checks, static assets at 1%, business logic at 100%)
3. **Set appropriate retention** (7 days for debug logs, 30 days for errors)
4. **Use log indexes** to separate hot vs cold data
5. **Archive to S3** for compliance (much cheaper long-term storage)

**Pro Tip:** Enable cost tracking in Datadog by tagging resources with `cost_center` and `team` tags.

## Production Deployment

### Agent as Sidecar (Kubernetes)

Deploy the agent as a sidecar container:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: hexagon-dotnet-app
spec:
  template:
    spec:
      containers:
        - name: app
          image: hexagon-dotnet-app:latest
          env:
            - name: OTEL_EXPORTER_OTLP_ENDPOINT
              value: "http://localhost:4318"

        - name: datadog-agent
          image: gcr.io/datadoghq/agent:7.50.0  # Pin to specific version
          env:
            - name: DD_API_KEY
              valueFrom:
                secretKeyRef:
                  name: datadog-secret
                  key: api-key
            - name: DD_SITE
              valueFrom:
                configMapKeyRef:
                  name: datadog-config
                  key: site
            - name: DD_ENV
              valueFrom:
                configMapKeyRef:
                  name: datadog-config
                  key: environment
          ports:
            - containerPort: 4318
            - containerPort: 8125
```

### Agent as DaemonSet (Kubernetes)

For node-level collection:

```yaml
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: datadog-agent
spec:
  selector:
    matchLabels:
      app: datadog-agent
  template:
    metadata:
      labels:
        app: datadog-agent
    spec:
      containers:
        - name: agent
          image: gcr.io/datadoghq/agent:7.50.0
          env:
            - name: DD_KUBERNETES_KUBELET_HOST
              valueFrom:
                fieldRef:
                  fieldPath: status.hostIP
            - name: DD_API_KEY
              valueFrom:
                secretKeyRef:
                  name: datadog-secret
                  key: api-key
      hostNetwork: true
      # Optional: Target specific nodes
      nodeSelector:
        observability: enabled
      # Optional: Tolerate node taints
      tolerations:
        - key: node-role.kubernetes.io/control-plane
          operator: Exists
          effect: NoSchedule
```

### Agent in Docker Compose

```yaml
version: "3.8"
services:
  app:
    build: .
    environment:
      OTEL_EXPORTER_OTLP_ENDPOINT: http://datadog-agent:4318
      ASPNETCORE_ENVIRONMENT: Development
    depends_on:
      - datadog-agent

  datadog-agent:
    image: gcr.io/datadoghq/agent:7.50.0
    environment:
      DD_API_KEY: ${DD_API_KEY}
      DD_SITE: us5.datadoghq.com
      DD_APM_ENABLED: "true"
      DD_LOGS_ENABLED: "true"
      DD_OTLP_CONFIG_RECEIVER_PROTOCOLS_HTTP_ENDPOINT: 0.0.0.0:4318
    ports:
      - "4318:4318"
      - "8125:8125/udp"
      - "8126:8126"
    volumes:
      - ./src/App.Api/logs:/app/logs:ro
```

## When to Use Agent vs Direct Cloud

| Scenario                  | Recommendation                        |
| ------------------------- | ------------------------------------- |
| **Local Development**     | ✅ Use Agent (offline capability)     |
| **CI/CD Pipeline**        | ⚠️ Direct Cloud (simpler setup)       |
| **Kubernetes Production** | ✅ Use Agent (DaemonSet pattern)      |
| **Serverless (Lambda)**   | ❌ Direct Cloud (no persistent agent) |
| **Docker Compose**        | ✅ Use Agent (sidecar pattern)        |
| **High-Volume Services**  | ✅ Use Agent (buffering, sampling)    |
| **Simple Single Service** | ⚠️ Either works                       |

## Monitoring Agent Health

### Create Agent Health Check Endpoint

```csharp
using System.Net.Sockets;

app.MapGet("/agent-health", async () =>
{
    var endpoints = new[] 
    { 
        ("OTLP", "localhost", 4318),
        ("StatsD", "localhost", 8125),
        ("APM", "localhost", 8126)
    };
    
    var results = new List<object>();
    
    foreach (var (name, host, port) in endpoints)
    {
        try
        {
            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(host, port);
            
            if (await Task.WhenAny(connectTask, Task.Delay(1000)) == connectTask)
            {
                await connectTask; // Throws if connection failed before the timeout
                results.Add(new { endpoint = name, port, status = "reachable" });
            }
            else
            {
                results.Add(new { endpoint = name, port, status = "timeout" });
            }
        }
        catch (Exception ex)
        {
            results.Add(new { endpoint = name, port, status = "unreachable", error = ex.Message });
        }
    }
    
    var allReachable = results.All(r => ((dynamic)r).status == "reachable");
    
    return allReachable 
        ? Results.Ok(new { agent = "healthy", endpoints = results })
        : Results.Problem(
            title: "Agent partially unreachable",
            detail: "Some agent endpoints are not responding",
            statusCode: 503,
            instance: "/agent-health",
            extensions: new Dictionary<string, object?> { ["endpoints"] = results }
        );
});
```

### Agent Metrics Dashboard

The agent exposes its own metrics:

```bash
# Agent internal metrics
docker exec dd-agent agent status --json | jq '.aggregator_stats // .aggregatorStats'

# Or get full status
docker exec dd-agent agent status
```

Track these in Datadog:

- `datadog.agent.running`
- `datadog.agent.started`
- `datadog.dogstatsd.packets_received`
- `datadog.trace.agent.trace_writer.traces`

## Summary

Setting up a Datadog agent for local observability provides:

✅ **Buffering & Reliability:** No data loss during network issues  
✅ **Local Debugging:** View telemetry before it hits cloud  
✅ **Offline Development:** Work without internet connectivity  
✅ **Performance:** Async, non-blocking telemetry shipping  
✅ **Cost Control:** Filter and sample data agent-side  
✅ **Multi-Protocol:** OTLP, StatsD, native APM support  
✅ **Production Ready:** Same agent scales from laptop to Kubernetes

The agent-based architecture is the **recommended approach** for most production deployments. While direct-to-cloud works for serverless and simple scenarios, the agent provides superior reliability, performance, and developer experience.

## Next Steps

1. **Explore advanced agent features:**
   - Service discovery and auto-configuration
   - Log processing pipelines
   - APM trace analytics

2. **Optimize for your environment:**
   - Configure sampling rates
   - Set up tag-based routing
   - Implement cost controls

3. **Deploy to production:**
   - Kubernetes DaemonSet setup
   - High-availability agent configuration
   - Multi-region deployments

## Complete Example Repository

The full working code from this guide is available on GitHub:

```
https://github.com/hieu-nv/hexagon-dotnet-app
```

Includes:

- Agent configuration files
- Startup scripts for Docker/Podman
- Complete .NET 10 application
- Kubernetes manifests
- Docker Compose setups

---

_Questions? Drop a comment below or reach out on [Bluesky](https://bsky.app/profile/hieunv.bsky.social)._

_Found this helpful? Share it with your team and give it a clap! 👏_
