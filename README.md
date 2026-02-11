# Hexagon .NET App

A modern ASP.NET Core application using minimal APIs to manage Todo items with a clean hexagonal architecture.

## Overview

This application demonstrates the implementation of a Todo list API using a hexagonal (ports and adapters) architecture in ASP.NET Core. It provides a structured approach to building maintainable and testable applications by separating business logic from external concerns.

**Key Technologies:**

- ASP.NET Core 10 with Minimal APIs
- Entity Framework Core with SQLite
- Dependency Injection
- Health Checks
- OpenAPI/Swagger Documentation

## Architecture

The application follows a hexagonal architecture pattern:

- **App.Core**: Contains the domain model, entities, repository interfaces (ports), and services
  - Organized with domain-focused directories (e.g., Todo/)
  - Includes the AppCore class for core service registration

- **App.Data**: Implements the data access layer and repository implementations (adapters)
  - Includes AppDbContext for Entity Framework Core data access
  - Implements the repository pattern with generic and specific repositories
  - AppData class provides extension methods for data layer configuration

- **App.Api**: Exposes the HTTP API endpoints using minimal API syntax
  - Uses extension methods for clean endpoint registration
  - Organized by domain rather than technical concerns
  - Leverages dependency injection for clean service resolution

## Features

- **Full CRUD operations** for Todo items (Create, Read, Update, Delete)
- **Filtering** todos by completion status (completed/incomplete)
- **Validation** with data annotations and business rule enforcement
- **Clean separation of concerns** using hexagonal architecture
- **SQLite database** for data persistence with Entity Framework Core
- **Health checks** endpoint for monitoring
- **Swagger UI** for API documentation and testing
- **Comprehensive code analysis** with Roslyn and SonarAnalyzer
- **Automated security scanning** and quality gates
- **GitHub Actions CI/CD** pipeline
- **Automatic timestamp tracking** (CreatedAt, UpdatedAt)

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

All endpoints are prefixed with `/api/v1/todos`:

| Method | URL                      | Description                     |
| ------ | ------------------------ | ------------------------------- |
| GET    | /api/v1/todos            | Find all todo items             |
| GET    | /api/v1/todos/{id}       | Find a specific todo item by ID |
| GET    | /api/v1/todos/completed  | Find all completed todo items   |
| GET    | /api/v1/todos/incomplete | Find all incomplete todo items  |
| POST   | /api/v1/todos            | Create a new todo item          |
| PUT    | /api/v1/todos/{id}       | Update an existing todo item    |
| DELETE | /api/v1/todos/{id}       | Delete a todo item              |

**Additional Endpoints:**

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
├── src/
│   ├── App.slnx
│   ├── Directory.Build.props
│   ├── GlobalSuppressions.cs
│   ├── App.Api/                      # HTTP Adapters (Minimal APIs)
│   │   ├── Todo/
│   │   │   ├── TodoEndpoints.cs       # Endpoint handlers
│   │   │   └── TodoEndpointsExtensions.cs  # DI and route registration
│   │   ├── Program.cs                 # Application entry point
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── App.Api.csproj
│   ├── App.Core/                     # Domain Logic & Ports
│   │   ├── AppCore.cs                 # Core DI registration
│   │   ├── Entities/
│   │   │   ├── Entity.cs              # Base entity with Id, timestamps
│   │   │   ├── IEntity.cs
│   │   │   └── TodoEntity.cs          # Todo domain model
│   │   ├── Repositories/
│   │   │   └── IRepository.cs         # Generic repository interface
│   │   ├── Todo/
│   │   │   ├── ITodoRepository.cs     # Todo repository port
│   │   │   └── TodoService.cs         # Todo business logic
│   │   └── App.Core.csproj
│   └── App.Data/                     # Infrastructure Adapters
│       ├── AppData.cs                 # Data layer DI registration
│       ├── AppDbContext.cs            # EF Core DbContext
│       ├── Todo/
│       │   └── TodoRepository.cs      # Todo repository implementation
│       └── App.Data.csproj
└── test/
    └── App.Core.Tests/               # Unit Tests
        ├── Todo/
        │   └── TodoServiceTests.cs
        └── App.Core.Tests.csproj
```

## License

[MIT](LICENSE)
