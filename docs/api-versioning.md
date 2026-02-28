# API Versioning Strategy

This project uses URI versioning to maintain backward compatibility and clearly communicate version changes to clients.

## Current Version
- **v1.0** is the default and current version for all endpoints.

## Implementation Details
The application employs `Asp.Versioning.Http` and `Asp.Versioning.Mvc.ApiExplorer` to correctly enforce and discover API versions.

### Route Format
All API routes are prefixed with `/api/v{version:apiVersion}/`. For example:
- `/api/v1/todos`
- `/api/v1/pokemon`

### Default Settings
- If no version is specified, it assumes `1.0`.
- API versions are reported in the response headers (`api-supported-versions`, `api-deprecated-versions`).
- Swagger will group endpoints by version format (`'v'VVV`).

## Future Upgrades
When adding breaking changes, developers should create new endpoints tied to a new version (e.g., `2.0`), leaving `1.0` endpoints intact to avoid breaking existing clients.
