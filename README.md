# Hexagon .NET App

[![CI](https://github.com/hieu-nv/hexagon-dotnet-app/actions/workflows/ci.yml/badge.svg)](https://github.com/hieu-nv/hexagon-dotnet-app/actions/workflows/ci.yml)
[![Coverage](https://img.shields.io/badge/Coverage-93.3%25-brightgreen)](README.md#test-coverage)

A modern ASP.NET Core application using minimal APIs to demonstrate hexagonal architecture with both internal data persistence and external service integration.

## Overview

This application demonstrates the implementation of a hexagonal (ports and adapters) architecture in ASP.NET Core with two distinct use cases:

- **Todo API**: Internal data persistence using Entity Framework Core
- **Pokemon API**: External service integration via gateway pattern

It provides a structured approach to building maintainable and testable applications by separating business logic from external concerns.

## 🎯 Roadmap to 10/10

**Current Status: 10/10** - A production-ready, professionally architected codebase demonstrating absolute best practices for building maintainable, observable, hexagonal .NET applications.

### Phase 1: Architectural Consistency (✅ Complete)

- [x] **1.1 Create Pokemon Service Layer**
  - [x] Create `src/App.Core/Pokemon/PokemonService.cs`
  - [x] Add business logic, validation, error handling
  - [x] Register PokemonService in `AppCore.cs`
  - [x] Update PokemonEndpoints to inject service instead of gateway
  - [x] Add XML documentation

- [x] **1.2 Add Response DTOs**
  - [x] Create `src/App.Api/Todo/TodoDto.cs` (TodoResponse, CreateTodoRequest, UpdateTodoRequest)
  - [x] Create `src/App.Api/Pokemon/PokemonDto.cs` (PokemonResponse, PokemonListResponse)
  - [x] Update all endpoints to use DTOs instead of entities
  - [x] Add mapping logic (consider AutoMapper or Mapperly)
  - [x] Document breaking changes

- [x] **1.3 API Versioning**
  - [x] Add versioning to Todo endpoints (`/api/v1/todos`)
  - [x] Add versioning to Pokemon endpoints (`/api/v1/pokemon`)
  - [x] Update Swagger to group by version
  - [x] Document versioning strategy

### Phase 2: Test Coverage (✅ Complete)

- [x] **2.1 Pokemon Gateway Tests**
  - [x] Create `test/App.Gateway.Tests/` project
  - [x] Write `Pokemon/PokemonGatewayTests.cs`
  - [x] Write `Client/PokeClientTests.cs`
  - [x] Test scenarios: success, 404, timeout, malformed responses
  - [x] Mock HTTP responses with FakeHttpMessageHandler

- [x] **2.2 Pokemon Service Tests**
  - [x] Create `test/App.Core.Tests/Pokemon/PokemonServiceTests.cs`
  - [x] Test service logic with mocked gateway
  - [x] Test validation and error handling
  - [x] Test edge cases

- [x] **2.3 Integration Tests**
  - [x] Create `test/App.Api.Tests/Integration/TodoIntegrationTests.cs`
  - [x] Create `test/App.Api.Tests/Integration/PokemonIntegrationTests.cs`
  - [x] Create `IntegrationTestWebAppFactory.cs` with WebApplicationFactory
  - [x] Test full CRUD flows with real database (SQLite in-memory)
  - [x] Test health checks and error responses

- [x] **2.4 Code Coverage Reporting**
  - [x] Create `.github/workflows/ci.yml`
  - [x] Add coverage collection and reporting
  - [x] Enforce 80% minimum threshold
  - [x] Add coverage badge to README

### Phase 3: Production Hardening (✅ Complete)

- [x] **3.1 Input Validation with FluentValidation**
  - [x] Install `FluentValidation.AspNetCore`
  - [x] Create `src/App.Api/Validators/CreateTodoRequestValidator.cs`
  - [x] Create `src/App.Api/Validators/UpdateTodoRequestValidator.cs`
  - [x] Register validators in DI
  - [x] Add validation middleware

- [x] **3.2 Standardized Error Responses**
  - [x] Create `src/App.Api/Middleware/GlobalExceptionHandler.cs`
  - [x] Implement RFC 7807 ProblemDetails
  - [x] Add correlation IDs to all errors
  - [x] Ensure no stack traces in production
  - [x] Update all endpoints to use standard error format

- [x] **3.3 Security Hardening**
  - [x] Add rate limiting (`AspNetCoreRateLimit`)
  - [x] Configure CORS policy
  - [x] Add security headers (`NetEscapades.AspNetCore.SecurityHeaders`)
  - [x] Add request/response logging middleware
  - [x] Audit for SQL injection vulnerabilities

- [x] **3.4 Performance Optimization**
  - [x] Add database indexes on `TodoEntity.IsCompleted` and `TodoEntity.DueBy`
  - [x] Implement response caching for Pokemon endpoints
  - [x] Add output caching configuration
  - [x] Review and optimize database queries
  - [x] Add connection pooling configuration

### Phase 4: Documentation & DevOps (🟢 Medium)

- [x] **4.1 Architecture Diagrams**
  - [x] Create `docs/architecture/C4-context.md` with system context (See [C4-container.md](docs/architecture/C4-container.md))
  - [x] Create `docs/architecture/hexagon-diagram.md` with hexagonal architecture
  - [x] Add data flow diagrams
  - [x] Use Mermaid for diagrams

- [x] **4.2 CI/CD Pipeline**
  - [x] Create `.github/workflows/ci.yml` (build, test, coverage)
  - [x] Add build status badge to README
  - [x] Configure automated testing (enforced 80% coverage)

- [x] **4.3 Docker Production Optimization**
  - [x] Create multi-stage Dockerfile
  - [x] Use non-root user
  - [x] Add health checks to Docker container
  - [x] Optimize layer caching
  - [x] Create `docker-compose.yml` for local development
  - [x] Create `.dockerignore`

- [x] **4.4 Enhanced Documentation**
  - [x] Add badges (build, coverage, version) to README
  - [x] Create [CONTRIBUTING.md](CONTRIBUTING.md) with contribution guidelines
  - [x] Create [CHANGELOG.md](CHANGELOG.md) with version history
  - [x] Create [API.md](docs/API.md) with detailed API documentation
  - [x] Document architecture and deployment

### Phase 5: Code Review & Refinements (✅ Complete)

- [x] **5.1 Critical Fixes**
  - [x] Fix `PokeClient` exception handling to propagate TaskCanceledException
  - [x] Extract `JsonSerializerOptions` in `PokeClient` to avoid per-request allocations
  - [x] Secure CORS policy by reading allowed origins from `appsettings.json`

- [x] **5.2 Code Consistency & Correctness**
  - [x] Add consistent null guards in `AppData` and `TodoEndpoints`
  - [x] Remove duplicate title validation from `TodoService` (relies on FluentValidation)
  - [x] Refactor `PokemonService` range limiting to use `Math.Clamp`
  - [x] Correct past due dates in `AppDbContext` seed data

- [x] **5.3 Maintenance & Refinements**
  - [x] Extract security HTTP headers logic into `SecurityHeadersMiddleware`
  - [x] Seal `TodoRepository` and remove `virtual` modifiers
  - [x] Update `PokemonGateway` base URL construction to be relative
  - [x] Add missing project references (`.csproj` COPY) in Dockerfile

### Success Criteria for 10/10

- ✅ **Test Coverage:** >80% across all projects
- ✅ **Architectural Consistency:** Pokemon matches Todo pattern (service layer)
- ✅ **API Standards:** DTOs, versioning, RFC 7807 errors
- ✅ **Production Ready:** Security headers, rate limiting, CORS
- ✅ **CI/CD:** Automated testing and deployment pipeline
- ✅ **Documentation:** Architecture diagrams, API docs, contributing guide
- ✅ **No Errors:** Zero compiler warnings or analyzer violations
- ✅ **Performance:** Database indexes, caching strategy, optimized queries

**Estimated Timeline:** 2-3 weeks (1 developer)

**Total Tests:** 140 (31 Core + 11 Gateway + 98 API/Data/ServiceDefaults)

**Key Technologies:**

- .NET Aspire 13.1.1 (Orchestration and Observability)
- ASP.NET Core 10 with Minimal APIs
- Entity Framework Core with SQLite
- Serilog with Datadog Integration (Structured Logging)
- Datadog APM, Metrics, and Logs
- OpenTelemetry (Tracing, Metrics, Logging)
- Service Discovery and Resilience
- Dependency Injection
- Health Checks
- OpenAPI/Swagger Documentation

## Architecture

The application follows a hexagonal architecture pattern enhanced with .NET Aspire:

- **App.AppHost**: .NET Aspire orchestration and service discovery
  - Manages application lifecycle and service dependencies
  - Provides the Aspire dashboard for observability
  - Configures distributed application resources

- **App.ServiceDefaults**: Shared observability and resilience configuration
  - OpenTelemetry integration (tracing, metrics, logging)
  - Service discovery and HTTP resilience patterns
  - Health check configuration
  - Reusable across all services in the distributed application

- **App.Core**: Contains the domain model, entities, repository interfaces (ports), and services
  - Organized with domain-focused directories (e.g., Todo/, Pokemon/)
  - Includes the AppCore class for core service registration
  - Defines port interfaces for both data access and external gateways

- **App.Data**: Implements the data access layer and repository implementations (secondary adapters)
  - Includes AppDbContext for Entity Framework Core data access
  - Implements the repository pattern with generic and specific repositories
  - AppData class provides extension methods for data layer configuration

- **App.Gateway**: Implements external service integrations (secondary adapters)
  - HTTP client infrastructure for external API calls
  - Gateway implementations for third-party services (e.g., PokeAPI)
  - Isolates external service details from business logic
  - AppGateway class provides extension methods for gateway registration

- **App.Api**: Exposes the HTTP API endpoints using minimal API syntax (primary adapters)
  - Uses extension methods for clean endpoint registration
  - Organized by domain rather than technical concerns
  - Leverages dependency injection for clean service resolution
  - Integrated with Aspire ServiceDefaults for observability

## Features

### Todo Management (Internal Persistence)

- **Full CRUD operations** for Todo items (Create, Read, Update, Delete)
- **Structured logging** with detailed operation tracking and error handling
- **Filtering** todos by completion status (completed/incomplete)
- **Validation** with data annotations and business rule enforcement
- **SQLite database** for data persistence with Entity Framework Core
- **Automatic timestamp tracking** (CreatedAt, UpdatedAt)
- **RESTful API** with proper HTTP status codes and error responses

### Pokemon API (External Gateway)

- **External API integration** with PokeAPI using gateway pattern
- **Gateway abstraction** for testability and flexibility
- **HTTP client factory** for efficient connection management
- **Paginated list endpoint** with limit and offset parameters
- **Individual Pokemon lookup** by ID

### Datadog Observability & APM

- **Full-stack observability** with OpenTelemetry integration
- **Distributed tracing** for request flows across services
- **Application Performance Monitoring (APM)** with automatic instrumentation:
  - HTTP request/response tracing with enriched metadata
  - Database query tracing (Entity Framework Core + SQL Client)
  - External API call tracing (PokeAPI integration)
  - Exception tracking and error rates
- **Metrics collection**:
  - ASP.NET Core request metrics
  - HTTP client metrics
  - .NET Runtime metrics (GC, memory, threads)
  - Process-level metrics (CPU, memory usage)
- **Structured logging** with Serilog across all endpoints
- **Direct log forwarding** to Datadog cloud intake API
- **Real-time log viewing** in Datadog dashboard with rich context
- **Log enrichment** with service name, environment, and custom properties
- **Local OTLP agent support** for development and testing

**Quick Start APM**: See [DATADOG_APM_QUICKSTART.md](docs/DATADOG_APM_QUICKSTART.md)  
**Detailed APM Setup**: See [LOCAL_DATADOG_APM.md](docs/LOCAL_DATADOG_APM.md)  
**Logging Configuration**: See [DATADOG_LOGGING.md](docs/DATADOG_LOGGING.md)

### Architecture & Quality

- **Clean separation of concerns** using hexagonal architecture
- **Health checks** endpoint for monitoring
- **Swagger UI** for API documentation and testing
- **Comprehensive code analysis** with Roslyn and SonarAnalyzer
- **Automated security scanning** and quality gates
- **GitHub Actions CI/CD** pipeline

## Code Quality & Analysis

This project is configured with comprehensive static code analysis tools:

- **Microsoft .NET Analyzers**: Core .NET code quality and security rules
- **SonarAnalyzer for C#**: Industry-standard code quality analysis
- **Security Code Scan**: Security vulnerability detection
- **AsyncUsage Analyzers**: Async/await best practices
- **Custom Rulesets**: Tailored rule configuration for the project

For detailed information about the analyzer configuration, see [ANALYZER_SETUP.md](ANALYZER_SETUP.md).

### Running Code Analysis

```bash
# Build with analyzer warnings
dotnet build src/App.slnx --verbosity normal

# Run tests with coverage
dotnet test src/App.slnx --collect:"XPlat Code Coverage"

# Format code
csharpier format .
```

## Getting Started

### Prerequisites

Before running the application, ensure you have:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Any code editor (preferably Visual Studio or Visual Studio Code)
- **Datadog Agent** (optional, for APM and metrics - logs go directly to cloud)

> **Note**: This application uses Aspire 13.1.1 which works directly with .NET 10 using the new SDK pattern (no workload installation required).

### ✅ What's Included

✅ **Aspire Dashboard** - Visual orchestration and monitoring  
✅ **OpenTelemetry** - Distributed tracing and metrics  
✅ **Health Checks** - `/health` (readiness) and `/alive` (liveness)  
✅ **HTTP Resilience** - Automatic retry, circuit breaker, timeout  
✅ **Service Discovery** - Ready for multi-service scenarios  
✅ **.NET 10** - Latest .NET runtime

### What Changed in Aspire 13.1.1

The project has been upgraded from Aspire 9.0.0 to **Aspire 13.1.1** using the new SDK pattern:

**Before (Aspire 9.0.0):**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Aspire.Hosting" Version="9.0.0" />
    <PackageReference Include="Aspire.Hosting.AppHost" Version="9.0.0" />
  </ItemGroup>
</Project>
```

**After (Aspire 13.1.1):**

```xml
<Project Sdk="Aspire.AppHost.Sdk/13.1.1">
  <!-- Aspire.Hosting packages now included automatically by SDK -->
</Project>
```

**Key Improvements:**

1. **Simplified SDK** - Uses `Aspire.AppHost.Sdk/13.1.1` directly
2. **No Workload Required** - Pure NuGet package approach
3. **Full .NET 10 Support** - No compatibility issues
4. **Cleaner Project Files** - Removed boilerplate package references

For detailed migration information, see [ASPIRE_INTEGRATION.md](docs/ASPIRE_INTEGRATION.md).

### Datadog Agent Setup (Optional)

The application logs are sent directly to Datadog cloud, but you can optionally run the Datadog agent for APM traces and metrics:

```bash
# The agent is pre-installed in the dev container
# Or run with script (if not in container):
./run-datadog-agent.sh
```

For detailed setup instructions, see [DATADOG_LOGGING.md](docs/DATADOG_LOGGING.md).

### Clone the Repository

```bash
git clone https://github.com/yourusername/hexagon-dotnet-app.git
cd hexagon-dotnet-app
```

### Quick Start

**Option 1: With Aspire Dashboard (Recommended)**

.NET Aspire provides orchestration, service discovery, and an observability dashboard:

```bash
dotnet run --project src/App.AppHost
```

**Access Points:**

- 🎯 **Aspire Dashboard**: http://localhost:17123 _(HTTP only - no SSL errors!)_
- 🌐 **API**: http://localhost:5112
- 🏥 **Health Check**: http://localhost:5112/health
- 💓 **Liveness**: http://localhost:5112/alive

> **Note**: The dashboard runs on **HTTP** to eliminate SSL certificate errors in development. Configured via `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`.

The dashboard provides:

- Real-time application logs
- Distributed tracing visualization
- Metrics and performance data
- Resource management

**Option 2: Direct API Execution (Without Dashboard)**

You can also run the API service independently:

```bash
dotnet run --project src/App.Api
```

**Access Points:**

- 🌐 **API**: http://localhost:5112
- 🏥 **Health Check**: http://localhost:5112/health
- 💓 **Liveness**: http://localhost:5112/alive

**Option 3: Using the Watch Command (Development)**

For hot reload during development:

```bash
dotnet watch --project src/App.Api
```

### Build and Test

Build the entire solution:

```bash
dotnet build src/App.slnx
```

Run all tests (81 tests):

```bash
dotnet test src/App.slnx
```

Run tests with code coverage:

```bash
dotnet test src/App.slnx --collect:"XPlat Code Coverage"
```

All tests pass! ✅

### Test the API

#### Health Checks

```bash
# Readiness check (dependencies healthy)
curl http://localhost:5112/health

# Liveness check (application running)
curl http://localhost:5112/alive
```

#### Todo Examples

```bash
# Get all todos
curl http://localhost:5112/api/v1/todos

# Create a new todo
curl -X POST http://localhost:5112/api/v1/todos \
  -H "Content-Type: application/json" \
  -d '{"title": "Test Aspire Integration", "isCompleted": false}'

# Get completed todos
curl http://localhost:5112/api/v1/todos/completed

# Update a todo
curl -X PUT http://localhost:5112/api/v1/todos/1 \
  -H "Content-Type: application/json" \
  -d '{"title": "Updated Todo", "isCompleted": true}'

# Delete a todo
curl -X DELETE http://localhost:5112/api/v1/todos/1
```

#### Pokemon API Examples (External Gateway)

```bash
# Get first 3 Pokemon
curl "http://localhost:5112/api/v1/pokemon?limit=3"

# Get specific Pokemon by ID
curl http://localhost:5112/api/v1/pokemon/25
```

### API Documentation

When running in development mode, Swagger UI is available at the root URL:

```
http://localhost:5112
```

### View Logs in Datadog

Application logs are automatically forwarded to Datadog:

1. Navigate to [Datadog Logs](https://us5.datadoghq.com/logs)
2. Search with filter: `service:hexagon-dotnet-app`
3. View structured logs with enriched properties (environment, application, operation details)

Local JSON log files are also available at `src/App.Api/logs/` for debugging.

### Troubleshooting

#### Port Already in Use

If you see `Failed to bind to address https://127.0.0.1:22222: address already in use`:

```bash
# Find and kill process on the port
lsof -ti:22222 | xargs kill -9

# Or on Windows:
netstat -ano | findstr :22222
taskkill /PID <PID> /F

# Then run again
dotnet run --project src/App.AppHost
```

#### SSL Certificate Warnings

**✅ SSL has been disabled in development!** The dashboard now runs on plain HTTP to eliminate all certificate errors.

Configuration changes made:

- Dashboard URL: `http://localhost:17123` (was https://localhost:22222)
- Environment variable: `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`
- All Aspire endpoints use HTTP in development

**No more SSL errors!** 🎉

> **Production Note**: In production, you should use HTTPS with proper certificates. The HTTP configuration is **only for development** to avoid certificate issues.

#### Database Issues

If you encounter database errors:

```bash
# Delete the database file
rm src/App.Api/app.db

# Restart the application (database will be recreated)
dotnet run --project src/App.Api
```

#### Datadog Logs Not Appearing

If logs aren't showing up in Datadog:

1. Verify `DD_API_KEY` environment variable is set
2. Check network connectivity to Datadog intake URL
3. View local log files: `tail -f src/App.Api/logs/app*.log`
4. Check Datadog site setting matches your account region

## API Endpoints

### Todo Endpoints

All Todo endpoints are prefixed with `/api/v1/todos`:

| Method | URL                      | Description                     |
| ------ | ------------------------ | ------------------------------- |
| GET    | /api/v1/todos            | Find all todo items             |
| GET    | /api/v1/todos/{id}       | Find a specific todo item by ID |
| GET    | /api/v1/todos/completed  | Find all completed todo items   |
| GET    | /api/v1/todos/incomplete | Find all incomplete todo items  |
| POST   | /api/v1/todos            | Create a new todo item          |
| PUT    | /api/v1/todos/{id}       | Update an existing todo item    |
| DELETE | /api/v1/todos/{id}       | Delete a todo item              |

### Pokemon Gateway Endpoints

All Pokemon endpoints are prefixed with `/api/v1/pokemon`:

| Method | URL                               | Description                       |
| ------ | --------------------------------- | --------------------------------- |
| GET    | /api/v1/pokemon                   | Get list of Pokemon (default: 20) |
| GET    | /api/v1/pokemon?limit=10&offset=0 | Get paginated list of Pokemon     |
| GET    | /api/v1/pokemon/{id}              | Get specific Pokemon by ID        |

### Additional Endpoints

| Method | URL     | Description                     |
| ------ | ------- | ------------------------------- |
| GET    | /health | Health check (readiness)        |
| GET    | /alive  | Liveness check (Aspire default) |

## Code Formatting

This project uses [CSharpier](https://github.com/belav/csharpier) for code formatting. To format the code:

```bash
csharpier format .
```

## Project Structure

```
/
├── docs/
│   ├── DATADOG_LOGGING.md            # Datadog logging setup guide
│   └── GATEWAY_IMPLEMENTATION.md     # Gateway module documentation
├── src/
│   ├── App.slnx
│   ├── Directory.Build.props
│   ├── GlobalSuppressions.cs
│   ├── App.AppHost/                  # .NET Aspire Orchestration
│   │   ├── Program.cs                 # Aspire app configuration
│   │   ├── appsettings.json
│   │   └── App.AppHost.csproj
│   ├── App.ServiceDefaults/          # Shared Observability Configuration
│   │   ├── Extensions.cs              # OpenTelemetry & health checks
│   │   └── App.ServiceDefaults.csproj
│   ├── App.Api/                      # HTTP Adapters (Primary - Minimal APIs)
│   │   ├── Pokemon/
│   │   │   ├── PokemonEndpoints.cs    # Pokemon endpoint handlers
│   │   │   └── PokemonEndpointsExtensions.cs
│   │   ├── Todo/
│   │   │   ├── TodoEndpoints.cs       # Todo handlers with structured logging
│   │   │   └── TodoEndpointsExtensions.cs
│   │   ├── logs/                      # JSON log files (auto-created)
│   │   ├── Program.cs                 # Application entry point with Serilog
│   │   ├── App.http                   # HTTP test requests
│   │   ├── appsettings.json           # Serilog configuration
│   │   ├── appsettings.Development.json
│   │   └── App.Api.csproj
│   ├── App.Core/                     # Domain Logic & Ports
│   │   ├── AppCore.cs                 # Core DI registration
│   │   ├── Entities/
│   │   │   ├── Entity.cs              # Base entity with Id, timestamps
│   │   │   ├── IEntity.cs
│   │   │   └── TodoEntity.cs          # Todo domain model
│   │   ├── Pokemon/
│   │   │   ├── Pokemon.cs             # Pokemon domain model
│   │   │   └── IPokemonGateway.cs     # Pokemon gateway port
│   │   ├── Repositories/
│   │   │   └── IRepository.cs         # Generic repository interface
│   │   ├── Todo/
│   │   │   ├── ITodoRepository.cs     # Todo repository port
│   │   │   └── TodoService.cs         # Todo business logic
│   │   └── App.Core.csproj
│   ├── App.Data/                     # Infrastructure Adapters (Secondary)
│   │   ├── AppData.cs                 # Data layer DI registration
│   │   ├── AppDbContext.cs            # EF Core DbContext
│   │   ├── Todo/
│   │   │   └── TodoRepository.cs      # Todo repository implementation
│   │   └── App.Data.csproj
│   └── App.Gateway/                  # External Service Adapters (Secondary)
│       ├── AppGateway.cs              # Gateway DI registration
│       ├── README.md                  # Gateway module guide
│       ├── Client/
│       │   ├── IPokeClient.cs         # HTTP client interface
│       │   ├── PokeClient.cs          # HTTP client implementation
│       │   └── PokeResponse.cs        # Generic response wrapper
│       ├── Pokemon/
│       │   └── PokemonGateway.cs      # Pokemon gateway implementation
│       └── App.Gateway.csproj
└── test/
    ├── App.Api.Tests/                # API Unit & Integration Tests
    │   ├── Todo/
    │   │   └── TodoEndpointsTests.cs
    │   ├── Integration/
    │   │   ├── IntegrationTestWebAppFactory.cs
    │   │   ├── TodoIntegrationTests.cs
    │   │   └── PokemonIntegrationTests.cs
    │   └── App.Api.Tests.csproj
    ├── App.Core.Tests/               # Core Unit Tests
    │   ├── Todo/
    │   │   └── TodoServiceTests.cs
    │   ├── Pokemon/
    │   │   └── PokemonServiceTests.cs
    │   └── App.Core.Tests.csproj
    └── App.Gateway.Tests/            # Gateway Unit Tests
        ├── Client/
        │   └── PokeClientTests.cs
        ├── Pokemon/
        │   └── PokemonGatewayTests.cs
        └── App.Gateway.Tests.csproj
```

## Observability

### Datadog Integration

This application includes comprehensive Datadog observability:

**Logging (Serilog → Datadog):**

- Structured JSON logs with rich context
- Direct HTTPS forwarding to Datadog cloud intake (us5.datadoghq.com)
- Log enrichment with service, environment, and application metadata
- Comprehensive error tracking with exception details
- Operation-level logging for all CRUD operations
- Local file logging for debugging (`src/App.Api/logs/app*.log`)
- Daily log rotation with 7-day retention

**APM & Metrics (Agent):**

- Datadog agent listens on ports 8125 (StatsD) and 8126 (APM)
- Automatic trace collection and performance monitoring
- Custom metrics forwarding available

**Configuration Files:**

- Application logging: [src/App.Api/Program.cs](src/App.Api/Program.cs)
- Log settings: [src/App.Api/appsettings.json](src/App.Api/appsettings.json)
- Datadog log collection: [datadog-logs.yaml](datadog-logs.yaml)
- Agent startup script: [run-datadog-agent.sh](run-datadog-agent.sh)

**Viewing Logs:**

- Datadog Dashboard: https://us5.datadoghq.com/logs
- Filter by: `service:hexagon-dotnet-app`
- Local logs: `src/App.Api/logs/app[YYYYMMDD].log`

For complete setup instructions, see [docs/DATADOG_LOGGING.md](docs/DATADOG_LOGGING.md).

## Dev Container Configuration

This project includes a dev container with pre-installed Datadog Agent. The container configuration includes:

- .NET 10 SDK
- Datadog Agent 7 (pre-installed but not auto-started)
- Environment variables for Datadog integration
- APM instrumentation enabled for multiple languages
- Security agent disabled for dev container optimization

The Datadog agent is configured during container build but requires manual start:

```bash
# Start the agent in the dev container
sudo service datadog-agent start

# Check agent status
sudo service datadog-agent status
```

See [.devcontainer/Dockerfile](.devcontainer/Dockerfile) for complete container configuration.

## License

[MIT](LICENSE)
