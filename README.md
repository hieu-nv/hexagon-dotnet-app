# Hexagon .NET App

A modern ASP.NET Core application using minimal APIs to demonstrate hexagonal architecture with both internal data persistence and external service integration.

## Overview

This application demonstrates the implementation of a hexagonal (ports and adapters) architecture in ASP.NET Core with two distinct use cases:

- **Todo API**: Internal data persistence using Entity Framework Core
- **Pokemon API**: External service integration via gateway pattern

It provides a structured approach to building maintainable and testable applications by separating business logic from external concerns.

**Key Technologies:**

- ASP.NET Core 10 with Minimal APIs
- Entity Framework Core with SQLite
- Dependency Injection
- Health Checks
- OpenAPI/Swagger Documentation

## Architecture

The application follows a hexagonal architecture pattern:

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

## Features

### Todo Management (Internal Persistence)

- **Full CRUD operations** for Todo items (Create, Read, Update, Delete)
- **Filtering** todos by completion status (completed/incomplete)
- **Validation** with data annotations and business rule enforcement
- **SQLite database** for data persistence with Entity Framework Core
- **Automatic timestamp tracking** (CreatedAt, UpdatedAt)

### Pokemon API (External Gateway)

- **External API integration** with PokeAPI using gateway pattern
- **Gateway abstraction** for testability and flexibility
- **HTTP client factory** for efficient connection management
- **Paginated list endpoint** with limit and offset parameters
- **Individual Pokemon lookup** by ID

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

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or .NET 9 SDK)
- Any code editor (preferably Visual Studio or Visual Studio Code)

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/yourusername/hexagon-dotnet-app.git
cd hexagon-dotnet-app
```

### Run the Application

```bash
dotnet run --project src/App.Api
```

The API will be available at `http://localhost:5112`.

### Build the Solution

```bash
dotnet build src/App.slnx
```

### Run Tests

```bash
dotnet test src/App.slnx
```

### Swagger Documentation

When running in development mode, Swagger UI is available at the root URL:

```
http://localhost:5112
```

## API Endpoints

### Todo Endpoints

All Todo endpoints are prefixed with `/todos`:

| Method | URL               | Description                     |
| ------ | ----------------- | ------------------------------- |
| GET    | /todos            | Find all todo items             |
| GET    | /todos/{id}       | Find a specific todo item by ID |
| GET    | /todos/completed  | Find all completed todo items   |
| GET    | /todos/incomplete | Find all incomplete todo items  |
| POST   | /todos            | Create a new todo item          |
| PUT    | /todos/{id}       | Update an existing todo item    |
| DELETE | /todos/{id}       | Delete a todo item              |

### Pokemon Gateway Endpoints

All Pokemon endpoints are prefixed with `/api/v1/pokemon`:

| Method | URL                               | Description                       |
| ------ | --------------------------------- | --------------------------------- |
| GET    | /api/v1/pokemon                   | Get list of Pokemon (default: 20) |
| GET    | /api/v1/pokemon?limit=10&offset=0 | Get paginated list of Pokemon     |
| GET    | /api/v1/pokemon/{id}              | Get specific Pokemon by ID        |

### Additional Endpoints

| Method | URL     | Description  |
| ------ | ------- | ------------ |
| GET    | /health | Health check |

## Code Formatting

This project uses [CSharpier](https://github.com/belav/csharpier) for code formatting. To format the code:

```bash
csharpier format .
```

## Project Structure

```
/
├── docs/
│   └── GATEWAY_IMPLEMENTATION.md     # Gateway module documentation
├── src/
│   ├── App.slnx
│   ├── Directory.Build.props
│   ├── GlobalSuppressions.cs
│   ├── App.Api/                      # HTTP Adapters (Primary - Minimal APIs)
│   │   ├── Pokemon/
│   │   │   ├── PokemonEndpoints.cs    # Pokemon endpoint handlers
│   │   │   └── PokemonEndpointsExtensions.cs
│   │   ├── Todo/
│   │   │   ├── TodoEndpoints.cs       # Todo endpoint handlers
│   │   │   └── TodoEndpointsExtensions.cs
│   │   ├── Program.cs                 # Application entry point
│   │   ├── App.http                   # HTTP test requests
│   │   ├── appsettings.json
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

## License

[MIT](LICENSE)
