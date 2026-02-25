# .NET Aspire Integration Guide

This document explains how .NET Aspire is integrated into this hexagonal architecture application.

## Overview

.NET Aspire is an opinionated, cloud-ready stack for building observable, production-ready distributed applications. This project uses Aspire to provide:

- **Orchestration**: Manage and coordinate multiple services
- **Observability**: Built-in OpenTelemetry for tracing, metrics, and logs
- **Service Discovery**: Automatic service-to-service communication
- **Resilience**: HTTP retry and circuit breaker patterns
- **Dashboard**: Real-time monitoring and debugging

## Architecture Components

### App.AppHost (Orchestration)

The `App.AppHost` project is the entry point for running the distributed application. It:

- Defines all services in the application
- Configures service dependencies and references
- Launches the Aspire Dashboard
- Manages service lifecycle

**Location**: `src/App.AppHost/Program.cs`

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add the API project - SQLite connection is managed within App.Data layer
// Path is relative to the AppHost project directory
builder.AddProject("api", "../App.Api/App.Api.csproj").WithExternalHttpEndpoints();

builder.Build().Run();
```

### App.ServiceDefaults (Observability)

The `App.ServiceDefaults` project contains shared configuration for all services:

- **OpenTelemetry**: Automatic instrumentation for ASP.NET Core, HTTP clients, and runtime
- **Health Checks**: Liveness and readiness probes
- **Service Discovery**: Automatic service resolution
- **HTTP Resilience**: Retry, timeout, and circuit breaker patterns

**Location**: `src/App.ServiceDefaults/Extensions.cs`

Key extension methods:

- `AddServiceDefaults()`: Adds all Aspire defaults to a service
- `ConfigureOpenTelemetry()`: Sets up telemetry export
- `MapDefaultEndpoints()`: Adds `/health` and `/alive` endpoints

## Running with Aspire

### Start the Application

```bash
# From the repository root
dotnet run --project src/App.AppHost
```

This launches:

1. The Aspire Dashboard at `http://localhost:17123`
2. The API service at `http://localhost:5112`

> **Development Note**: The dashboard uses HTTP (not HTTPS) to eliminate SSL certificate errors. This is configured via `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true` in the launch settings.

### Access the Dashboard

Navigate to `http://localhost:17123` to see:

- **Resources**: All running services and their status
- **Console Logs**: Real-time log output from all services
- **Structured Logs**: Filterable, searchable log entries
- **Traces**: Distributed tracing across service boundaries
- **Metrics**: Performance counters and custom metrics

### View Telemetry

The dashboard automatically collects:

1. **Traces**: HTTP requests, database queries, external API calls
2. **Metrics**: Request rates, error rates, response times, runtime metrics
3. **Logs**: Structured logs with correlation IDs

## Integration with Hexagonal Architecture

Aspire integrates seamlessly with the hexagonal architecture:

```
┌─────────────────────────────────────────┐
│         App.AppHost                     │  ← Orchestration Layer
│   (Aspire Orchestration)                │
└─────────────────────────────────────────┘
              │
              ├─── References ───┐
              │                  │
┌─────────────▼─────────────┐   │
│      App.ServiceDefaults   │   │          ← Observability Layer
│  (OpenTelemetry, Health)   │   │
└────────────────────────────┘   │
                                 │
        ┌────────────────────────▼──────┐
        │         App.Api                │  ← Primary Adapters
        │    (HTTP Endpoints)            │
        └────────────┬───────────────────┘
                     │
        ┌────────────┴───────────┐
        │                        │
┌───────▼──────────┐   ┌────────▼───────────┐
│   App.Core       │   │    App.Gateway     │  ← Ports & Domain
│   (Domain)       │   │  (External APIs)   │
└───────┬──────────┘   └────────────────────┘
        │
┌───────▼──────────┐
│   App.Data       │                          ← Secondary Adapters
│  (Repository)    │
└──────────────────┘
```

### Service Defaults in App.Api

The API project references `App.ServiceDefaults` and calls:

```csharp
// In Program.cs
builder.AddServiceDefaults();  // Adds observability and resilience
// ... register domain services ...
app.MapDefaultEndpoints();     // Adds /health and /alive
```

This maintains separation of concerns:

- **Domain logic** (App.Core) remains pure and unaware of Aspire
- **Infrastructure** (App.Data, App.Gateway) focuses on adapters
- **Application** (App.Api) adds observability at the edge

## OpenTelemetry Configuration

The ServiceDefaults project configures OpenTelemetry to:

### Tracing

- Instrument ASP.NET Core requests
- Instrument HttpClient calls to external APIs (PokeAPI)
- Add activity IDs for correlation

### Metrics

- ASP.NET Core request metrics (rate, duration, status)
- HttpClient metrics (external API performance)
- .NET Runtime metrics (GC, thread pool, exceptions)

### Logging

- Structured logging with log levels
- Correlation with traces via activity IDs
- Export to Aspire dashboard

### Export

Telemetry is exported via OTLP (OpenTelemetry Protocol) to the Aspire dashboard when the environment variable `OTEL_EXPORTER_OTLP_ENDPOINT` is set (automatically by AppHost).

## Health Checks

Two health check endpoints are provided:

### `/health` - Readiness

Checks if the application is ready to accept traffic:

