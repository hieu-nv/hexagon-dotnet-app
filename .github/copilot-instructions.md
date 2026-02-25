# Hexagon .NET App - AI Coding Assistant Instructions

## Architecture Overview

This is a .NET 10 application implementing **hexagonal (ports and adapters) architecture** with three distinct layers:

- **App.Core**: Domain logic, entities, and port interfaces (e.g., `ITodoRepository`)
- **App.Data**: Infrastructure adapters implementing ports (e.g., `TodoRepository` implements `ITodoRepository`)
- **App.Api**: HTTP adapters using ASP.NET Core minimal APIs

## Key Patterns & Conventions

### Extension Method Configuration Pattern

Each layer exposes configuration through extension methods on `WebApplicationBuilder` and `WebApplication`:

```csharp
// In Program.cs
builder.UseAppCore();    // Registers domain services
builder.UseAppData();    // Configures DbContext, repositories
builder.UseTodo();       // Registers endpoint handlers

app.UseAppData();        // Ensures database creation
app.UseTodo();           // Maps HTTP endpoints
```

### Domain-Organized Structure

Code is organized by **domain concepts** (not technical layers):

- `src/App.Core/Todo/` contains `TodoService.cs`, `ITodoRepository.cs`
- `src/App.Data/Todo/` contains `TodoRepository.cs`
- `src/App.Api/Todo/` contains `TodoEndpoints.cs`

### Minimal API Endpoint Pattern

Endpoints use a handler class pattern with extension methods:

```csharp
// Handler class with injected dependencies
sealed class TodoEndpoints(TodoService todoService) { /* methods */ }

// Extension methods for registration and mapping
internal static class TodoEndpointsExtensions
{
    public static WebApplicationBuilder UseTodo(this WebApplicationBuilder app) // DI registration
    public static IEndpointRouteBuilder UseTodo(this IEndpointRouteBuilder routes) // Route mapping
}
```

### Entity Conventions

- All entities inherit from `Entity<T>` base class with `Id`, `CreatedAt`, `UpdatedAt`
- Column names use UPPERCASE with underscores: `[Column("IS_COMPLETED")]`
- Nullable reference types enabled project-wide

## Development Workflows

### Build & Test

```bash
dotnet build                                    # Build entire solution
dotnet run --project src/App.Api              # Run API (http://localhost:5112)
dotnet test                                    # Run all tests
dotnet test --collect:"XPlat Code Coverage"   # Run with coverage
```

### Code Formatting

Uses CSharpier for formatting:

```bash
csharpier format .                             # Format all files
```

### Database

- SQLite with Entity Framework Core
- Database auto-created on app startup via `UseAppData()` extension
- Connection string: `"Data Source=app.db"` (default)

## Code Quality Standards

- **Nullable reference types**: Enabled via `Directory.Build.props`
- **Code analysis**: `AnalysisMode=All` with latest analyzers
- **Documentation**: XML comments required for public APIs
- **Dependency injection**: Constructor injection with primary constructors
- **ConfigureAwait(false)**: Used consistently in async service methods

## Testing Conventions

- Uses **xUnit** with **Moq** for mocking
- Test project: `test/App.Core.Tests/`
- Repository interfaces mocked, services tested directly
- Test naming: `MethodName_Condition_ExpectedResult`
- Coverage reports generated in `test/App.Core.Tests/TestResults/`

## Integration Points

- **JSON serialization**: Custom `JsonSerializerContext` for AOT compatibility
- **Entity Framework**: DbContext configured in `AppData.UseAppData()`
- **Swagger**: Auto-enabled in development mode
- **HTTP endpoints**: Grouped under `/todos` with RESTful patterns

## When Adding New Features

1. **Domain first**: Create entities, services, repository interfaces in `App.Core/{Domain}/`
2. **Infrastructure**: Implement repository in `App.Data/{Domain}/`
3. **API**: Create endpoints handler and extensions in `App.Api/{Domain}/`
4. **Wire up**: Add `Use{Domain}()` calls to `Program.cs`
5. **Tests**: Add service tests in `test/App.Core.Tests/{Domain}/`

## Container

This project use podman instead of using docker.
