# Datadog APM Quickstart

Get application performance monitoring (APM) running in 5 minutes.

## Default Configuration

**This application uses a LOCAL Datadog agent by default** (configured in `.env` file):

```dotenv
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
```

**Architecture**: Application → Local Agent (`localhost:4318`) → Datadog Cloud

This two-hop setup provides:

- ✅ Local development without requiring API keys
- ✅ Buffering and retry logic via the agent
- ✅ Support for both OTLP traces AND metrics
- ✅ Offline development capability

> **Note**: For direct-to-cloud setup (bypassing local agent), see "Alternative: Direct to Cloud" section below.

## Prerequisites

- Docker or Podman installed
- .NET 10 SDK installed
- (Optional) Datadog API key for cloud forwarding

## Quick Start (Local Agent Mode)

### 1. Start the Datadog Agent

```bash
./scripts/datadog-agent.sh
```

This starts a local Datadog agent with OpenTelemetry support on ports:

- `4318` - OTLP HTTP (used by this app)
- `4317` - OTLP gRPC
- `8126` - Datadog APM
- `8125` - StatsD metrics

### 2. Run the Application

```bash
dotnet run --project src/App.Api
```

The application is **preconfigured in `.env`** to send traces to the local agent at `http://localhost:4318`. The agent then forwards traces to Datadog cloud if you provide a `DD_API_KEY`.

### 3. Generate Test Traffic

```bash
# Health check
curl http://localhost:5112/health

# Create a todo (tests database operations)
curl -X POST http://localhost:5112/api/v1/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Datadog APM","isCompleted":false}'

# Get todos
curl http://localhost:5112/api/v1/todos

# Get a Pokemon (tests external HTTP calls)
curl http://localhost:5112/api/v1/pokemon/25
curl http://localhost:5112/api/v1/pokemon/1

# Search Pokemon (tests pagination)
curl http://localhost:5112/api/v1/pokemon?limit=20&offset=0
```

### 4. View Traces

#### Option A: Local Agent Dashboard

View traces processed by the local agent:

```bash
# Check agent status
docker exec dd-agent agent status

# View trace stats
docker logs dd-agent | grep trace
```

#### Option B: Aspire Dashboard (Local)

```bash
dotnet run --project src/App.AppHost
```

Open http://localhost:17123 and navigate to the **Traces** section.

#### Option C: Datadog Cloud Dashboard

If you provided `DD_API_KEY` to the agent, traces will be forwarded to Datadog cloud:

1. Go to https://us5.datadoghq.com/apm/traces
2. Filter by `service:hexagon-dotnet-app`
3. Filter by `env:development`

## Alternative: Direct to Cloud (Optional)

If you want to bypass the local agent and send traces directly to Datadog cloud:

**Architecture**: Application → Datadog Cloud (one-hop)

**Trade-offs**:

- ✅ Simpler deployment (no agent needed)
- ✅ Immediate data in Datadog cloud
- ❌ Requires API key in environment variables
- ❌ Only supports OTLP traces (not metrics)
- ❌ No offline development capability
- ❌ No local buffering/retry logic

**Configuration**:

1. Update `.env` file or export variables:

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT="https://trace.agent.us5.datadoghq.com:443"
export OTEL_EXPORTER_OTLP_PROTOCOL="http/protobuf"
export OTEL_EXPORTER_OTLP_HEADERS="dd-api-key=YOUR_ACTUAL_API_KEY"
export DD_API_KEY="YOUR_ACTUAL_API_KEY"
export DD_SITE="us5.datadoghq.com"
```

2. Run the application:

```bash
dotnet run --project src/App.AppHost
```

> **Note**: The code in `src/App.ServiceDefaults/Extensions.cs` (lines 185-209) automatically detects the endpoint and configures OTLP accordingly. Localhost endpoints enable both traces + metrics, while cloud endpoints only enable traces.

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

When you create a todo (`POST /api/v1/todos`), you'll see:

```
POST /api/v1/todos (200ms total)
├── ASP.NET Core Handler (200ms)
│   ├── Tag: http.request.method = POST
│   ├── Tag: http.request.path = /api/v1/todos
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

| File                                                                              | Purpose                     |
| --------------------------------------------------------------------------------- | --------------------------- |
| [src/App.ServiceDefaults/Extensions.cs](../src/App.ServiceDefaults/Extensions.cs) | OpenTelemetry configuration |
| [scripts/datadog-agent/datadog.yaml](../scripts/datadog-agent/datadog.yaml)       | Datadog agent configuration |

## Environment Variables

The application automatically configures OpenTelemetry when running under Aspire orchestration. For manual configuration with a local Datadog agent, you can set:

```bash
# OpenTelemetry Configuration (optional - auto-configured by Aspire)
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf
OTEL_SERVICE_NAME=hexagon-dotnet-app
OTEL_RESOURCE_ATTRIBUTES=deployment.environment=development,service.version=1.0.0

# Datadog Labels (optional)
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
curl http://localhost:5112/api/v1/todos
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