- Database connectivity (DbContext health check)
- All registered health checks must pass

### `/alive` - Liveness

Checks if the application is running and should not be restarted:

- Only checks tagged with "live" (self-check)
- Indicates the process is responsive

## Service Discovery

While this application currently has a single API service, Aspire's service discovery is configured and ready for multi-service scenarios:

```csharp
// ServiceDefaults automatically adds:
builder.Services.AddServiceDiscovery();

// HttpClient factory is configured for service discovery:
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddServiceDiscovery();  // Resolve services by name
    http.AddStandardResilienceHandler();  // Retry, timeout, circuit breaker
});
```

## HTTP Resilience

All HTTP clients automatically get resilience patterns:

- **Retry**: Transient failures are retried with exponential backoff
- **Timeout**: Requests timeout if they take too long
- **Circuit Breaker**: Failing services are temporarily bypassed

This applies to:

- PokeAPI HTTP client (external API calls)
- Any future service-to-service calls

## Environment Variables

When running through AppHost, these variables are configured:

- `OTEL_EXPORTER_OTLP_ENDPOINT`: **Default**: `http://localhost:4318` (from `.env` file) - sends to local Datadog agent
- `ASPNETCORE_ENVIRONMENT`: Development
- Connection strings and service URLs (if using Aspire resources)

> **Note**: The application is preconfigured in `.env` to use a local Datadog agent at `http://localhost:4318`. Aspire also exposes its own dashboard at `http://localhost:17123` for local trace viewing.

## Running Without Aspire

The application can still run independently:

```bash
dotnet run --project src/App.Api
```

When `OTEL_EXPORTER_OTLP_ENDPOINT` is not set:

- OpenTelemetry is still instrumented but not exported
- Health checks still work
- Service discovery is not active (direct URLs used)

## Local Development

### Prerequisites

- .NET 10 SDK
- Docker Desktop (for containerized resources, if needed in future)

### First Run

```bash
# Restore dependencies
dotnet restore src/App.slnx

# Run with Aspire
dotnet run --project src/App.AppHost
```

### Debugging

In Visual Studio or Visual Studio Code:

1. Set `App.AppHost` as the startup project
2. F5 to debug
3. All services start with debugging enabled
4. Breakpoints work across all projects

## Testing

Tests run independently and don't require Aspire:

```bash
dotnet test src/App.slnx
```

Integration tests use the Test Host and don't depend on the AppHost.

## Production Deployment

For production deployments:

1. **Build all projects**:

   ```bash
   dotnet publish src/App.Api -c Release -o publish/api
   ```

2. **Configure OpenTelemetry export** to your observability platform:
   - Set `OTEL_EXPORTER_OTLP_ENDPOINT` to your collector
   - Or configure other exporters (Jaeger, Zipkin, Application Insights)

3. **Deploy the API service** (not the AppHost):
   - AppHost is for local development and orchestration
   - In production, use container orchestrators (Kubernetes) or PaaS

4. **Configure health checks** in your load balancer:
   - Liveness: `GET /alive`
   - Readiness: `GET /health`

## Benefits for This Application

Even with a single service, Aspire provides:

1. **Visibility**: See all HTTP requests to PokeAPI, database queries, and logs in one place
2. **Performance**: Automatic metrics for response times and error rates
3. **Debugging**: Distributed traces show the full request lifecycle
4. **Resilience**: PokeAPI calls are automatically retried on failure
5. **Readiness**: Easy to add more services with automatic service discovery

## Future Enhancements

As the application grows, Aspire enables:

- Adding a separate Auth service with automatic discovery
- Running multiple API instances with load balancing
- Adding Redis for caching (Aspire has built-in Redis support)
- Containerizing with minimal configuration changes
- Deploying to Azure Container Apps with `azd`

## Troubleshooting

### Development HTTP Configuration

This application is configured to use **HTTP (not HTTPS)** for the Aspire Dashboard in development:

- Dashboard URL: `http://localhost:17123`
- API URL: `http://localhost:5112`
- Environment variable: `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`

This eliminates SSL certificate errors during development. **Production deployments should use HTTPS with proper certificates.**

### Port Already in Use

If `dotnet run --project src/App.AppHost` fails with:

```
Failed to bind to address http://127.0.0.1:17123: address already in use
```

**Solution**: Kill the existing process and try again:

```bash
lsof -ti:17123 | xargs kill -9
lsof -ti:22222 | xargs kill -9
dotnet run --project src/App.AppHost
```

### Dashboard Not Loading

If the dashboard URL (http://localhost:17123) doesn't load:

1. **Check the process is running**: `lsof -nP -iTCP:17123 -sTCP:LISTEN`
2. **Check the logs**: Look for "Aspire version:" and "Now listening on" messages in the terminal
3. **Verify ports**: Ensure ports 17123 and 22222 are not blocked by firewall
4. **Kill and restart**: `lsof -ti:17123 | xargs kill -9 && dotnet run --project src/App.AppHost`

## Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire)
- [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet)
- [Aspire Dashboard](https://learn.microsoft.com/dotnet/aspire/fundamentals/dashboard)
- [Aspire 13 Upgrade Guide](https://learn.microsoft.com/dotnet/aspire/get-started/upgrade-to-aspire-13)
