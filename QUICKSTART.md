# Quick Start Guide

## ✅ Aspire 13.1.1 Successfully Integrated!

The application now runs with **full Aspire 13.1.1** support on **.NET 10**!

### Run the Application

#### Option 1: Run with Aspire Dashboard (Recommended)

```bash
dotnet run --project src/App.AppHost
```

**Access Points:**

- 🎯 **Aspire Dashboard**: http://localhost:17123 _(HTTP only - no SSL errors!)_
- 🌐 **API**: http://localhost:5112
- 🏥 **Health Check**: http://localhost:5112/health
- 💓 **Liveness**: http://localhost:5112/alive

> **Note**: The dashboard runs on **HTTP** to eliminate SSL certificate errors in development. Configured via `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`.

#### Option 2: Run API Only (Without Dashboard)

```bash
dotnet run --project src/App.Api
```

**Access Points:**

- 🌐 **API**: http://localhost:5112
- 🏥 **Health Check**: http://localhost:5112/health
- 💓 **Liveness**: http://localhost:5112/alive

### Test the API

#### Health Checks

```bash
# Readiness check
curl http://localhost:5112/health

# Liveness check
curl http://localhost:5112/alive
```

#### Todos

```bash
# Get all todos
curl http://localhost:5112/api/v1/todos

# Create a todo
curl -X POST http://localhost:5112/api/v1/todos \
  -H "Content-Type: application/json" \
  -d '{"title": "Test Aspire", "isCompleted": false}'
```

#### Pokemon API (External Gateway Test)

```bash
# Get first 3 Pokemon
curl "http://localhost:5112/api/v1/pokemon?limit=3"
```

## Features Enabled

✅ **Aspire Dashboard** - Visual orchestration and monitoring  
✅ **OpenTelemetry** - Distributed tracing and metrics  
✅ **Health Checks** - `/health` (readiness) and `/alive` (liveness)  
✅ **HTTP Resilience** - Automatic retry, circuit breaker, timeout  
✅ **Service Discovery** - Ready for multi-service scenarios  
✅ **.NET 10** - Latest .NET runtime

## What Changed

### AppHost Project ([src/App.AppHost/App.AppHost.csproj](src/App.AppHost/App.AppHost.csproj))

**Before:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Aspire.Hosting" Version="9.0.0" />
    <PackageReference Include="Aspire.Hosting.AppHost" Version="9.0.0" />
  </ItemGroup>
</Project>
```

**After (Aspire 13.1.1 Pattern):**

```xml
<Project Sdk="Aspire.AppHost.Sdk/13.1.1">
  <!-- Aspire.Hosting packages now included automatically by SDK -->
</Project>
```

### Key Improvements

1. **Simplified SDK** - Uses `Aspire.AppHost.Sdk/13.1.1` directly
2. **No Workload Required** - Pure NuGet package approach
3. **Full .NET 10 Support** - No compatibility issues
4. **Cleaner Project Files** - Removed boilerplate package references

## Build & Test

```bash
# Build everything
dotnet build src/App.slnx

# Run all tests (48 tests)
dotnet test src/App.slnx

# Run with code coverage
dotnet test src/App.slnx --collect:"XPlat Code Coverage"
```

All tests pass! ✅

## Troubleshooting

### Port Already in Use

If you see `Failed to bind to address https://127.0.0.1:22222: address already in use`:

```bash
# Kill process on port 22222
lsof -ti:22222 | xargs kill -9

# Then run again
dotnet run --project src/App.AppHost
```

### SSL Certificate Warnings

**✅ SSL has been disabled in development!** The dashboard now runs on plain HTTP to eliminate all certificate errors.

Configuration changes made:

- Dashboard URL: `http://localhost:17123` (was https://localhost:22222)
- Environment variable: `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`
- All Aspire endpoints use HTTP in development

**No more SSL errors!** 🎉

> **Production Note**: In production, you should use HTTPS with proper certificates. The HTTP configuration is **only for development** to avoid certificate issues.

## Documentation

- [ASPIRE_INTEGRATION.md](docs/ASPIRE_INTEGRATION.md) - Detailed architecture guide
- [ASPIRE_QUICKSTART.md](docs/ASPIRE_QUICKSTART.md) - Setup and configuration
- [README.md](README.md) - Project overview
