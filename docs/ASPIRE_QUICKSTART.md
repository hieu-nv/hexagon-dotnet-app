# Quick Start Guide - Running with .NET Aspire

This guide will help you get started with the Aspire-enabled version of this application.

## Prerequisites

- .NET 10 SDK
- (Optional) Docker Desktop if you plan to add containerized resources

> **Note**: This application uses Aspire 13.1.1 with the new **SDK pattern** (`Aspire.AppHost.Sdk/13.1.1`). No workload installation is required.

## Running the Application

### Option 1: Run with Aspire (Recommended)

Run the AppHost orchestrator to start the application with full observability:

```bash
dotnet run --project src/App.AppHost
```

This will:

1. Start the Aspire Dashboard at `http://localhost:17123`
2. Launch the API service at `http://localhost:5112`
3. Enable distributed tracing, metrics, and structured logging

> **Development Note**: The dashboard uses HTTP (not HTTPS) to avoid SSL certificate errors. Configured via `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`.

### Option 2: Run API Directly (No Aspire Required)

You can still run the API independently without Aspire:

```bash
dotnet run --project src/App.Api
```

The API will be available at `http://localhost:5112`

## Accessing the Aspire Dashboard

Once the AppHost is running, navigate to: `http://localhost:17123`

### Dashboard Features

1. **Resources Tab**
   - View all running services and their health status
   - See assigned ports and endpoints
   - Monitor resource status

2. **Console Logs**
   - Real-time console output from all services
   - Easily switch between services

3. **Structured Logs**
   - Searchable, filterable log entries
   - Correlation IDs for request tracking
   - Log level filtering

4. **Traces**
   - Distributed tracing across HTTP requests
   - Database query tracing
   - External API call tracking (e.g., PokeAPI)
   - End-to-end request visualization

5. **Metrics**
   - Request rates and response times
   - Error rates
   - .NET runtime metrics (GC, thread pool, exceptions)
   - Custom application metrics

## Testing the Application

### Via Swagger UI

When running the API directly, Swagger is available at: `http://localhost:5112`

Note: When running via Aspire, check the dashboard for the assigned API port.

### Via Aspire Dashboard

The Aspire dashboard shows the API endpoints with clickable links to test directly.

### Sample Requests

```bash
# Get all todos
curl http://localhost:5112/todos

# Create a new todo
curl -X POST http://localhost:5112/todos \
  -H "Content-Type: application/json" \
  -d '{"title": "Test Aspire", "isCompleted": false}'

# Get Pokemon list
curl http://localhost:5112/api/v1/pokemon?limit=10
```

## Observing Telemetry

### Tracing PokeAPI Calls

1. Make a request to the Pokemon endpoint:

   ```bash
   curl http://localhost:5112/api/v1/pokemon/1
   ```

2. In the Aspire Dashboard, go to **Traces**

3. Find the trace that includes:
   - Incoming HTTP request to your API
   - Outgoing HTTP request to PokeAPI
   - Database queries (if any)

4. Click to drill down and see timing for each span

### Viewing Logs with Correlation

1. Make several API requests
2. In the Aspire Dashboard, go to **Structured Logs**
3. Click on a log entry to see its trace ID
4. Click the trace ID to jump to the full distributed trace

### Monitoring Metrics

1. In the Aspire Dashboard, go to **Metrics**
2. Select a service (e.g., "api")
3. View metrics like:
   - `http.server.request.duration` - Request response times
   - `http.client.request.duration` - External API call times
   - `process.runtime.dotnet.gc.collections.count` - Garbage collection activity

## Health Checks

The application exposes two health check endpoints:

### `/health` - Readiness

Checks if the application is ready to accept traffic:

- Database connectivity
- All critical dependencies

```bash
curl http://localhost:5112/health
```

### `/alive` - Liveness

Checks if the application process is running and responsive:

```bash
curl http://localhost:5112/alive
```

When running via Aspire, these health checks are monitored automatically and displayed in the dashboard.

## Development Workflow

### Hot Reload

When running with Aspire, code changes trigger hot reload:

1. Start the application: `dotnet run --project src/App.AppHost`
2. Make code changes to App.Api, App.Core, etc.
3. Save the file
4. The service automatically reloads with your changes

### Debugging

#### Visual Studio / VS Code

1. Open the solution in your IDE
2. Set `App.AppHost` as the startup project
3. Press F5 to start debugging
4. Breakpoints work across all projects
5. The Aspire Dashboard opens automatically

#### Command Line

```bash
# In one terminal - watch for changes
dotnet watch run --project src/App.AppHost

# Make code changes and they'll be applied automatically
```

## Stopping the Application

Press `Ctrl+C` in the terminal where AppHost is running. This will gracefully shut down all services.

## Troubleshooting

### Port Already in Use

If you see port conflicts:

```bash
# macOS/Linux
lsof -ti:17123 | xargs kill -9
lsof -ti:22222 | xargs kill -9

# Windows
netstat -ano | findstr :17123
taskkill /PID <PID> /F
```

### Dashboard Not Loading

1. Ensure .NET 10 SDK is installed: `dotnet --version`
2. Check if AppHost is running: `ps aux | grep AppHost`
3. Verify ports are accessible: `lsof -nP -iTCP:17123 -sTCP:LISTEN`
4. Check logs in the terminal for any errors

### Telemetry Not Appearing

If telemetry isn't showing in the dashboard:

1. Verify `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable is set by AppHost
2. Check logs in the Console Logs tab for errors
3. Ensure ServiceDefaults is referenced by App.Api

## Next Steps

- Add more services to the AppHost
- Configure Redis for caching
- Add RabbitMQ or Azure Service Bus for messaging
- Deploy to Azure Container Apps using `azd`
- Set up CI/CD pipelines

For more details, see [docs/ASPIRE_INTEGRATION.md](../docs/ASPIRE_INTEGRATION.md)
