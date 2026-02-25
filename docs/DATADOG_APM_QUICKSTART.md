# Datadog APM Quickstart

Get application performance monitoring (APM) running in 5 minutes.

## Prerequisites

- Docker or Podman installed
- .NET 10 SDK installed
- (Optional) Datadog API key for cloud integration

## Quick Start

### 1. Start the Datadog Agent

```bash
./run-datadog-agent.sh
```

This starts a local Datadog agent with OpenTelemetry support on ports:

- `4318` - OTLP HTTP (used by this app)
- `4317` - OTLP gRPC
- `8126` - Datadog APM
- `8125` - StatsD metrics

### 2. Run the Application

```bash
cd src/App.Api
dotnet run
```

The application is preconfigured with OpenTelemetry and will automatically send traces to the local agent.

### 3. Generate Test Traffic

```bash
# Health check
curl http://localhost:5112/health

# Create a todo (tests database operations)
curl -X POST http://localhost:5112/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Datadog APM","isCompleted":false}'

# Get todos
curl http://localhost:5112/todos

# Get a Pokemon (tests external HTTP calls and caching)
curl http://localhost:5112/pokemon/pikachu
curl http://localhost:5112/pokemon/charizard

# Search Pokemon (tests pagination)
curl http://localhost:5112/pokemon?limit=20&offset=0
```

### 4. View Traces

#### Option A: Aspire Dashboard (Local)

```bash
cd src
dotnet run --project App.AppHost
```

Open http://localhost:17123 and navigate to the **Traces** section.

#### Option B: Datadog Cloud Dashboard

If you provided `DD_API_KEY` to the agent:

1. Go to https://us5.datadoghq.com/apm/traces
2. Filter by `service:hexagon-dotnet-app`
3. Filter by `env:development`

## What You'll See

### Automatic Instrumentation

The application automatically traces:

✅ **HTTP Requests**

- Request method, path, headers
- Response status codes
- Request duration

✅ **Database Queries**

- SQL command text
- Query execution time
- Database connection details

✅ **External API Calls**

- PokeAPI requests
- HTTP client resilience (retries, timeouts)
- Response codes and latency

✅ **System Metrics**

- CPU usage
- Memory allocation
- Garbage collection
- Thread pool statistics

### Example Trace

When you create a todo (`POST /todos`), you'll see:

```
POST /todos (200ms total)
├── ASP.NET Core Handler (200ms)
│   ├── Tag: http.request.method = POST
│   ├── Tag: http.request.path = /todos
│   ├── Tag: http.response.status_code = 201
│   │
│   └── EF Core Query (15ms)
│       ├── Command: INSERT INTO TODOS (TITLE, IS_COMPLETED, ...)
│       ├── Tag: db.execution_time_ms = 15.2
│       │
│       └── SQL Client (14ms)
│           ├── Tag: db.system = sqlite
│           └── Tag: db.statement = INSERT INTO TODOS...
```

## Configuration Files

All configuration is centralized:

| File                                                                                        | Purpose                        |
| ------------------------------------------------------------------------------------------- | ------------------------------ |
| [src/App.ServiceDefaults/Extensions.cs](../src/App.ServiceDefaults/Extensions.cs)           | OpenTelemetry configuration    |
| [src/App.Api/Properties/launchSettings.json](../src/App.Api/Properties/launchSettings.json) | Environment variables for OTLP |
| [.devcontainer/datadog.yaml](../.devcontainer/datadog.yaml)                                 | Datadog agent configuration    |

## Environment Variables

The application uses these environment variables (already configured in `launchSettings.json`):

```bash
# OpenTelemetry Configuration
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_SERVICE_NAME=hexagon-dotnet-app
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=development,service.version=1.0.0

# Datadog Agent Configuration
DD_AGENT_HOST=localhost
DD_TRACE_AGENT_PORT=8126
DD_DOGSTATSD_PORT=8125
DD_ENV=development
DD_SERVICE=hexagon-dotnet-app
DD_VERSION=1.0.0
```

## Instrumentation Details

### OpenTelemetry Packages

| Package                                             | Purpose               |
| --------------------------------------------------- | --------------------- |
| `OpenTelemetry.Instrumentation.AspNetCore`          | HTTP request tracing  |
| `OpenTelemetry.Instrumentation.Http`                | HTTP client tracing   |
| `OpenTelemetry.Instrumentation.EntityFrameworkCore` | EF Core query tracing |
| `OpenTelemetry.Instrumentation.SqlClient`           | SQL query tracing     |
| `OpenTelemetry.Instrumentation.Runtime`             | .NET CLR metrics      |
| `OpenTelemetry.Instrumentation.Process`             | Process-level metrics |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol`      | OTLP exporter         |

All configured in [App.ServiceDefaults/Extensions.cs](../src/App.ServiceDefaults/Extensions.cs)

### Trace Enrichment

Each span is enriched with:

**Service Metadata**:

- Service name: `hexagon-dotnet-app`
- Service version: `1.0.0`
- Service namespace: `hexagon`
- Environment: `development`

**Runtime Information**:

- Process runtime: `.NET`
- Runtime version: `10.0.x`
- Host name: Your machine name

**Operation Details**:

- HTTP method, path, status code
- Database query text and execution time
- Exception details (when errors occur)

## Verify Everything Works

### 1. Check Agent Status

```bash
docker exec dd-agent agent status | grep -A 10 "OTLP"
```

Expected output:

```
OTLP Receiver
    Status: Running
    Spans received: XXX
    Metrics received: XXX
```

### 2. Check Application Logs

```bash
tail -f src/App.Api/logs/app.log
```

Look for startup messages indicating OpenTelemetry initialization.

### 3. Check for Traces

Make a request:

```bash
curl http://localhost:5112/todos
```

Then check agent:

```bash
docker exec dd-agent agent status | grep "Trace Writer"
```

You should see "Trace Writer" showing successful trace submissions.

## Next Steps

- **[Full Documentation](./LOCAL_DATADOG_APM.md)** - Detailed configuration options
- **[Datadog Logging](./DATADOG_LOGGING.md)** - Set up log forwarding
- **[Aspire Integration](./ASPIRE_INTEGRATION.md)** - Local observability dashboard

## Troubleshooting

### No traces appearing?

1. **Verify agent is running:**

   ```bash
   docker ps | grep dd-agent
   ```

2. **Check agent logs:**

   ```bash
   docker logs dd-agent | tail -50
   ```

3. **Verify OTLP endpoint is reachable:**

   ```bash
   curl http://localhost:4318/v1/traces
   ```

   Should return: `404 page not found` (endpoint exists, just doesn't accept GET)

4. **Check application environment variables:**
   ```bash
   dotnet run --project src/App.Api | grep OTEL
   ```

### Traces in Aspire but not Datadog Cloud?

- Verify you set `DD_API_KEY` when starting the agent
- Wait 2-5 minutes for initial data to appear
- Check Datadog agent logs for forwarding errors

### Performance impact?

OpenTelemetry adds minimal overhead:

- ~1-2ms per HTTP request
- Sampling can be configured to reduce overhead in production
- Agent runs separately, no impact on app process

## Production Considerations

For production deployment:

1. **Use Datadog Cloud endpoint directly** instead of local agent
2. **Configure sampling** to reduce data volume
3. **Set appropriate environment** (staging, production)
4. **Enable error tracking** with `RecordException = true`
5. **Use resource attributes** for team/project identification
6. **Set up alerting** based on error rates and latency

See [LOCAL_DATADOG_APM.md](./LOCAL_DATADOG_APM.md#switching-between-local-and-cloud) for cloud configuration.
