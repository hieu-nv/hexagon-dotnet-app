# Hexagon .NET App

A modern ASP.NET Core application using minimal APIs to manage Todo items with a clean hexagonal architecture.

## Overview

This application demonstrates the implementation of a Todo list API using a hexagonal (ports and adapters) architecture in ASP.NET Core. It provides a structured approach to building maintainable and testable applications by separating business logic from external concerns.

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

- CRUD operations for Todo items
- Filtering todos by completion status
- Clean separation of concerns using hexagonal architecture
- SQLite database for data persistence
- Swagger UI for API documentation and testing
- Comprehensive code analysis with Roslyn and SonarAnalyzer
- Automated security scanning and quality gates
- GitHub Actions CI/CD pipeline

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
dotnet build src/App.sln --verbosity normal

# Run tests with coverage
dotnet test src/App.sln --collect:"XPlat Code Coverage"

# Format code
csharpier format .
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Any code editor (preferably Visual Studio or Visual Studio Code)

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/yourusername/hexagon-dotnet-app.git
cd hexagon-dotnet-app
```

### Run the Application

```bash
dotnet run --project App.Api
```

The API will be available at `http://localhost:5112`.

### Swagger Documentation

When running in development mode, Swagger UI is available at the root URL:

```
http://localhost:5112
```

## API Endpoints

| Method | URL               | Description                     |
| ------ | ----------------- | ------------------------------- |
| GET    | /todos            | Find all todo items             |
| GET    | /todos/{id}       | Find a specific todo item by ID |
| GET    | /todos/completed  | Find all completed todo items   |
| GET    | /todos/incomplete | Find all incomplete todo items  |

## Code Formatting

This project uses [CSharpier](https://github.com/belav/csharpier) for code formatting. To format the code:

```bash
csharpier format .
```

## Project Structure

```
App.sln
├── App.Api/
│   ├── Todo/
│   │   └── TodoEndpoints.cs
│   ├── Program.cs
│   └── App.Api.csproj
├── App.Core/
│   ├── AppCore.cs
│   ├── Entities/
│   │   ├── Entity.cs
│   │   ├── IEntity.cs
│   │   └── TodoEntity.cs
│   ├── Repositories/
│   │   └── IRepository.cs
│   ├── Todo/
│   │   ├── ITodoRepository.cs
│   │   └── TodoService.cs
│   └── App.Core.csproj
└── App.Data/
    ├── AppData.cs
    ├── AppDbContext.cs
    ├── Repositories/
    │   └── Repository.cs
    ├── Todo/
    │   └── TodoRepository.cs
    └── App.Data.csproj
```

## License

[MIT](LICENSE)
