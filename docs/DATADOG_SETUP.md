# Datadog Logging & APM Setup Guide

## Why Logs Aren't Appearing in Datadog

### Understanding Logs vs APM Traces

In Datadog, **logs** and **APM traces** are separate but can be correlated:

- **APM Traces**: Performance monitoring data showing request flows, latency, errors
  - View at: `https://us5.datadoghq.com/apm/traces`
  - Automatically sent via OpenTelemetry when APM is configured

- **Logs**: Application log messages (Info, Warning, Error)
  - View at: `https://us5.datadoghq.com/logs`
  - Require Datadog API key to send from Serilog

- **Correlated Logs in APM**: Logs appear within traces when they have matching `trace_id` and `span_id`
  - The custom `OpenTelemetryTraceEnricher` in Program.cs handles this correlation

## Prerequisites

1. **Datadog Account**: Sign up at https://www.datadoghq.com/
2. **Datadog API Key**: Get from https://app.datadoghq.com/organization-settings/api-keys
3. **Datadog Agent** (optional for local dev): For OTLP traces and metrics

## Configuration Steps

### 1. Set Your Datadog API Key

The API key is required to send logs to Datadog cloud.

#### Set Environment Variable (Recommended)

```bash
export DD_API_KEY="YOUR_API_KEY_HERE"
dotnet run --project src/App.Api
```

⚠️ **Security Warning**: Never commit real API keys to git! Use environment variables or user secrets instead.

### 2. Start the Local Datadog Agent (Optional)

The local agent is **optional** but recommended for:

- Viewing traces locally before they reach Datadog cloud
- Testing without sending data to cloud
- Using the Aspire Dashboard for local observability

```bash
# Make the script executable
chmod +x scripts/datadog-agent.sh

# Start the agent
./scripts/datadog-agent.sh
```

Verify agent is running:

```bash
docker ps | grep dd-agent
# or
podman ps | grep dd-agent
```

### 3. Run the Application

```bash
dotnet run --project src/App.Api
```

The application will:

