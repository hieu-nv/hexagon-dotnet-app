# Changelog

All notable changes to this project will be documented in this file.

## [1.1.0] - 2026-03-01

### Added
- **Docker Optimization**: Multi-stage build support with non-root user and health checks.
- **Architecture Diagrams**: Hexagonal and C4 Container diagrams (Mermaid).
- **ServiceDefaults Tests**: Comprehensive coverage for OpenTelemetry and Health Checks.
- **Data Tests**: Unit tests for `TodoRepository` and `AppDbContext`.
- **API Documentation**: Detailed `API.md` and Swagger enhancements.

### Changed
- **Code Coverage**: Increased aggregate solution-wide coverage to **93.3%**.
- **Security**: Added security headers, rate limiting, and CORS support.
- **Refactoring**: Aligned Pokemon logic with hexagonal patterns (Service Layer + DTOs).

## [1.0.0] - 2026-01-15

### Added
- Initial implementation of Hexagonal Architecture in ASP.NET Core.
- Todo Management (Internal Persistence).
- Pokemon API integration (External Gateway).
- Datadog Observability and .NET Aspire integration.
