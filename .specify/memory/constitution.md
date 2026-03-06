<!-- 
Sync Impact Report
- Version change: null → 1.0.0
- List of modified principles:
  - [PRINCIPLE_1_NAME] → I. Hexagonal Architecture (Ports and Adapters)
  - [PRINCIPLE_2_NAME] → II. Dependency Inversion Principle
  - [PRINCIPLE_3_NAME] → III. Test-First & High Coverage (NON-NEGOTIABLE)
  - [PRINCIPLE_4_NAME] → IV. Observability-First Development
  - [PRINCIPLE_5_NAME] → V. API Consistency & Versioning
- Added sections: Technology Stack & Constraints, Development Workflow
- Removed sections: None
- Templates requiring updates:
  - .specify/templates/plan-template.md (✅ updated)
  - .specify/templates/tasks-template.md (✅ updated)
- Follow-up TODOs: None
-->

# Hexagon .NET App Constitution

## Core Principles

### I. Hexagonal Architecture (Ports and Adapters)
The application MUST maintain a strict separation between core business logic and external concerns. Business logic resides in `App.Core` and must not depend on UI, databases, or external services. External integrations MUST be implemented as adapters in `App.Data`, `App.Gateway`, or `App.Api`, interacting with the core only through defined ports (interfaces).

### II. Dependency Inversion Principle
High-level business rules (Core) MUST NOT depend on low-level implementation details (Infrastructure). Both MUST depend on abstractions (Interfaces). Dependencies MUST be injected via constructor injection. Core logic must be agnostic of the persistence mechanism or transport protocol.

### III. Test-First & High Coverage (NON-NEGOTIABLE)
New features SHOULD be developed following TDD principles. A minimum of 80% code coverage MUST be maintained across all projects. Unit tests for `App.Core` MUST be exhaustive and framework-independent. Integration tests in `App.Api.Tests` MUST verify end-to-end flows using `WebApplicationFactory`.

### IV. Observability-First Development
Every significant operation MUST include structured logging using Serilog. Tracing and metrics MUST be integrated via .NET Aspire and OpenTelemetry. All errors MUST be captured with correlation IDs and follow RFC 7807 Problem Details for API responses.

### V. API Consistency & Versioning
All external APIs MUST be versioned (starting at `v1`). Data transfer between layers MUST use dedicated DTOs; domain entities MUST NOT be exposed directly via APIs. All requests MUST be validated using FluentValidation before reaching the core services.

## Technology Stack & Constraints

- **Runtime**: .NET 10 (latest stable)
- **Orchestration**: .NET Aspire 13.1.1 (SDK pattern)
- **Persistence**: Entity Framework Core with SQLite (Dev) / SQL Server (Prod)
- **Validation**: FluentValidation
- **Observability**: Serilog, Datadog, OpenTelemetry
- **Security**: Keycloak (OAuth2/OIDC), Security Headers, Rate Limiting
- **Formatting**: CSharpier (Mandatory)

## Development Workflow

1. **Core Definition**: Define Domain Entities and Port Interfaces in `App.Core`.
2. **Business Logic**: Implement Services in `App.Core` with corresponding unit tests.
3. **Infrastructure**: Implement Secondary Adapters in `App.Data` (Persistence) or `App.Gateway` (External Services).
4. **API Layer**: Implement Primary Adapters in `App.Api` using Minimal APIs and DTOs.
5. **Validation**: Add FluentValidation rules and integration tests.
6. **Formatting**: Run `csharpier format .` before every commit.

## Governance

- This constitution is the source of truth for all architectural decisions.
- Any deviation MUST be documented and approved via Pull Request.
- Versioning follows Semantic Versioning (SemVer).
- `CONTRIBUTING.md` and `README.md` MUST be kept in sync with these principles.
- Use \`docs/ASPIRE_QUICKSTART.md\` for runtime development guidance.

**Version**: 1.0.0 | **Ratified**: 2026-03-06 | **Last Amended**: 2026-03-06
