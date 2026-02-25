# Datadog Log Forwarding Setup

This document describes how logs are forwarded from the Hexagon .NET App to the Datadog agent.

## Architecture

The application uses **Serilog** with multiple sinks to forward logs:

1. **Console Sink**: Logs to console for development/debugging
2. **File Sink**: Writes JSON-formatted logs to `src/App.Api/logs/app.log`
3. **Datadog Sink**: Sends logs directly to Datadog cloud via HTTP (when DD_API_KEY is set)

## Configuration

### Application Configuration

The application is configured in [Program.cs](../src/App.Api/Program.cs) with:

- **Minimum log level**: Information (Warning for Microsoft.AspNetCore)
- **Enrichment**: Application name, environment, service name, context, **trace_id and span_id** from OpenTelemetry
- **File logging**: JSON format with daily rolling, 7-day retention at `src/App.Api/logs/app.log`
- **Datadog HTTP Intake**: Direct to Datadog cloud at `https://http-intake.logs.us5.datadoghq.com`

**Note**: Logs are sent directly to Datadog's cloud intake API when `DD_API_KEY` is set. The local agent (when running) can also collect logs via OTLP or file tailing.

### Datadog Agent Configuration (Optional)

When using a local Datadog agent, it can collect logs through multiple methods:

1. **OTLP Logs**: Direct integration via OpenTelemetry (port 4318)
2. **File Tailing**: Reading from `src/App.Api/logs/app*.log` (requires volume mount)
3. **StatsD metrics**: Port 8125

Configuration file: `scripts/datadog-agent/datadog.yaml`

## Running the Setup

### Option 1: Direct to Datadog Cloud (No Agent Required)

This is the simplest setup for development:

1. **Set your API key** as an environment variable:

   ```bash
   export DD_API_KEY="your-api-key-here"
   ```

2. **Run the application:**
   ```bash
   dotnet run --project src/App.Api
   ```

Logs will be sent directly to Datadog cloud via HTTPS.

### Option 2: With Local Datadog Agent

For local testing with an agent:

### 1. Start the Datadog Agent

```bash
./scripts/datadog-agent.sh
```

This script:

- Starts the Datadog agent container
- Exposes ports 4317, 4318 (OTLP), 8125 (StatsD) and 8126 (APM)
- Configures the agent for local development

### 2. Verify the Agent is Running

```bash
# Check if agent is running (use docker or podman depending on your setup)
docker ps | grep dd-agent
# or
podman ps | grep dd-agent

# View agent logs
docker logs -f dd-agent
# or
podman logs -f dd-agent
```

### 3. Run the Application

```bash
# If sending logs to Datadog cloud, set DD_API_KEY
export DD_API_KEY="your-api-key-here"

dotnet run --project src/App.Api
```

### 4. Verify Logs are Being Forwarded

Check the application is writing logs locally:

```bash
tail -f src/App.Api/logs/app*.log
```

If using local agent, check agent is receiving logs:

```bash
docker exec dd-agent agent status | grep -A 20 "Logs Agent"
# or
podman exec dd-agent agent status | grep -A 20 "Logs Agent"
```

## Log Format

Logs are written in JSON format with the following structure:

```json
{
  "@t": "2026-02-24T10:30:45.1234567Z",
  "@mt": "Starting Hexagon .NET App",
  "@l": "Information",
  "Application": "App.Api",
  "Environment": "Development",
  "service": "hexagon-dotnet-app",
  "SourceContext": "Program",
  "SpanId": "a1b2c3d4e5f6g7h8",
  "TraceId": "0af7651916cd43dd8448eb211c80319c",
  "ParentId": null
}
```

The `SpanId`, `TraceId`, and `ParentId` fields are automatically populated by the `Serilog.Enrichers.Span` package, which extracts them from the current OpenTelemetry Activity. This enables correlation between logs and APM traces in Datadog.

## Viewing Logs in Datadog

### Access the Logs Dashboard

1. Navigate to Datadog UI: https://us5.datadoghq.com
2. Go to **Logs** → **Logs Explorer** (or use direct link: https://us5.datadoghq.com/logs)
3. Wait 1-2 minutes for logs to appear (there's a slight ingestion delay)

### Filter Your Logs

Use these filters in the search bar:

```
service:hexagon-dotnet-app
```

Or combine multiple filters:

```
service:hexagon-dotnet-app source:csharp env:Development
```

### Available Log Attributes

- **service**: `hexagon-dotnet-app`
- **source**: `csharp`
- **env**: `Development` (or `Production`)
- **Application**: `App.Api`
- **host**: Your machine name
- **version**: `1.0.0`

### Troubleshooting Missing Logs

If logs don't appear after 2-3 minutes:

1. **Verify API Key**: Ensure `DD_API_KEY` is set in your environment or launchSettings.json
2. **Check Network**: Logs are sent to `https://http-intake.logs.us5.datadoghq.com`
3. **Verify Application**: Run `curl http://localhost:5112/health` to ensure app is running
4. **Check Log Files**: Verify logs are being written locally:
   ```bash
   tail -f src/App.Api/logs/app*.log
   ```

## Troubleshooting

### Logs not appearing in Datadog

1. **Verify DD_API_KEY is set:**

   ```bash
   echo $DD_API_KEY
   ```

2. **Check application logs for errors:**

   ```bash
   cat src/App.Api/logs/app*.log | grep -i error
   ```

3. **If using local agent, check agent status:**

   ```bash
   docker exec dd-agent agent status
   # or
   podman exec dd-agent agent status
   ```

4. **Test direct log submission:**
   ```bash
   curl -X POST "https://http-intake.logs.us5.datadoghq.com/api/v2/logs" \
     -H "Content-Type: application/json" \
     -H "DD-API-KEY: $DD_API_KEY" \
     -d '[{"message":"test","service":"hexagon-dotnet-app"}]'
   ```
   Should return `{"status":"ok"}` or similar.

## Configuration Files

- **Application logging**: [src/App.Api/Program.cs](../src/App.Api/Program.cs)
- **Log settings**: [src/App.Api/appsettings.json](../src/App.Api/appsettings.json)
- **Launch profiles**: [src/App.Api/Properties/launchSettings.json](../src/App.Api/Properties/launchSettings.json)
- **Service defaults (OTel config)**: [src/App.ServiceDefaults/Extensions.cs](../src/App.ServiceDefaults/Extensions.cs)
- **Agent startup script**: [scripts/datadog-agent.sh](../scripts/datadog-agent.sh)
- **Agent configuration**: [scripts/datadog-agent/datadog.yaml](../scripts/datadog-agent/datadog.yaml)

## NuGet Packages Used

- `Serilog.AspNetCore` - Serilog integration for ASP.NET Core
- `Serilog.Sinks.Datadog.Logs` - Direct Datadog HTTP logging
- `Serilog.Sinks.File` - File-based logging with daily rolling
- `Serilog.Enrichers.Span` - OpenTelemetry trace context enrichment
