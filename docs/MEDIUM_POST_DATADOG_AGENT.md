# Setting Up Datadog Observability in .NET 10 Using a Local Agent

![Datadog Agent Architecture](https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=1200&h=400&fit=crop)

When building production-ready applications, having a robust observability stack isn't optional—it's essential. But setting up logging, tracing, and metrics can feel overwhelming, especially when juggling multiple services and environments.

What if I told you there's a better way? Instead of sending telemetry directly from your application to the cloud, you can use a **local Datadog agent** that acts as a smart proxy, buffering data, handling retries, and giving you local visibility before anything hits production.

In this comprehensive guide, I'll show you how to set up a **Datadog agent-based observability stack** for .NET 10 applications, covering logs, traces, and metrics—all with automatic correlation.

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
| **Local Debugging**     | ❌ No local visibility     | ✅ View data before cloud   |
| **Performance**         | ⚠️ Blocks on network calls | ✅ Async with buffering     |
| **Multiple Protocols**  | ❌ Limited                 | ✅ OTLP, StatsD, native APM |
| **Data Processing**     | ❌ None                    | ✅ Agent-side filtering     |
| **Cost Control**        | ⚠️ Send everything         | ✅ Filter before sending    |

The agent becomes especially valuable in development environments where:

- Internet connectivity is unreliable
- You want to test without affecting production metrics
- You need to debug telemetry before it leaves your machine
- You're working with multiple services that need unified collection

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

Before we begin, ensure you have:

- ✅ **.NET 10 SDK** installed
- ✅ **Docker or Podman** (for running the agent)
- ✅ **Datadog account** ([free trial available](https://www.datadoghq.com/))
- ✅ **Datadog API key** (optional for local-only development)

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

Make it executable:

```bash
chmod +x scripts/datadog-agent.sh
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
   - OTLP HTTP:      localhost:4318
   - OTLP gRPC:      localhost:4317
   - StatsD:         localhost:8125
   - APM:            localhost:8126
```

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
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
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
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

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
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequest = (activity, request) =>
            {
                activity.SetTag("http.method", request.Method);
                activity.SetTag("http.url", request.Path);
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
        })
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(
                Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                ?? "http://localhost:4318");
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
        }));

var app = builder.Build();

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
        flushToDiskInterval: TimeSpan.FromSeconds(1)
    )
    .CreateLogger();
```

### Enable Log Collection in Agent

The agent startup script already mounts the logs directory:

```bash
-v "${SCRIPT_DIR}/../src/App.Api/logs:/app/logs:ro"
```

To enable log tailing, create `scripts/datadog-agent/conf.d/app-logs.d/conf.yaml`:

```yaml
logs:
  - type: file
    path: /app/logs/app*.log
    service: hexagon-dotnet-app
    source: csharp
    tags:
      - env:development
```

Restart the agent to apply changes:

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

```csharp
using StatsdClient;

// Configure DogStatsD client
var dogstatsdConfig = new StatsdConfig
{
    StatsdServerName = "localhost",
    StatsdPort = 8125,
    Prefix = "hexagon"
};

DogStatsd.Configure(dogstatsdConfig);

// Send metrics
app.MapGet("/metrics-test", () =>
{
    DogStatsd.Increment("api.request");
    DogStatsd.Gauge("api.active_connections", 42);
    DogStatsd.Histogram("api.response_time", 123.45);

    return Results.Ok(new { message = "Metrics sent to agent!" });
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
- **Memory:** 150-300MB typical workload
- **Network:** Batches and compresses data (80% reduction)

### Application Performance Impact

Using a local agent vs direct cloud:

- **Latency:** <1ms to agent vs 50-200ms to cloud
- **Blocking:** Non-blocking with async sends
- **Reliability:** Buffers during cloud outages

### Optimization Tips

1. **Use sampling for high-volume traces:**

```csharp
.AddAspNetCoreInstrumentation(options =>
{
    options.Sampler = new TraceIdRatioBasedSampler(0.1); // 10% sampling
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
          image: gcr.io/datadoghq/agent:latest
          env:
            - name: DD_API_KEY
              valueFrom:
                secretKeyRef:
                  name: datadog-secret
                  key: api-key
            - name: DD_SITE
              value: "us5.datadoghq.com"
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
  template:
    spec:
      containers:
        - name: agent
          image: gcr.io/datadoghq/agent:latest
          env:
            - name: DD_KUBERNETES_KUBELET_HOST
              valueFrom:
                fieldRef:
                  fieldPath: status.hostIP
      hostNetwork: true
```

### Agent in Docker Compose

```yaml
version: "3.8"
services:
  app:
    build: .
    environment:
      OTEL_EXPORTER_OTLP_ENDPOINT: http://datadog-agent:4318
    depends_on:
      - datadog-agent

  datadog-agent:
    image: gcr.io/datadoghq/agent:latest
    environment:
      DD_API_KEY: ${DD_API_KEY}
      DD_SITE: us5.datadoghq.com
    ports:
      - "4318:4318"
      - "8125:8125/udp"
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
app.MapGet("/agent-health", async () =>
{
    using var httpClient = new HttpClient();

    try
    {
        var response = await httpClient.GetAsync("http://localhost:4318/");
        return Results.Ok(new
        {
            agent = "reachable",
            endpoint = "http://localhost:4318",
            status = response.StatusCode
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(new
        {
            agent = "unreachable",
            error = ex.Message
        });
    }
});
```

### Agent Metrics Dashboard

The agent exposes its own metrics:

```bash
# Agent internal metrics
docker exec dd-agent agent status --json | jq '.aggregatorStats'
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
https://github.com/yourusername/hexagon-dotnet-app
```

Includes:

- Agent configuration files
- Startup scripts for Docker/Podman
- Complete .NET 10 application
- Kubernetes manifests
- Docker Compose setups

---

_Questions? Drop a comment below or reach out on [Twitter](https://twitter.com/yourhandle)._

_Found this helpful? Share it with your team and give it a clap! 👏_
