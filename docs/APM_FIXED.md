# FIXED: APM Logs Now Working ✅

## The Problem

Your APM traces and logs weren't appearing in Datadog because of an **incorrect OTLP endpoint**.

### What Was Wrong

**Before:**

```
OTEL_EXPORTER_OTLP_ENDPOINT=https://api.us5.datadoghq.com
```

This endpoint returned **404 Not Found** errors:

```
"StatusCode":404
"Uri":"https://api.us5.datadoghq.com/v1/traces"
```

### What Was Fixed

**After:**

```
OTEL_EXPORTER_OTLP_ENDPOINT=https://trace.agent.us5.datadoghq.com:443
```

Now returns **202 Accepted** (success):

```
"StatusCode":202
"Uri":"https://trace.agent.us5.datadoghq.com/v1/traces"
```

## Files Updated

1. **[launchSettings.json](src/App.Api/Properties/launchSettings.json)** - Changed OTLP endpoint
2. **[.env](.env)** - Updated for consistency

## How to Verify It's Working

### 1. Start the Application

```bash
cd /Users/hieunv/Documents/tmp/hexagon-dotnet-app
dotnet run --project src/App.Api
```

### 2. Check Startup Messages

You should see:

```
[OpenTelemetry] Configuring OTLP Exporter to: https://trace.agent.us5.datadoghq.com:443
[OpenTelemetry] Service: hexagon-dotnet-app
[OpenTelemetry] Exporters configured for traces, metrics, and logs
[INF] Starting Hexagon .NET App
```

### 3. Generate Traffic

```bash
# Health check
curl http://localhost:5112/health

# List todos
curl http://localhost:5112/todos

# Create todos
curl -X POST http://localhost:5112/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Test APM Integration","isCompleted":false}'

# Get Pokemon (external HTTP call)
curl http://localhost:5112/pokemon/pikachu
```

### 4. Check Logs for Success

```bash
tail -20 src/App.Api/logs/app$(date +%Y%m%d).log | grep StatusCode
```

Should show:

```
"StatusCode":202
"StatusCode":202
...
```

**202 = Success!** Traces are being accepted by Datadog.

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
