# Local Datadog APM Configuration

This guide explains how to configure and use the local Datadog agent for Application Performance Monitoring (APM) with the Hexagon .NET App.

## Overview

The application uses **OpenTelemetry (OTEL)** to send traces and metrics to Datadog. You can configure it to send data either:

1. **Directly to Datadog Cloud** - For production or when you want data in Datadog immediately
2. **To a Local Datadog Agent** - For local development, testing, or when working offline

## Architecture

```
┌─────────────────────┐
│   .NET Application  │
│  (App.Api)          │
└──────────┬──────────┘
           │ OpenTelemetry
           │ (OTLP over HTTP)
           │
           ▼
┌─────────────────────┐
│  Datadog Agent      │
│  (localhost:4318)   │
│                     │
│  • OTLP Receiver    │◄── Traces (port 4318/4317)
│  • APM Processor    │◄── Native APM (port 8126)
│  • StatsD           │◄── Metrics (port 8125)
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│  Datadog Cloud      │
│  (us5.datadoghq.com)│
└─────────────────────┘
```

## Configuration

### Local Agent Setup (Current Configuration)

The application is currently configured to use the **local Datadog agent** for APM:

#### launchSettings.json

Located at: `src/App.Api/Properties/launchSettings.json`

```json
{
  "environmentVariables": {
    "DD_AGENT_HOST": "localhost",
    "DD_TRACE_AGENT_PORT": "8126",
    "DD_DOGSTATSD_PORT": "8125",
    "DD_ENV": "development",
    "DD_SERVICE": "hexagon-dotnet-app",
    "DD_VERSION": "1.0.0",
    "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4318",
    "OTEL_EXPORTER_OTLP_PROTOCOL": "http/protobuf",
    "OTEL_SERVICE_NAME": "hexagon-dotnet-app",
    "OTEL_RESOURCE_ATTRIBUTES": "deployment.environment=development,service.version=1.0.0"
  }
}
```

**Key Settings:**

- `OTEL_EXPORTER_OTLP_ENDPOINT`: Points to local agent's OTLP HTTP receiver
- `DD_AGENT_HOST`: Hostname of the Datadog agent
- `DD_TRACE_AGENT_PORT`: Port for native Datadog APM traces (8126)
- No API key required - the local agent handles forwarding to Datadog cloud

### Datadog Agent Configuration

Located at: `.devcontainer/datadog.yaml`

The agent is configured with:

```yaml
apm_config:
  enabled: true
  otlp_config:
    receiver:
      protocols:
        grpc:
          endpoint: 0.0.0.0:4317
        http:
          endpoint: 0.0.0.0:4318
```

**Ports:**

- `4317` - OTLP over gRPC
- `4318` - OTLP over HTTP (used by this application)
- `8125` - StatsD for metrics
- `8126` - Native Datadog APM

## Running the Local Agent

### Prerequisites

- Docker or Podman installed
- Datadog API key (optional for local-only development)

### Start the Agent

Run the provided script:

```bash
./run-datadog-agent.sh
```

This will:

1. Detect Docker or Podman
2. Stop any existing agent container
3. Start a new agent with OTLP receiver enabled
4. Expose ports 8125, 8126, 4317, and 4318

### Verify the Agent is Running

```bash
# Check container status
docker ps | grep dd-agent
# or
podman ps | grep dd-agent

# View agent logs
docker logs -f dd-agent

# Check agent status
docker exec dd-agent agent status

# Verify OTLP receiver is active
docker exec dd-agent agent status | grep -A 10 "OTLP"
```

## Sending Traces to the Agent

### 1. Start the Datadog Agent

```bash
./run-datadog-agent.sh
```

Wait for the agent to fully start (about 10-15 seconds).

### 2. Run Your Application

```bash
dotnet run --project src/App.Api
```

The application will automatically send traces to the local agent via OpenTelemetry.

### 3. View Traces

**Local Aspire Dashboard:**

```bash
dotnet run --project src/App.AppHost
```

Then open: http://localhost:17123

**Datadog Cloud Dashboard:**

If you provided a `DD_API_KEY`, traces will be forwarded to Datadog cloud after a few minutes:

- Go to: https://us5.datadoghq.com/apm/traces
- Filter by service: `hexagon-dotnet-app`

## Testing the Configuration

### Generate Test Traffic

```bash
# Health check
curl http://localhost:5112/health

# Get all todos
curl http://localhost:5112/todos

# Create a todo
curl -X POST http://localhost:5112/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Test APM","isCompleted":false}'

# Get a Pokemon (tests external HTTP calls)
curl http://localhost:5112/pokemon/pikachu
```

### Verify Traces in Agent

```bash
docker exec dd-agent agent status | grep -A 20 "APM Agent"
```

Look for:

