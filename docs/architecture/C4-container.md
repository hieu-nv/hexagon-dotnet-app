# C4 Container Diagram

This diagram provides a high-level view of the application's containers (applications, databases, etc.) and their relationships.

```mermaid
C4Container
    title Container Diagram for Hexagon .NET Application

    Person(user, "Application User", "A user interacting with the mobile or web interface to manage Todos or view Pokemon.")

    System_Boundary(c1, "Hexagon .NET Solution") {
        Container(api, "Web API", ".NET 10, Minimal API", "Exposes Todo and Pokemon endpoints, implements business logic.")
        ContainerDb(database, "SQLite Database", "Local File", "Stores todo items and application state.")
    }

    System_Ext(pokeapi, "PokeAPI", "External REST API providing Pokemon data.")
    System_Ext(datadog, "Datadog", "Observability platform for logs, metrics, and traces.")

    Rel(user, api, "Uses", "HTTPS")
    Rel(api, database, "Reads/Writes", "EF Core")
    Rel(api, pokeapi, "Consumes", "HTTPS/JSON")
    Rel(api, datadog, "Sends Telemetry", "OTLP/HTTPS")

    UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

## Containers

- **Web API**: The main entry point for the system. It handles HTTP requests, authenticates users, and coordinates between the database and external services.
- **SQLite Database**: A persistent storage system for managing Todo items.
- **PokeAPI**: A third-party external service used to fetch Pokemon details.
- **Datadog**: External observability platform used for monitoring system health and performance.
 village
