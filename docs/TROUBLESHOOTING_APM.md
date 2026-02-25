# Troubleshooting: APM Logs Not Appearing in Datadog

## Understanding the Data Flow

You have **two separate log/trace pipelines**:

### 1. **Serilog Logs** → Datadog Logs (Direct HTTP)

- **From**: `Program.cs` Serilog configuration
- **To**: `https://http-intake.logs.us5.datadoghq.com`
- **Contains**: Application logs with trace correlation (TraceId, dd.trace_id)
- **Status**: ✅ This should work if DD_API_KEY is set

### 2. **OpenTelemetry Traces** → Datadog APM (OTLP)

- **From**: `ServiceDefaults/Extensions.cs` OpenTelemetry configuration
- **To**: `https://api.us5.datadoghq.com/v1/traces`
- **Contains**: Performance traces, spans, distributed tracing
- **Status**: ⚠️ This depends on correct OTLP configuration

## Common Issues

### Issue 1: Traces Not Appearing in APM

**Symptoms:**

- Logs appear in **Datadog Logs** but not in **APM Traces**
- No traces visible at https://us5.datadoghq.com/apm/traces

**Causes:**

1. OTLP endpoint not configured correctly
2. API key not in OTLP headers
3. Firewall/network blocking OTLP traffic
4. Wrong Datadog site (us5 vs us1, eu1, etc.)

**Solution:**
Check your environment variables in `launchSettings.json`:

```json
{
  "environmentVariables": {
    "OTEL_EXPORTER_OTLP_ENDPOINT": "https://api.us5.datadoghq.com",
    "OTEL_EXPORTER_OTLP_PROTOCOL": "http/protobuf",
    "OTEL_EXPORTER_OTLP_HEADERS": "dd-api-key=YOUR_ACTUAL_API_KEY"
  }
}
```

### Issue 2: Logs Not Correlated with Traces

**Symptoms:**

- Traces appear in APM
- Logs appear in Logs Explorer
- But clicking on a trace doesn't show related logs

**Causes:**

1. OpenTelemetryTraceEnricher not working
2. Trace IDs not in correct format for Datadog
3. Service names don't match between logs and traces

**Solution:**

1. Check logs contain `dd.trace_id` and `dd.span_id`:

   ```bash
   cat logs/app.log | grep "dd.trace_id"
   ```

2. Verify service names match:
   - Logs: `service:hexagon-dotnet-app`
   - Traces: `service:hexagon-dotnet-app`

### Issue 3: Nothing Appears in Datadog

**Symptoms:**

- No logs
- No traces
- Nothing in Datadog at all

**Causes:**

1. DD_API_KEY not set or invalid
2. Wrong Datadog site
3. Application not generating traffic

**Solution:**

1. Verify API key:

   ```bash
   curl -X POST "https://http-intake.logs.us5.datadoghq.com/api/v2/logs" \
     -H "Content-Type: application/json" \
     -H "DD-API-KEY: YOUR_API_KEY" \
     -d '[{"message":"test","service":"test"}]'
   ```

   Should return `{"status":"ok"}` or similar.

2. Generate traffic:

   ```bash
   curl http://localhost:5112/todos
   curl http://localhost:5112/pokemon/pikachu
   ```

3. Wait 1-2 minutes for data to appear

## Diagnostic Steps

### Step 1: Check Application Logs

```bash
dotnet run --project src/App.Api
```

Look for:

```
[OpenTelemetry] Configuring OTLP Exporter to: https://api.us5.datadoghq.com
[OpenTelemetry] Service: hexagon-dotnet-app
[OpenTelemetry] Exporters configured for traces, metrics, and logs
```

### Step 2: Check Local Log File

```bash
cat logs/app.log | tail -20
```

Look for fields:

- `"TraceId": "..."` - W3C format
- `"dd.trace_id": "..."` - Datadog format (numeric)
- `"service": "hexagon-dotnet-app"`

### Step 3: Test Trace Generation

```bash
# Generate a request
curl -v http://localhost:5112/todos

# Check if OpenTelemetry Activity is created
# Log should show TraceId in output
```

### Step 4: Verify Datadog Configuration

```bash
# Check environment
env | grep -E "DD_|OTEL_"
```

Should show:

```
DD_API_KEY=7330e4f6...
DD_SITE=us5.datadoghq.com
OTEL_EXPORTER_OTLP_ENDPOINT=https://api.us5.datadoghq.com
OTEL_EXPORTER_OTLP_HEADERS=dd-api-key=7330e4f6...
OTEL_SERVICE_NAME=hexagon-dotnet-app
```

### Step 5: Check Datadog Dashboards

**Logs** (should appear immediately):
https://us5.datadoghq.com/logs?query=service%3Ahexagon-dotnet-app

**APM Traces** (may take 1-2 minutes):
https://us5.datadoghq.com/apm/traces?query=service%3Ahexagon-dotnet-app

**APM Service** Page:
https://us5.datadoghq.com/apm/services

## Expected Behavior

When working correctly:

1. **Application starts** → See OpenTelemetry configuration messages
2. **Request arrives** → OpenTelemetry creates Activity/Span
3. **Log written** → Enricher adds TraceId and dd.trace_id
4. **Serilog** → Sends log to Datadog Logs API
5. **OpenTelemetry** → Sends trace to Datadog APM API
6. **Datadog** → Correlates logs and traces by dd.trace_id

Result: Click on trace in APM → See related logs

## Still Not Working?

### Enable Debug Logging

Add to `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "OpenTelemetry": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Check for Errors

```bash
dotnet run --project src/App.Api 2>&1 | tee app-debug.log
```

Look for:

- `Failed to export` - OTLP export errors
- `Unable to send logs` - Serilog errors
- `401 Unauthorized` - API key issues
- `403 Forbidden` - API key invalid for site

### Test with Local Agent

If cloud endpoint isn't working, try local agent:

1. Start Datadog agent:

   ```bash
   ./scripts/datadog-agent.sh
   ```

2. Update launchSettings.json:

   ```json
   "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4318"
   ```

3. Remove HEADERS (agent doesn't need API key for OTLP):

   ```json
   // Remove or comment out:
   // "OTEL_EXPORTER_OTLP_HEADERS": "..."
   ```

4. Agent forwards to Datadog cloud

## Quick Verification Script

```bash
#!/bin/bash
echo "==> Checking Datadog Configuration"

echo "1. DD_API_KEY: ${DD_API_KEY:+SET (len=${#DD_API_KEY})}"
echo "2. OTLP Endpoint: $OTEL_EXPORTER_OTLP_ENDPOINT"
echo "3. OTLP Headers: ${OTEL_EXPORTER_OTLP_HEADERS:+SET}"

echo ""
echo "==> Testing Logs API"
curl -s -X POST "https://http-intake.logs.us5.datadoghq.com/api/v2/logs" \
  -H "DD-API-KEY: $DD_API_KEY" \
  -H "Content-Type: application/json" \
  -d '[{"message":"test from script","service":"hexagon-dotnet-app"}]' | jq '.'

echo ""
echo "==> Checking local log file"
if [ -f logs/app.log ]; then
  echo "Last log entry:"
  tail -1 logs/app.log | jq '.'
else
  echo "❌ No log file found"
fi

echo ""
echo "==> App running?"
curl -s http://localhost:5112/health >/dev/null && echo "✅ App is running" || echo "❌ App not responding"
```

## Contact Datadog Support

If still having issues, provide:

1. Your Datadog site (us5.datadoghq.com)
2. Service name (hexagon-dotnet-app)
3. Time range when you sent data
4. Error messages from application logs
5. Output from diagnostic steps above
