# Hexagon .NET App

A modern ASP.NET Core application using minimal APIs to demonstrate hexagonal architecture with both internal data persistence and external service integration.

## Overview

This application demonstrates the implementation of a hexagonal (ports and adapters) architecture in ASP.NET Core with two distinct use cases:

- **Todo API**: Internal data persistence using Entity Framework Core
- **Pokemon API**: External service integration via gateway pattern

It provides a structured approach to building maintainable and testable applications by separating business logic from external concerns.

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

### Datadog Observability & Logging

- **Structured logging** with Serilog across all endpoints
- **Direct log forwarding** to Datadog cloud intake API
- **Datadog APM agent** integration for traces and metrics
- **Real-time log viewing** in Datadog dashboard with rich context
- **Log enrichment** with service name, environment, and custom properties
- **StatsD metrics** forwarding (port 8125)
- **APM traces** collection (port 8126)
- **JSON log files** for local debugging and agent tailing

For detailed logging setup and configuration, see [DATADOG_LOGGING.md](docs/DATADOG_LOGGING.md).

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

This will start:

- **Aspire Dashboard**: `http://localhost:17123` (view logs, traces, metrics)
- **API Service**: `http://localhost:5112`

> **Note**: The dashboard runs on HTTP (not HTTPS) in development to avoid SSL certificate errors. This is configured via `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true` in launch settings.

The dashboard provides:

- Real-time application logs
- Distributed tracing visualization
- Metrics and performance data
- Resource management

**Option 2: Direct API Execution**

You can also run the API service independently:

```bash
dotnet run --project src/App.Api
```

The API will be available at `http://localhost:5112`.

**Option 3: Using the Watch Command (Development)**

For hot reload during development:

```bash
dotnet watch --project src/App.Api
```

### Build and Test

```bash
dotnet build src/App.slnx
```

Run all tests:

```bash
dotnet test src/App.slnx
```

Run tests with coverage:

```bash
dotnet test src/App.slnx --collect:"XPlat Code Coverage"
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
    ├── App.Api.Tests/                # API Integration Tests
    │   ├── Todo/
    │   │   └── TodoEndpointsTests.cs
    │   └── App.Api.Tests.csproj
    └── App.Core.Tests/               # Unit Tests
        ├── Todo/
        │   └── TodoServiceTests.cs
        └── App.Core.Tests.csproj
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
