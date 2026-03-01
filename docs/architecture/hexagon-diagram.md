# Hexagonal Architecture Diagram

This diagram visualizes the application's structure following the Hexagonal Architecture (Ports and Adapters) pattern.

```mermaid
graph TB
    subgraph "Core (Inbound & Outbound Ports)"
        Domain["Domain Entities (TodoEntity, Pokemon)"]
        Services["Domain Services (TodoService, PokemonService)"]
        RepoPort["Repository Ports (ITodoRepository)"]
        GatePort["Gateway Ports (IPokemonGateway)"]
    end

    subgraph "Primary Adapters (Inbound)"
        API["Minimal API (App.Api)"]
        Swagger["Swagger/OpenAPI"]
    end

    subgraph "Secondary Adapters (Outbound)"
        EF["EF Core Repository (App.Data)"]
        PokeGate["PokeAPI Gateway (App.Gateway)"]
    end

    subgraph "External Systems"
        SQLite["SQLite Database"]
        PokeAPI["PokeAPI (External REST)"]
    end

    %% Inbound Flow
    API --> Services
    Swagger --> API

    %% Service to Port Flow
    Services --> Domain
    Services --> RepoPort
    Services --> GatePort

    %% Outbound implementation
    EF -- implements --> RepoPort
    PokeGate -- implements --> GatePort

    %% Adapter to Infrastructure
    EF --> SQLite
    PokeGate --> PokeAPI

    style Domain fill:#f9f,stroke:#333,stroke-width:2px
    style Services fill:#bbf,stroke:#333,stroke-width:2px
    style RepoPort fill:#dfd,stroke:#333,stroke-dasharray: 5 5
    style GatePort fill:#dfd,stroke:#333,stroke-dasharray: 5 5
```

## Key Components

- **Domain Entities**: Core business objects and logic.
- **Domain Services**: Business workflows that coordinate domain entities and ports.
- **Ports (Interfaces)**: Define how the core interacts with the outside world (Repository Port, Gateway Port).
- **Primary Adapters**: Entry points to the application (API endpoints).
- **Secondary Adapters**: Implementations of ports that talk to external infrastructure.
