# .NET Aspire Compatibility Note - .NET 10

> **⚠️ DEPRECATED**: This document is **obsolete** as of the upgrade to **Aspire 13.1.1**.
>
> **Current Status**: ✅ Aspire 13.1.1 works perfectly with .NET 10 using the new SDK pattern
>
> For current setup and usage, see:
>
> - [ASPIRE_INTEGRATION.md](ASPIRE_INTEGRATION.md) - Full integration guide
> - [ASPIRE_QUICKSTART.md](ASPIRE_QUICKSTART.md) - Quick start guide
> - [README.md](../README.md) - Main project documentation
>
> This file is preserved for historical context only.

---

## Historical Context (Obsolete)

### Original Status (Aspire 9.0)

There **was** a compatibility issue between .NET 10 and .NET Aspire 9.0:

- ✅ **.NET 10** is installed and working
- ❌ **.NET Aspire workload is deprecated** in .NET 10
- ❌ **Aspire 9.0 requires** the workload for DCP (orchestration runtime)
- ⚠️ **Aspire 10+** (which would support .NET 10 properly) is not yet released

**Resolution**: Upgraded to **Aspire 13.1.1** which uses the new SDK pattern (`Aspire.AppHost.Sdk/13.1.1`) and fully supports .NET 10 without workload dependency.

---

## Historical Information (All sections below are obsolete)

### What Worked (Aspire 9.0 Era) ✅

All Aspire **observability and resilience features** worked when running the API directly:

```bash
dotnet run --project src/App.Api
```

You get:

- ✅ **OpenTelemetry** - Automatic tracing, metrics, and logging
- ✅ **Health Checks** - `/health` and `/alive` endpoints
- ✅ **HTTP Resilience** - Automatic retries, timeouts, circuit breakers
- ✅ **Service Discovery** - Ready for multi-service scenarios

### What Didn't Work (Aspire 9.0 Era) ❌

The **AppHost orchestrator** (dashboard and DCP) didn't work:

```bash
dotnet run --project src/App.AppHost  # ❌ Failed with DCP error
```

This was because:

1. The workload was deprecated in .NET 10
2. Aspire 9.0 packages didn't include DCP for .NET 10
3. Aspire 13+ wasn't yet released

### Solutions That Were Available (Now Obsolete)

### Former Option 1: Run API Directly

```bash
dotnet run --project src/App.Api
```

Access:

- API: http://localhost:5112
- Swagger: http://localhost:5112
- Health: http://localhost:5112/health

**Telemetry output**: Configure an external OTLP endpoint if you want to visualize telemetry:

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT=http://your-collector:4317
dotnet run --project src/App.Api
```

### Former Option 2: Downgrade to .NET 9

If you needed the Aspire Dashboard with Aspire 9.0, the workaround was:

1. Install .NET 9 SDK
2. Change `<TargetFramework>net10.0</TargetFramework>` to `net9.0` in all .csproj files
3. Install workload: `dotnet workload install aspire`
4. Run: `dotnet run --project src/App.AppHost`

### Former Option 3: Wait for Aspire 13+

This option materialized when Aspire 13.1.1 was released with .NET 10 support.

### Telemetry Alternatives (Historical)

Even without the Aspire dashboard, telemetry was collected. Alternative options were:

1. **Export to Jaeger**:

   ```bash
   docker run -d --name jaeger -p 16686:16686 -p 4317:4317 \
     jaegertracing/all-in-one:latest

   export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
   dotnet run --project src/App.Api
   ```

   View at: http://localhost:16686

2. **Export to Azure Application Insights**:
   Add connection string to appsettings.json:

   ```json
   {
     "APPLICATIONINSIGHTS_CONNECTION_STRING": "your-connection-string"
   }
   ```

3. **Export to Console** (development):
   ```bash
   export OTEL_EXPORTER_OTLP_ENDPOINT=console
   dotnet run --project src/App.Api
   ```

### Testing ServiceDefaults (Historical Example)

This example showed how to verify ServiceDefaults was working without the dashboard:

```bash
# Start the API
dotnet run --project src/App.Api

# In another terminal:
# Check health
curl http://localhost:5112/health

# Make some requests to generate telemetry
curl http://localhost:5112/todos
curl http://localhost:5112/api/v1/pokemon?limit=5

# Check logs for OpenTelemetry activity IDs
```

You would see correlation IDs in logs like:

```
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5112/todos - - -
      ActivityId: 00-a1b2c3d4e5f6...
```

## Historical Summary

**The Problem (Aspire 9.0 + .NET 10)**: The Aspire workload was deprecated in .NET 10, but Aspire 9.0 required it for the orchestration runtime (DCP and Dashboard).

**The Solution**: Aspire 13.1.1 introduced a new SDK pattern (`Aspire.AppHost.Sdk/13.1.1`) that works with .NET 10 without requiring any workload installation.

**Current Status**: Everything works perfectly. See [ASPIRE_INTEGRATION.md](ASPIRE_INTEGRATION.md) for current setup.
