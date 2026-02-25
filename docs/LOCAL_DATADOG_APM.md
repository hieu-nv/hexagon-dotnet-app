# Local Datadog APM Configuration

This guide explains how to configure and use the local Datadog agent for Application Performance Monitoring (APM) with the Hexagon .NET App.

## Overview

The application uses **OpenTelemetry (OTEL)** to send traces and metrics to Datadog.

### Default Configuration: Local Agent Mode

**The application is configured by default** (in `.env` file) to use a **local Datadog agent**:

```dotenv
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
```

**Architecture**: Application → Local Agent → Datadog Cloud (optional)

You can configure it to send data either:

1. **To a Local Datadog Agent** (default) - For local development, testing, or when working offline
2. **Directly to Datadog Cloud** (alternative) - For production or when you want data in Datadog immediately

### Instrumentation Details

The application includes comprehensive OpenTelemetry instrumentation:

- **ASP.NET Core Instrumentation**: HTTP request/response tracing with enriched metadata
- **HTTP Client Instrumentation**: Outgoing HTTP request tracing (PokeAPI calls)
- **Entity Framework Core Instrumentation**: Database query execution tracing with command text
- **SQL Client Instrumentation**: Low-level database query tracing with execution time
- **Runtime Instrumentation**: .NET CLR metrics (GC, memory, thread pool)
- **Process Instrumentation**: Process-level metrics (CPU, memory usage)

All instrumentation is configured in `src/App.ServiceDefaults/Extensions.cs` following the **Aspire service defaults** pattern.

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

### Default Configuration (Local Agent via .env)

**The application is preconfigured in `.env` to use a local Datadog agent**:

```dotenv
# From .env file
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_SERVICE_NAME=hexagon-dotnet-app
DD_ENV=development
DD_SERVICE=hexagon-dotnet-app
```

When running via Aspire orchestration:

```bash
dotnet run --project src/App.AppHost
```

Traces flow through **both** paths:

1. **Local Agent** (`localhost:4318`) → Datadog Cloud (if `DD_API_KEY` is set)
2. **Aspire Dashboard** (`localhost:17123`) → Local viewing

> **Note**: The `.env` file is loaded automatically by Aspire orchestration. You don't need to export variables manually unless running `src/App.Api` directly.

### Manual Configuration (Without Aspire)

If running `App.Api` directly without Aspire orchestration:

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4318"
export OTEL_EXPORTER_OTLP_PROTOCOL="http/protobuf"
export OTEL_SERVICE_NAME="hexagon-dotnet-app"
export OTEL_RESOURCE_ATTRIBUTES="deployment.environment=development,service.version=1.0.0"
export DD_ENV="development"
export DD_SERVICE="hexagon-dotnet-app"
export DD_VERSION="1.0.0"

dotnet run --project src/App.Api
```

**Key Settings:**

- `OTEL_EXPORTER_OTLP_ENDPOINT`: Points to local agent's OTLP HTTP receiver
- `OTEL_SERVICE_NAME`: Service name for traces
- `DD_ENV`: Environment label (development, staging, production)
- No API key required - the local agent handles forwarding to Datadog cloud

### Datadog Agent Configuration

Located at: `scripts/datadog-agent/datadog.yaml`

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
./scripts/datadog-agent.sh
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
./scripts/datadog-agent.sh
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
curl http://localhost:5112/api/v1/todos

# Create a todo
curl -X POST http://localhost:5112/api/v1/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Test APM","isCompleted":false}'

# Get a Pokemon (tests external HTTP calls)
curl http://localhost:5112/api/v1/pokemon/25
```

### Verify Traces in Agent

```bash
docker exec dd-agent agent status | grep -A 20 "APM Agent"
```

Look for:

- `Receiver Stats` showing received traces
- `Trace Writer` showing processed spans

## What Traces You'll See in Datadog

After generating traffic, you should see distributed traces with the following spans:

### HTTP Request Trace Example

```
GET /api/v1/todos
├── ASP.NET Core HTTP GET /api/v1/todos (root span)
│   ├── http.request.method: GET
│   ├── http.request.path: /api/v1/todos
│   ├── http.response.status_code: 200
│   └── EF Core SELECT operation
│       ├── db.execution_time_ms: 5.2
│       ├── db.statement: SELECT * FROM TODOS
│       └── SQL Client Query
│           ├── db.system: sqlite
│           └── db.execution_time: 5ms
```

### External API Call Trace Example

```
GET /api/v1/pokemon/25
├── ASP.NET Core HTTP GET /api/v1/pokemon/{id} (root span)
│   └── HTTP Client GET https://pokeapi.co/api/v2/pokemon/25
│       ├── http.request.uri: https://pokeapi.co/api/v2/pokemon/25
│       ├── http.response.status_code: 200
│       └── duration: 250ms
```

### Metrics Available

- **Request Rate**: Requests per second by endpoint
- **Error Rate**: 4xx/5xx responses
- **Latency**: p50, p75, p95, p99 response times
- **Database Query Time**: Average query execution time
- **External API Latency**: Time spent calling external services
- **GC Metrics**: Garbage collection frequency and duration
- **Memory Usage**: Heap size, allocations
- **Thread Pool**: Active threads, queue length

### Trace Context and Enrichment

All spans include:

- `service.name`: hexagon-dotnet-app
- `service.version`: 1.0.0
- `deployment.environment`: development
- `service.namespace`: hexagon
- `host.name`: Your machine name
- `process.runtime.name`: .NET
- `process.runtime.version`: 10.0.x
- `telemetry.sdk.language`: dotnet

## Switching Between Local and Cloud

### Default: Local Agent (Configured in .env)

The application is **preconfigured to use the local agent** via the `.env` file:

```bash
dotnet run --project src/App.AppHost  # Uses .env automatically
```

This sends traces to:

- Local Agent at `http://localhost:4318` (buffered, supports traces + metrics)
- Aspire Dashboard at `http://localhost:17123` (local viewing)
- Datadog Cloud (if agent has `DD_API_KEY` configured)

### Alternative: Direct to Cloud (Skip Local Agent)

To bypass the local agent and send traces directly to Datadog cloud, override the `.env` configuration:

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT="https://trace.agent.us5.datadoghq.com:443"
export OTEL_EXPORTER_OTLP_PROTOCOL="http/protobuf"
export OTEL_EXPORTER_OTLP_HEADERS="dd-api-key=YOUR_API_KEY"
export DD_API_KEY="YOUR_API_KEY"
export OTEL_SERVICE_NAME="hexagon-dotnet-app"
export OTEL_RESOURCE_ATTRIBUTES="deployment.environment=development,service.version=1.0.0"

dotnet run --project src/App.Api
```

**Note**: Direct-to-cloud mode only supports OTLP traces. The local agent supports both traces and metrics. See `src/App.ServiceDefaults/Extensions.cs` (lines 185-209) for the endpoint detection logic.

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
| `DD_API_KEY`          | Datadog API key  | `=********...`       |
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
curl http://localhost:5112/api/v1/todos

# 4. View traces in Aspire Dashboard
dotnet run --project src/App.AppHost
# Open: http://localhost:17123
```