- `Receiver Stats` showing received traces
- `Trace Writer` showing processed spans

## Switching Between Local and Cloud

### To Use Cloud Directly (Skip Local Agent)

Update `launchSettings.json`:

```json
{
  "environmentVariables": {
    "OTEL_EXPORTER_OTLP_ENDPOINT": "https://api.us5.datadoghq.com:443",
    "OTEL_EXPORTER_OTLP_PROTOCOL": "http/protobuf",
    "OTEL_EXPORTER_OTLP_HEADERS": "dd-api-key=YOUR_API_KEY",
    "OTEL_SERVICE_NAME": "hexagon-dotnet-app",
    "OTEL_RESOURCE_ATTRIBUTES": "deployment.environment=development,service.version=1.0.0"
  }
}
```

### To Use Local Agent (Current Configuration)

Use the configuration shown above in the "Local Agent Setup" section.

## Troubleshooting

### No Traces Appearing

1. **Check agent is running:**

   ```bash
   docker ps | grep dd-agent
   ```

2. **Verify OTLP receiver is listening:**

   ```bash
   netstat -an | grep 4318
   # or
   lsof -i :4318
   ```

3. **Check agent logs:**

   ```bash
   docker logs dd-agent | grep -i error
   docker logs dd-agent | grep -i otlp
   ```

4. **Verify application configuration:**
   ```bash
   # Make sure OTEL_EXPORTER_OTLP_ENDPOINT points to localhost:4318
   echo $OTEL_EXPORTER_OTLP_ENDPOINT
   ```

### Connection Refused Errors

If you see "connection refused" errors:

1. Ensure the agent is fully started (wait 10-15 seconds)
2. Check firewall settings
3. Verify ports are not already in use:
   ```bash
   lsof -i :8125 -i :8126 -i :4317 -i :4318
   ```

### Agent Not Forwarding to Cloud

1. **Check API key is set:**

   ```bash
   docker exec dd-agent env | grep DD_API_KEY
   ```

2. **Verify agent can reach Datadog:**

   ```bash
   docker exec dd-agent agent status | grep -A 5 "Forwarder"
   ```

3. **Check agent configuration:**
   ```bash
   docker exec dd-agent agent configcheck
   ```

## Performance Considerations

### Local Development

- **Pros:**
  - Works offline
  - Faster trace collection (no internet latency)
  - Better privacy (data stays local until forwarded)
  - Can inspect traces before they go to cloud

- **Cons:**
  - Requires Docker/Podman
  - Uses local resources (CPU, memory)
  - Extra container to manage

### Direct to Cloud

- **Pros:**
  - No local agent needed
  - Simpler setup
  - Immediate visibility in Datadog

- **Cons:**
  - Requires internet connection
  - Slower in high-latency networks
  - All data sent directly to cloud

## Environment Variables Reference

### Datadog-Specific

| Variable              | Description      | Example              |
| --------------------- | ---------------- | -------------------- |
| `DD_API_KEY`          | Datadog API key  | `7330e4f63...`       |
| `DD_SITE`             | Datadog site     | `us5.datadoghq.com`  |
| `DD_ENV`              | Environment name | `development`        |
| `DD_SERVICE`          | Service name     | `hexagon-dotnet-app` |
| `DD_VERSION`          | Service version  | `1.0.0`              |
| `DD_AGENT_HOST`       | Agent hostname   | `localhost`          |
| `DD_TRACE_AGENT_PORT` | APM port         | `8126`               |
| `DD_DOGSTATSD_PORT`   | StatsD port      | `8125`               |

### OpenTelemetry

| Variable                      | Description         | Example                      |
| ----------------------------- | ------------------- | ---------------------------- |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OTLP endpoint       | `http://localhost:4318`      |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | Protocol            | `http/protobuf`              |
| `OTEL_SERVICE_NAME`           | Service name        | `hexagon-dotnet-app`         |
| `OTEL_RESOURCE_ATTRIBUTES`    | Resource attributes | `deployment.environment=dev` |

## Related Documentation

- [Datadog Logging Setup](DATADOG_LOGGING.md)
- [Datadog OTLP Documentation](https://docs.datadoghq.com/opentelemetry/)
- [.NET OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)

## Summary

The application is now configured to use the **local Datadog agent** for APM. This provides:

✅ Offline development capability  
✅ Faster trace collection  
✅ Local trace inspection  
✅ Privacy-first approach  
✅ Easy switching between local/cloud

To start developing with local APM:

```bash
# 1. Start the Datadog agent
./run-datadog-agent.sh

# 2. Run your application
dotnet run --project src/App.Api

# 3. Generate some traffic
curl http://localhost:5112/todos

# 4. View traces in Aspire Dashboard
dotnet run --project src/App.AppHost
# Open: http://localhost:17123
```
