# Datadog APM Configuration Guide

## Understanding the OTLP Endpoint

When sending traces directly to Datadog cloud (not using a local agent), the correct OTLP endpoint is:

**✅ CORRECT:**

```
OTEL_EXPORTER_OTLP_ENDPOINT=https://trace.agent.us5.datadoghq.com:443
```

**❌ INCORRECT (Returns 404):**

```
OTEL_EXPORTER_OTLP_ENDPOINT=https://api.us5.datadoghq.com
```

### Why This Matters

The `api.us5.datadoghq.com` endpoint returns **404 Not Found** errors when trying to send traces:

```
"StatusCode":404
"Uri":"https://api.us5.datadoghq.com/v1/traces"
```

The correct endpoint `trace.agent.us5.datadoghq.com:443` returns **202 Accepted** (success):

```
"StatusCode":202
"Uri":"https://trace.agent.us5.datadoghq.com/v1/traces"
```

## Configuration Options

There are multiple ways to configure Datadog APM for this application:

### Option 1: With .NET Aspire (Recommended for Development)

The simplest approach using Aspire orchestration:

```bash
dotnet run --project src/App.AppHost
```

The Aspire dashboard automatically collects traces and displays them at `http://localhost:17123`.

### Option 2: Direct to Datadog Cloud

Set environment variables to send traces directly to Datadog:

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT="https://trace.agent.us5.datadoghq.com:443"
export OTEL_EXPORTER_OTLP_PROTOCOL="http/protobuf"
export OTEL_EXPORTER_OTLP_HEADERS="dd-api-key=YOUR_DD_API_KEY"
export DD_API_KEY="YOUR_DD_API_KEY"
export DD_SITE="us5.datadoghq.com"

dotnet run --project src/App.Api
```

⚠️ **Replace `YOUR_DD_API_KEY`** with your actual Datadog API key.

### Option 3: Via Local Datadog Agent

For local testing with an agent:

```bash
# Terminal 1: Start the local agent
./scripts/datadog-agent.sh

# Terminal 2: Configure and run the app
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4318"
dotnet run --project src/App.Api
```

## How to Verify It's Working

### 1. Start the Application

Choose the appropriate setup based on your configuration:

**Option A: With .NET Aspire (Recommended for Development)**

```bash
dotnet run --project src/App.AppHost
```

Open the Aspire Dashboard at `http://localhost:17123` to view traces.

**Option B: With Datadog Cloud**

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT="https://trace.agent.us5.datadoghq.com:443"
export OTEL_EXPORTER_OTLP_HEADERS="dd-api-key=YOUR_DD_API_KEY"
export DD_API_KEY="YOUR_DD_API_KEY"

dotnet run --project src/App.Api
```

**Option C: With Local Agent**

```bash
# Terminal 1: Start agent
./scripts/datadog-agent.sh

# Terminal 2: Run app
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4318"
dotnet run --project src/App.Api
```

### 2. Check Startup Messages

You should see OpenTelemetry configuration messages:

**With Datadog Cloud:**

```
[OpenTelemetry] Configuring OTLP Exporter to: https://trace.agent.us5.datadoghq.com:443
[OpenTelemetry] Service: hexagon-dotnet-app
[OpenTelemetry] OTLP exporter configured for TRACES only (cloud endpoint)
[INF] Starting Hexagon .NET App
```

**With Local Agent:**

```
[OpenTelemetry] Configuring OTLP Exporter to: http://localhost:4318
[OpenTelemetry] Service: hexagon-dotnet-app
[OpenTelemetry] OTLP exporter configured for TRACES and METRICS (local agent)
[INF] Starting Hexagon .NET App
```

**With Aspire:**

```
[OpenTelemetry] Running under Aspire - using automatic OTLP configuration
[OpenTelemetry] Traces and Metrics will be exported to Aspire dashboard
[INF] Starting Hexagon .NET App
```

### 3. Generate Traffic

```bash
# Health check
curl http://localhost:5112/health

# List todos
curl http://localhost:5112/api/v1/todos

# Create todos
curl -X POST http://localhost:5112/api/v1/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Test APM Integration","isCompleted":false}'

# Get Pokemon (external HTTP call)
curl http://localhost:5112/api/v1/pokemon/25
```

### 4. Verify Traces Are Being Sent

**For Datadog Cloud Profile:**

Check application logs for OTLP export status:

```bash
tail -20 src/App.Api/logs/app$(date +%Y%m%d).log | grep StatusCode
```

Should show:

```
"StatusCode":202
"StatusCode":202
...
```

**202 = Success!** Traces are being accepted by Datadog cloud.

**For Local Agent Profile:**

Check agent logs:

```bash
docker logs dd-agent | grep "trace" | tail -20
# or with podman:
podman logs dd-agent | grep "trace" | tail -20
```

**For Aspire:**

Open the Aspire Dashboard at `http://localhost:17123` and navigate to the **Traces** section.