1. Send **logs** directly to Datadog cloud via HTTPS (if DD_API_KEY is set)
2. Send **traces** via OTLP (to local agent at http://localhost:4318 if running, or nowhere if not)
3. Write logs locally to `src/App.Api/logs/app.log`

### 4. Generate Test Traffic

Create some activity to generate logs and traces:

```bash
# Health check
curl http://localhost:5112/health

# List todos
curl http://localhost:5112/api/v1/todos

# Create a todo
curl -X POST http://localhost:5112/api/v1/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Datadog Integration","isCompleted":false}'

# Get a Pokemon (generates external HTTP trace)
curl http://localhost:5112/api/v1/pokemon/25
```

### 5. View Logs and Traces in Datadog

#### APM Traces

1. Go to: https://us5.datadoghq.com/apm/traces
2. Filter by service: `hexagon-dotnet-app`
3. You should see traces for your API requests

#### Logs

1. Go to: https://us5.datadoghq.com/logs
2. Search for: `service:hexagon-dotnet-app`
3. You should see your application logs

#### Correlated Logs in Traces

1. Open any trace in APM
2. Look for the "Logs" tab or logs panel
3. You should see logs that occurred during that trace, linked by `trace_id`

## How the Integration Works

### OpenTelemetry (Traces)

- Configured in `src/App.ServiceDefaults/Extensions.cs`
- Captures ASP.NET Core, HTTP client, and Entity Framework traces
- Sends to local Datadog agent via OTLP
- Enriches traces with custom tags

### Serilog (Logs)

- Configured in `src/App.Api/Program.cs`
- Three sinks: Console, File, Datadog HTTP
- Custom `OpenTelemetryTraceEnricher` adds trace context:
  - `TraceId` and `SpanId` from OpenTelemetry Activity
  - `dd.trace_id` and `dd.span_id` in Datadog format (base-10)
- Logs are enriched with: application, environment, service name

### Correlation Magic

The custom enricher converts OpenTelemetry trace IDs to Datadog's numeric format, enabling:

- Logs to appear within APM trace views
- Click from logs to traces and vice versa
- Unified troubleshooting experience

## Troubleshooting

### Logs Not Appearing

**Check 1: DD_API_KEY is set**

```bash
echo $DD_API_KEY
```

If empty, set it as shown in step 1.

**Check 2: Application is running**

```bash
curl http://localhost:5112/health
```

**Check 3: Logs are being written locally**

```bash
cat src/App.Api/logs/app.log
```

**Check 4: Check application startup logs**
If DD_API_KEY is missing, the app crashes with:

```
InvalidOperationException: DD_API_KEY environment variable is required for Datadog logging
```

### Traces Not Appearing

**Check 1: Local Datadog agent is running**

```bash
docker ps | grep dd-agent
```

**Check 2: OTLP endpoint is reachable**

```bash
curl http://localhost:4318
```

**Check 3: Check agent logs**

```bash
docker logs dd-agent | grep -i otlp
```

**Check 4: Verify OpenTelemetry configuration**

```bash
# Check if OTLP endpoint environment variable is set:
echo $OTEL_EXPORTER_OTLP_ENDPOINT
# If not set, the app will use Aspire's automatic configuration
```

### Logs Not Correlated with Traces

**Symptom**: Logs appear in Logs Explorer but not within APM traces

**Cause**: The `OpenTelemetryTraceEnricher` might not be working

**Check**: Look in logs for `TraceId` and `dd.trace_id` fields:

```bash
cat src/App.Api/logs/app.log | grep -o '"TraceId":"[^"]*"'
```

If these fields are missing, the enricher isn't running correctly.

## Direct Cloud Configuration (No Local Agent)

If you don't want to run a local agent, you can send traces directly to Datadog cloud:

### Set Environment Variables

```bash
export DD_API_KEY="your-api-key"
export DD_SITE="us5.datadoghq.com"
export OTEL_EXPORTER_OTLP_ENDPOINT="https://trace.agent.us5.datadoghq.com:443"
export OTEL_EXPORTER_OTLP_HEADERS="dd-api-key=your-api-key"

dotnet run --project src/App.Api
```

⚠️ Replace `us5` with your actual Datadog site (check your Datadog URL).

## Metrics Integration

OpenTelemetry metrics are exported in **Prometheus format** at the `/metrics` endpoint. Datadog can scrape this endpoint to collect metrics.

### Quick Verification

```bash
# Check metrics are exposed
curl http://localhost:5112/metrics

# You'll see metrics like:
# http_server_request_duration_seconds
# http_server_active_requests
# process_memory_usage_bytes
# process_cpu_time_seconds_total
```

### Configure Datadog to Collect Metrics

Two options:

1. **Datadog Agent with Prometheus Check** (Recommended)
   - Configure agent to scrape http://localhost:5112/metrics
   - See detailed instructions: [DATADOG_METRICS.md](./DATADOG_METRICS.md)

2. **Kubernetes/Cloud**
   - Use Datadog Operator or ServiceMonitor annotations
   - Automatically discovers and scrapes metrics endpoints

📖 **Full Guide**: See [DATADOG_METRICS.md](./DATADOG_METRICS.md) for:

- Datadog Agent configuration
- Kubernetes integration
- Available metrics reference
- Troubleshooting tips

**Why not OTLP for metrics?** Datadog's OTLP endpoint only supports traces, not metrics. Prometheus format is the standard way to export OpenTelemetry metrics to Datadog.

## Alternative: Aspire Dashboard (Local Only)

For local development without Datadog, use the built-in Aspire Dashboard:

```bash
# Run the AppHost (includes dashboard)
dotnet run --project src/App.AppHost
```

Open: http://localhost:17123

This shows:

- All traces and spans
- Structured logs
- Metrics and counters
- Resource health

No API key or external service required!

## Summary

| Component                    | Purpose                 | Requires Agent | Requires API Key  |
| ---------------------------- | ----------------------- | -------------- | ----------------- |
| Logs → Datadog Cloud         | Send logs to Datadog    | ❌ No          | ✅ Yes            |
| Traces → Local Agent → Cloud | Send traces via agent   | ✅ Yes         | ⚠️ Agent needs it |
| Metrics → Prometheus         | Expose metrics endpoint | ❌ No          | ❌ No             |
| Metrics → Datadog            | Agent scrapes metrics   | ✅ Yes         | ⚠️ Agent needs it |
| Traces → Aspire Dashboard    | Local-only traces       | ❌ No          | ❌ No             |
| Log-Trace Correlation        | Link logs to traces     | ⚠️ Both needed | ⚠️ Both needed    |

**Minimal Setup**: Set `DD_API_KEY` → Logs work

**Full Setup**: Set `DD_API_KEY` + run agent → Logs + Traces + Metrics + Correlation work

**Local-Only**: Use Aspire Dashboard → Everything works locally, nothing sent to cloud
