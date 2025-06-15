# Hexagon .NET App

A modern ASP.NET Core application using minimal APIs to manage Todo items with a clean hexagonal architecture.

## Overview

This application demonstrates the implementation of a Todo list API using a hexagonal (ports and adapters) architecture in ASP.NET Core. It provides a structured approach to building maintainable and testable applications by separating business logic from external concerns.

## Architecture

The application follows a hexagonal architecture pattern:

- **App.Core**: Contains the domain model, entities, and repository interfaces (ports)
- **App.Data**: Implements the data access layer and repository implementations (adapters)
- **App.Api**: Exposes the HTTP API endpoints using minimal API syntax

## Features

- CRUD operations for Todo items
- Filtering todos by completion status
- Clean separation of concerns using hexagonal architecture
- SQLite database for data persistence
- Swagger UI for API documentation and testing

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

| Method | URL                    | Description                           |
|--------|------------------------|---------------------------------------|
| GET    | /todos                 | Get all todo items                    |
| GET    | /todos/{id}            | Get a specific todo item by ID        |
| GET    | /todos/completed       | Get all completed todo items          |
| GET    | /todos/incomplete      | Get all incomplete todo items         |

## Code Formatting

This project uses [CSharpier](https://github.com/belav/csharpier) for code formatting. To format the code:

```bash
csharpier format .
```

## Project Structure

```
App.sln
├── App.Api/
│   ├── Endpoints/
│   │   └── TodoEndpoints.cs
│   ├── Controllers/
│   ├── Program.cs
│   └── App.Api.csproj
├── App.Core/
│   ├── Entities/
│   │   ├── Entity.cs
│   │   ├── IEntity.cs
│   │   └── TodoEntity.cs
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   └── ITodoRepository.cs
│   └── App.Core.csproj
└── App.Data/
    ├── Data.cs
    ├── Data/
    │   └── AppDbContext.cs
    ├── Repositories/
    │   ├── Repository.cs
    │   └── TodoRepository.cs
    └── App.Data.csproj
```

## License

[MIT](LICENSE)