### 5. View in Datadog (Wait 1-2 Minutes)

**APM Traces:**
https://us5.datadoghq.com/apm/traces?query=service%3Ahexagon-dotnet-app

**Logs:**
https://us5.datadoghq.com/logs?query=service%3Ahexagon-dotnet-app

**Service Map:**
https://us5.datadoghq.com/apm/services

**APM Service Page:**
https://us5.datadoghq.com/apm/service/hexagon-dotnet-app

## What's Being Sent

### 1. Traces (via OpenTelemetry OTLP)

- **Destination**: `https://trace.agent.us5.datadoghq.com:443/v1/traces`
- **Contains**: Request spans, external HTTP calls, database queries
- **Frequency**: Every 5-10 seconds (batched)

### 2. Logs (via Serilog HTTP)

- **Destination**: `https://http-intake.logs.us5.datadoghq.com`
- **Contains**: Application logs with trace correlation
- **Enrichment**: TraceId, dd.trace_id, dd.span_id, service

### 3. Metrics (via OpenTelemetry OTLP)

- **Destination**: `https://trace.agent.us5.datadoghq.com:443/v1/metrics`
- **Contains**: Runtime metrics, HTTP metrics, process metrics
- **Frequency**: Every 10 seconds

### 4. Trace Correlation

- Serilog logs include `dd.trace_id` and `dd.span_id`
- Datadog automatically correlates logs with traces
- Click on a trace → see related logs
- Click on a log → see related trace

## Datadog OTLP Endpoints by Region

If you're in a different Datadog region, use the appropriate endpoint:

| Region | OTLP Endpoint                               |
| ------ | ------------------------------------------- |
| US1    | `https://trace.agent.datadoghq.com:443`     |
| US3    | `https://trace.agent.us3.datadoghq.com:443` |
| US5    | `https://trace.agent.us5.datadoghq.com:443` |
| EU1    | `https://trace.agent.datadoghq.eu:443`      |
| AP1    | `https://trace.agent.ap1.datadoghq.com:443` |

## Troubleshooting

### Still Getting 404 Errors?

Check your Datadog site:

1. Go to https://app.datadoghq.com
2. Note the URL (us5, us1, eu, etc.)
3. Update `DD_SITE` and `OTEL_EXPORTER_OTLP_ENDPOINT` accordingly

### Traces Not Appearing?

1. **Wait 1-2 minutes** - Datadog ingestion has a delay
2. **Check the time range** in Datadog UI (last 15 minutes)
3. **Verify API key** is correct
4. **Check firewall** - ensure outbound HTTPS to `*.datadoghq.com` is allowed

### Logs Not Correlated?

Correlation requires:

- Logs have `dd.trace_id` field
- Service names match between logs and traces
- Both use the same API key

Check correlation fields:

```bash
cat src/App.Api/logs/app*.log | jq 'select(.Properties."dd.trace_id" != null)' | head -5
```

## Performance Impact

OpenTelemetry and Serilog are highly optimized:

- **Traces**: Async batched export, minimal overhead
- **Logs**: Buffered writes, async I/O
- **Typical overhead**: < 1% CPU, < 10MB memory

## What You'll See in Datadog

### APM Service Page

- Request rate, latency percentiles, error rate
- Service dependencies (e.g., App.Api → PokeAPI)
- Endpoint breakdowns (/todos, /pokemon/{name})

### Traces

- Full request flow with timing
- Database queries with actual SQL
- External API calls
- Errors with stack traces

### Logs

- Structured JSON logs
- Correlated with traces (click to jump)
- Filterable by service, environment, level

### Service Map

- Visual graph of service dependencies
- Automatic discovery of external services

## Next Steps

1. **Create Monitors**: Alert on high latency or errors
2. **Build Dashboards**: Custom views of your metrics
3. **Set up SLOs**: Track reliability targets
4. **Add Custom Tags**: Enrich traces with business context

## Summary

✅ **Fixed**: Changed OTLP endpoint from `api.us5.datadoghq.com` to `trace.agent.us5.datadoghq.com:443`  
✅ **Result**: Traces now accepted (202) instead of rejected (404)  
✅ **Status**: APM and logging fully operational

Your application is now sending complete telemetry to Datadog! 🎉
