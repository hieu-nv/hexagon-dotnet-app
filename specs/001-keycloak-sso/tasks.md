---
description: "Task list for Keycloak SSO authentication feature implementation"
---

# Tasks: Keycloak SSO Support

**Feature Branch**: `001-keycloak-sso`  
**Input**: Design document from `/specs/001-keycloak-sso/spec.md`  
**Architecture**: Hexagonal (Ports & Adapters) - .NET 10 with ASP.NET Core Minimal APIs

## Format: `- [ ] [ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no explicit dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4, US5, US6)
- **File paths**: Relative to repository root

## Implementation Strategy

**MVP Scope** (Phase 3): Complete User Stories 1-3 (P1) for end-to-end OAuth2 authentication

- Keycloak setup and containerization
- OpenID Connect middleware configuration
- Claims extraction and AuthService

**Phase 4-5**: Authorization policies and endpoint protection (P2 stories)  
**Phase 6**: Logout functionality (P3 story)

**Parallel Opportunities**:

- US2 & US3 tests can run while infrastructure is being set up
- Port interfaces (IClaimsExtractor) can be developed in parallel with Keycloak configuration
- Authorization policies (US4) can be designed while OAuth2 middleware is being configured

---

## Phase 1: Setup & Project Configuration

**Purpose**: Initialize OAuth2/OpenID Connect infrastructure, project structures, and NuGet dependencies

- [ ] T001 Add NuGet dependencies: `Microsoft.AspNetCore.Authentication.OpenIdConnect` and `System.IdentityModel.Tokens.Jwt` to `src/App.Api/App.Api.csproj`
- [ ] T002 Create authentication configuration structure in `src/App.Core/Authentication/` directory with empty placeholder files
- [ ] T003 Create authorization configuration structure in `src/App.Core/Authorization/` directory with empty placeholder files
- [ ] T004 Update .github/copilot-instructions.md with OAuth2/OpenID Connect patterns and conventions for this feature
- [ ] T005 Create Keycloak configuration directory at `/scripts/keycloak/` for realm and client configurations

---

## Phase 2: Foundational (Port & Adapter Layer)

**Purpose**: Establish port interfaces and foundational value objects that all user stories depend on

⚠️ **CRITICAL**: These must be complete before any user story implementation begins

### Port Interfaces (defines contracts)

- [ ] T006 [P] Create `IClaimsExtractor` port interface in `src/App.Core/Authentication/Ports/IClaimsExtractor.cs` with methods:
  - `ExtractFromPrincipal(ClaimsPrincipal principal): AuthenticatedUser?`
  - `IsValidPrincipal(ClaimsPrincipal principal): bool`

- [ ] T007 [P] Create `IAuthorizationPolicyProvider` port interface in `src/App.Core/Authorization/Ports/IAuthorizationPolicyProvider.cs` with methods:
  - `GetPolicy(string policyName): AuthenticationPolicy?`
  - `AuthorizeUser(AuthenticatedUser user, string policyName): bool`
  - `AuthorizeByRoles(AuthenticatedUser user, IReadOnlyList<string> requiredRoles): bool`

### Value Objects (domain models)

- [ ] T008 [P] Create `AuthenticatedUser` value object in `src/App.Core/Authentication/AuthenticatedUser.cs` with properties: Id, Email, Name, Roles, CustomClaims
- [ ] T009 [P] Create `AuthenticationPolicy` value object in `src/App.Core/Authorization/AuthenticationPolicy.cs` with properties: Name, Description, RequiredRoles
- [ ] T010 [P] Create `OpenIdConnectConfiguration` value object in `src/App.Core/Authentication/OpenIdConnectConfiguration.cs` with properties: Enabled, Authority, ClientId, ClientSecret, Scopes, RequiredClaims

### Domain Services (orchestrates business logic)

- [ ] T011 Create `AuthService` domain service in `src/App.Core/Authentication/AuthService.cs` that:
  - Depends on `IClaimsExtractor` (injected)
  - Provides `GetAuthenticatedUser(ClaimsPrincipal): AuthenticatedUser?`
  - Provides `IsAuthenticated(ClaimsPrincipal): bool`
  - Includes XML documentation per csharp-docs skill

- [ ] T012 Create `AuthorizationService` domain service in `src/App.Core/Authorization/AuthorizationService.cs` that:
  - Depends on `IAuthorizationPolicyProvider` (injected)
  - Provides `AuthorizeUser(AuthenticatedUser, string): bool`
  - Provides `AuthorizeByRoles(AuthenticatedUser, IReadOnlyList<string>): bool`
  - Includes XML documentation

### Extension Methods (wiring)

- [ ] T013 Create `AuthenticationExtensions.cs` in `src/App.Core/Authentication/` with:
  - `UseAppAuthentication(this WebApplicationBuilder)`: Registers `AuthService` and port interfaces in DI
  - `UseAppAuthentication(this WebApplication)`: Maps any authentication-related middleware

- [ ] T014 Create `AuthorizationExtensions.cs` in `src/App.Core/Authorization/` with:
  - `UseAppAuthorization(this WebApplicationBuilder)`: Registers authorization services in DI

**Checkpoint**: Port interfaces, value objects, and services are ready. User story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Administrator Sets Up Keycloak Identity Provider (P1) 🚀

**Goal**: Automate Keycloak realm, OAuth2 client, test users, and roles configuration for reproducible local development environment

**Independent Test**: Administrator can run setup script and access Keycloak admin console to verify realm, client, and test user configurations

### Test Tasks for US1

- [ ] T015 [P] [US1] Create integration test in `test/App.Core.Tests/Authentication/KeycloakSetupTests.cs` verifying:
  - Keycloak container health endpoint returns 200
  - Setup script creates "hexagon" realm successfully
  - OAuth2 client "hexagon-app" is configured and discoverable
  - Test users (admin@example.com, user@example.com) exist with correct roles

### Implementation for US1

- [ ] T016 [P] [US1] Create Keycloak realm configuration file in `scripts/keycloak/realm-export.json` with:
  - Realm name: "hexagon"
  - Event logs enabled
  - SMTP configuration for email (optional)
  - Password policy: minimum 8 characters, requires uppercase and number

- [ ] T017 [P] [US1] Create Keycloak OAuth2 client configuration in `scripts/keycloak/client-config.json` with:
  - Client ID: "hexagon-app"
  - Client Secret: (generated per environment)
  - Redirect URI: `http://localhost:5112/signin-oidc` (dev), configurable for prod
  - Standard Authorization Code flow with PKCE
  - Token endpoint: configured for RS256 signing

- [ ] T018 [P] [US1] Create Keycloak test users setup file in `scripts/keycloak/users-roles.json` with:
  - User "admin@example.com" with roles: ["admin", "user"]
  - User "user@example.com" with roles: ["user"]
  - Both with temporary password set to expire on first login

- [ ] T019 [US1] Create bash setup script `scripts/keycloak-setup.sh` that:
  - Verifies Keycloak container is running (health check)
  - Uses Keycloak REST Admin API to create realm, client, users, and roles
  - Imports configurations from JSON files (realm-export.json, client-config.json, users-roles.json)
  - Validates setup completion and reports success/failure
  - Target completion: <30 seconds (per NFR-002)
  - Includes error handling and rollback mechanism

- [ ] T020 [US1] Create `docker-compose.yml` or `podman-compose.yml` in `scripts/keycloak/` for local Keycloak development with:
  - Keycloak 22+ service
  - PostgreSQL 15+ backend (or SQLite for simpler local dev)
  - Environment variables for realm admin password (injected from .env)
  - Volume mapping for realm configuration persistence
  - Health check endpoint configured

- [ ] T021 [US1] Create README documenting Keycloak setup in `docs/KEYCLOAK_SETUP.md`:
  - Prerequisites (podman or Docker)
  - Quick start: `podman-compose up -d && ./scripts/keycloak-setup.sh`
  - Configuration reference for OAuth2 client settings
  - Troubleshooting common issues
  - Test user credentials for manual testing

**Checkpoint**: Keycloak is fully automated and testable. OAuth2 infrastructure setup complete.

---

## Phase 4: User Story 2 - Developer Configures OAuth2 Authentication in Application (P1)

**Goal**: Enable OpenID Connect middleware in the application, making OAuth2 authentication configurable and toggleable via configuration

**Independent Test**: Developer can set `OpenIdConnect:Enabled=true` in appsettings.json, start the app with Keycloak running, and be redirected to Keycloak login for protected endpoints. Disabling it makes all endpoints accessible.

### Test Tasks for US2

- [ ] T022 [P] [US2] Create unit test in `test/App.Core.Tests/Authentication/OpenIdConnectConfigurationTests.cs` verifying:
  - Configuration loads correctly from appsettings.json
  - Validates required properties (Authority, ClientId, ClientSecret)
  - Handles missing configuration gracefully

- [ ] T023 [P] [US2] Create integration test in `test/App.Api.Tests/Authentication/OpenIdConnectMiddlewareTests.cs` verifying:
  - When OAuth2 disabled: unauthenticated access to any endpoint works
  - When OAuth2 enabled: unauthenticated request to protected endpoint redirects to Keycloak
  - When OAuth2 enabled: valid JWT from Keycloak is accepted by middleware

### Implementation for US2

- [ ] T024 [P] [US2] Add OpenID Connect configuration section to `src/App.Api/appsettings.json` and `appsettings.Development.json`:

  ```json
  {
    "OpenIdConnect": {
      "Enabled": true,
      "Authority": "http://localhost:8080/realms/hexagon",
      "ClientId": "hexagon-app",
      "ClientSecret": "your-secret-here",
      "Scopes": ["openid", "profile", "email"],
      "RequiredClaims": ["sub", "email"]
    }
  }
  ```

- [ ] T025 [US2] Create `OpenIdConnectConfiguration.cs` loader in `src/App.Api/Configuration/` that:
  - Binds from IConfiguration
  - Validates required properties at startup
  - Provides strongly-typed access to OAuth2 settings
  - Implements validation with clear error messages

- [ ] T026 [US2] Implement OpenID Connect middleware registration in `src/App.Api/Authentication/OpenIdConnectExtensions.cs`:
  - Reads configuration from `OpenIdConnectConfiguration`
  - Registers authentication schemes: "oidc" for OpenID Connect, cookie for session
  - Configures OIDC events (OnAuthenticationFailed, OnTokenValidated) for proper error handling
  - Handles token validation failures gracefully
  - Supports configurable ID token validation rules

- [ ] T027 [US2] Update `src/App.Api/Program.cs` to wire OpenID Connect into ASP.NET Core pipeline:
  - Add `builder.Services.AddAuthentication()` with OIDC configuration
  - Add `app.UseAuthentication()` and `app.UseAuthorization()` middleware
  - Conditionally enable based on `OpenIdConnect:Enabled` setting
  - Call `builder.UseAppAuthentication()` extension method

- [ ] T028 [US2] Create test endpoint `GET /auth/status` in `src/App.Api/Authentication/AuthStatusEndpoints.cs` for manual verification:
  - Returns 200 with authenticated status if user is authenticated
  - Returns 401 if user is not authenticated
  - Includes user email and roles in response (for debugging)
  - Documentation visible in Swagger

- [ ] T029 [US2] Add comprehensive logging for OAuth2 events to `src/App.Api/Logging/`:
  - Log OIDC middleware events (auth started, token received, validation failed)
  - Correlate logs with trace IDs for debugging
  - Log configuration validation results
  - No sensitive data (secrets) logged

**Checkpoint**: OAuth2 middleware is integrated. Application can authenticate users via Keycloak. Configuration is toggleable.

---

## Phase 5: User Story 3 - Application Extracts and Uses User Claims from OAuth2 ID Tokens (P1)

**Goal**: Extract authenticated user identity (email, name, roles, custom claims) from OAuth2 ID tokens into domain-facing `AuthenticatedUser` value objects

**Independent Test**: Unit tests verify `AuthService.GetAuthenticatedUser()` correctly extracts AuthenticatedUser from mocked ClaimsPrincipal with various claim combinations. No Keycloak required for these tests.

### Test Tasks for US3

- [ ] T030 [P] [US3] Create comprehensive unit test in `test/App.Core.Tests/Authentication/ClaimsExtractorTests.cs` covering:
  - Extract valid user with all claims (email, name, roles)
  - Extract user with only required claims (email, subject)
  - Extract user with multiple roles
  - Extract user with custom claims beyond standard OpenID Connect
  - Validation fails when required claims missing (email, subject)
  - Validation fails when claims are malformed (empty, null)
  - Role extraction: single role, multiple roles, no roles

- [ ] T031 [P] [US3] Create unit test in `test/App.Core.Tests/Authentication/AuthServiceTests.cs` verifying:
  - AuthService.GetAuthenticatedUser() returns AuthenticatedUser for valid principal
  - AuthService.GetAuthenticatedUser() returns null for invalid principal
  - AuthService.IsAuthenticated() returns true/false correctly
  - Custom claims are preserved in AuthenticatedUser object

### Implementation for US3

- [ ] T032 [P] [US3] Implement `KeycloakClaimsExtractor` adapter in `src/App.Data/Authentication/KeycloakClaimsExtractor.cs`:
  - Implements `IClaimsExtractor` port interface
  - Extracts standard OpenID Connect claims: subject, email, name, roles
  - Maps Keycloak role claim path (configurable, default: "realm_access.roles")
  - Extracts custom claims not in standard OpenID Connect spec
  - Validates required claims (subject, email) before returning AuthenticatedUser
  - Handles missing/null/empty claim values gracefully

- [ ] T033 [US3] Create constants file `src/App.Core/Authentication/OpenIdConnectClaims.cs` defining:
  - Standard claim type constants (subject, email, name, jti, iat, exp)
  - Keycloak-specific claim paths (realm roles, client roles)
  - Custom claim prefixes for extensibility
  - Role claim separator (space or comma)

- [ ] T034 [US3] Create `AuthServiceExtensions.cs` in `src/App.Core/Authentication/` that:
  - Extension method `GetAuthenticatedUser(this IServiceProvider, ClaimsPrincipal): AuthenticatedUser?`
  - Resolves `AuthService` from DI and calls extract method
  - Provides convenience method for use in endpoint handlers

- [ ] T035 [US3] Update `EntityFrameworkCore.OpenIdConnectTokenHandler.cs` (new file) to:
  - Validate ID token structure (header, payload, signature)
  - Parse JWT without validation first (for debugging failed tokens)
  - Log token validation results at appropriate levels
  - Handle JWT parsing exceptions (malformed tokens)

- [ ] T036 [US3] Create `AuthenticationEventHandler.cs` in `src/App.Api/Authentication/` that:
  - Subscribes to `OpenIdConnectEvents.OnTokenValidated`
  - Extracts `AuthenticatedUser` and attaches to `context.Principal`
  - Sets custom claim principal for later retrieval in endpoints
  - Logs token validation and extraction results

- [ ] T037 [US3] Create sample endpoint handler `src/App.Api/Authentication/AuthInfoEndpoints.cs`:
  - `GET /auth/me` returns current authenticated user info (logged in via OAuth2)
  - Returns 401 if not authenticated
  - Includes email, name, roles, custom claims
  - Demonstrates claims extraction in action

**Checkpoint**: Claims extraction fully functional. Authenticated users have identity information. AuthService ready for authorization use.

---

## Phase 6: User Story 4 - Developers Define and Apply Authorization Policies (P2)

**Goal**: Create a flexible authorization policy system allowing role-based access control without modifying ASP.NET Core's authorization pipeline

**Independent Test**: Unit tests create policies and verify role-based authorization logic without Keycloak. Policies work with mock AuthenticatedUser objects.

### Test Tasks for US4

- [ ] T038 [P] [US4] Create unit test in `test/App.Core.Tests/Authorization/AuthorizationPolicyTests.cs` verifying:
  - Policy creation with single required role
  - Policy creation with multiple required roles (OR logic)
  - Authorization succeeds when user has required role
  - Authorization fails when user lacks required role
  - Authorization with no roles (any authenticated user allowed)
  - Authorization policy edge cases (empty role list, null user)

- [ ] T039 [P] [US4] Create unit test in `test/App.Core.Tests/Authorization/AuthorizationServiceTests.cs` verifying:
  - AuthorizeUser() returns true/false for specific policy
  - AuthorizeByRoles() returns true/false for role list (OR logic)
  - Authorization handles multiple roles per user
  - Authorization with case-insensitive role matching (if applicable)

### Implementation for US4

- [ ] T040 [P] [US4] Create `AuthorizationPoliciesRegistry.cs` in `src/App.Core/Authorization/` defining standard policies:
  - "AdminOnly": requires "admin" role
  - "UserAccess": requires "user" or "admin" role
  - "TodoAccess": requires "user" or "admin" role (for Todo domain)
  - Easy to extend with new policies
  - Includes policy descriptions for documentation

- [ ] T041 [US4] Implement `DefaultAuthorizationPolicyProvider` adapter in `src/App.Data/Authorization/DefaultAuthorizationPolicyProvider.cs`:
  - Implements `IAuthorizationPolicyProvider` port interface
  - Returns predefined policies from `AuthorizationPoliciesRegistry`
  - Supports dynamic policy creation (future enhancement)
  - Handles unknown policy names gracefully

- [ ] T042 [US4] Update `AuthorizationService` in `src/App.Core/Authorization/AuthorizationService.cs`:
  - Implement `AuthorizeUser(user, policyName): bool` using `IAuthorizationPolicyProvider`
  - Implement `AuthorizeByRoles(user, requiredRoles): bool` with OR logic (user has ANY required role)
  - Clear authorization decision logging
  - Thread-safe for concurrent requests

- [ ] T043 [US4] Create authorization helpers in `src/App.Core/Authorization/AuthorizationHelpers.cs`:
  - `HasRole(AuthenticatedUser, role): bool`
  - `HasAnyRole(AuthenticatedUser, roles): bool` (OR logic)
  - `HasAllRoles(AuthenticatedUser, roles): bool` (AND logic)
  - Case-insensitive role matching utility

- [ ] T044 [US4] Create documentation in `docs/AUTHORIZATION_POLICIES.md`:
  - Explain policy system and how it maps to roles
  - List built-in policies and their requirements
  - Show how to create custom policies
  - Examples of policy usage in endpoints
  - Edge cases and limitations

**Checkpoint**: Authorization policy system is functional and testable. Policies can be applied to endpoints in next story.

---

## Phase 7: User Story 5 - Developer Protects API Endpoints with OAuth2 Authentication (P2)

**Goal**: Make it easy to protect endpoints with authentication and authorization using simple attributes or middleware

**Independent Test**: Endpoints with [Authorize] attributes return 401 for unauthenticated users, 403 for unauthorized users, and 200 for authorized users. Verified through HTTP integration tests.

### Test Tasks for US5

- [ ] T045 [P] [US5] Create integration test in `test/App.Api.Tests/Authentication/AuthorizedEndpointsTests.cs` verifying:
  - Endpoint without [Authorize] is accessible without authentication
  - Endpoint with [Authorize] returns 401 unauthenticated
  - Endpoint with [Authorize("AdminOnly")] returns 403 for non-admin user
  - Endpoint with [Authorize("AdminOnly")] returns 200 for admin user
  - Endpoint with [Authorize("TodoAccess")] accessible to both user and admin roles

- [ ] T046 [P] [US5] Create contract test in `test/App.Api.Tests/Authentication/AuthenticationContractTests.cs` verifying:
  - Swagger/OpenAPI correctly documents [Authorize] requirements
  - 401 response defined for unauthenticated
  - 403 response defined for unauthorized

### Implementation for US5

- [ ] T047 [P] [US5] Create authorization middleware helper in `src/App.Api/Authentication/AuthorizationMiddleware.cs`:
  - Middleware that checks if endpoint requires authentication
  - Returns 401 for missing/invalid token
  - Returns 403 for insufficient permissions
  - Works with ASP.NET Core's built-in [Authorize] attribute

- [ ] T048 [US5] Update Todo endpoints in `src/App.Api/Todo/TodoEndpoints.cs`:
  - Protect POST /todos with [Authorize("TodoAccess")]
  - Protect DELETE /todos/{id} with [Authorize]
  - Protect PUT /todos/{id} with [Authorize("TodoAccess")]
  - GET /todos visible to all (or authenticated only, per requirements)
  - Document which endpoints require authentication in XML comments

- [ ] T049 [US5] Create example protected endpoint `src/App.Api/Admin/AdminEndpoints.cs`:
  - `GET /admin/stats` with [Authorize("AdminOnly")]
  - Returns system statistics (only for admins)
  - Demonstrates admin-only endpoint pattern

- [ ] T050 [US5] Update Swagger/OpenAPI configuration in `src/App.Api/Program.cs`:
  - Register OpenID Connect security scheme in Swagger
  - Mark [Authorize] endpoints as requiring authentication
  - Include 401/403 responses in generated OpenAPI spec
  - Provide "Authorize" button in Swagger UI for manual testing

- [ ] T051 [US5] Create authorization extension helper in `src/App.Api/Authentication/EndpointAuthorizationExtensions.cs`:
  - Extension method `RequireAuthorization(this RouteHandlerBuilder, string? policy)` (fluent API)
  - Alternative to [Authorize] attribute for minimal APIs
  - Enables authorization without changing handler signature
  - Works with TodoEndpoints, AdminEndpoints, etc.

- [ ] T052 [US5] Add authorization decision logging to all protected endpoints in `src/App.Api/Authentication/AuthorizationLogging.cs`:
  - Log which user (email) accessed which endpoint
  - Log authorization decision (allowed/denied reason)
  - Include correlation IDs for audit trails
  - Respect log levels (info for access, warning for denial)

**Checkpoint**: Endpoints are protected with OAuth2 authentication and role-based authorization. Users can only access endpoints they're authorized for.

---

## Phase 8: User Story 6 - Users Log Out and Session Ends (P3)

**Goal**: Implement logout endpoint that clears OAuth2 session and cookies, requiring re-authentication

**Independent Test**: Logout endpoint clears session. Accessing protected endpoint after logout redirects to Keycloak login. Manual testing via browser confirms session entirely cleared.

### Test Tasks for US6

- [ ] T053 [P] [US6] Create integration test in `test/App.Api.Tests/Authentication/LogoutTests.cs` verifying:
  - Unauthenticated request to logout endpoint returns error/redirect
  - Authenticated user calling logout endpoint clears their session
  - After logout, accessing protected endpoint redirects to Keycloak
  - Logout returns proper redirect to Keycloak logout endpoint
  - Double logout (already logged out) handled gracefully

### Implementation for US6

- [ ] T054 [P] [US6] Create logout endpoint handler in `src/App.Api/Authentication/AuthLogoutEndpoints.cs`:
  - `POST /auth/logout` (or `GET /auth/logout`)
  - Clears authentication cookie
  - Clears session state
  - Builds redirect URL to Keycloak logout endpoint with post-logout redirect URI
  - Returns 200 with redirect URL or 302 redirect to Keycloak

- [ ] T055 [US6] Implement logout service `src/App.Core/Authentication/LogoutService.cs`:
  - Method `BuildLogoutRedirectUri(string authority, string? postLogoutRedirectUri): Uri`
  - Properly encodes redirect parameters
  - Handles Keycloak-specific logout endpoint (/protocol/openid-connect/logout)

- [ ] T056 [US6] Create logout extension in `src/App.Core/Authentication/LogoutExtensions.cs`:
  - Extension method `LogoutAsync(this HttpContext, LogoutService)` for convenience
  - Calls SignOutAsync on HttpContext.AuthenticationManager
  - Clears cookies and session
  - Can be used in endpoint handlers

- [ ] T057 [US6] Create post-logout redirect page `src/App.Api/Static/logout-success.html` (or Razor page):
  - Shown after logout completes
  - Friendly message confirming logout
  - Link to login page for reconvening authentication

- [ ] T058 [US6] Update documentation in `docs/KEYCLOAK_SETUP.md` with logout flow:
  - Explain logout behavior
  - Document logout endpoint usage
  - Explain post-logout redirect URI configuration
  - Browser-based testing steps

**Checkpoint**: User logout is fully functional. Session is completely cleared. All SSO features are now complete.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, error handling edge cases, performance validation, and feature completeness

- [ ] T059 Create comprehensive OAuth2/OpenID Connect architecture guide in `docs/OAUTH2_ARCHITECTURE.md`:
  - High-level architecture diagram (ASCII or Mermaid)
  - Component interactions (app, Keycloak, middleware, services)
  - Data flow for authentication request
  - Token lifecycle and refresh
  - Claims extraction pipeline
  - Authorization decision flow

- [ ] T060 Create OAuth2 troubleshooting guide in `docs/OAUTH2_TROUBLESHOOTING.md`:
  - Common errors and solutions
  - Token validation failures (expired, invalid signature, missing claims)
  - Keycloak connectivity issues
  - Configuration mistakes
  - Debugging tips (logging, token inspection)

- [ ] T061 [P] Create OAuth2 integration tests in `test/App.Api.Tests/Authentication/OAuth2IntegrationTests.cs`:
  - End-to-end authentication flow with real Keycloak (Docker container)
  - Token refresh flow (if implemented)
  - Session timeout and re-authentication
  - Token expiration handling

- [ ] T062 [P] Create performance tests in `test/App.Core.Tests/Authentication/AuthenticationPerformanceTests.cs`:
  - Claims extraction: <10ms per operation (per NFR-001)
  - Authorization policy check: <5ms per operation
  - Batch operations (100 simultaneous requests) under load

- [ ] T063 Create OAuth2 security checklist in `docs/OAUTH2_SECURITY.md`:
  - HTTPS enforcement (redirect URI validation)
  - Client secret management best practices
  - PKCE implementation for mobile/SPA
  - Token storage and expiration
  - CORS configuration for Keycloak
  - Rate limiting on authentication endpoint (edge case)

- [ ] T064 Create OAuth2 configuration reference in `docs/OAUTH2_CONFIG_REFERENCE.md`:
  - All configuration options in appsettings.json
  - Environment variable mapping
  - Production vs. development defaults
  - Keycloak realm vs. client configuration

- [ ] T065 Add OAuth2 code examples in `docs/OAUTH2_EXAMPLES.md`:
  - Use authenticated user in endpoint handlers
  - Define and use authorization policies
  - Extract custom claims
  - Manual configuration for advanced scenarios

- [ ] T066 Create OpenAPI/Swagger documentation updates:
  - Security scheme configuration for OIDC in OpenAPI spec
  - [Authorize] attributes propagated to generated spec
  - `/auth/status` endpoint documented
  - `/auth/logout` endpoint documented
  - All endpoints clearly marked as public or authenticated

- [ ] T067 [P] Handle edge cases in `src/App.Core/Authentication/` and `src/App.Api/Authentication/`:
  - Keycloak unavailable during token validation (graceful degradation)
  - Token validation timeout (configurable timeout with sensible default)
  - Malformed ID tokens (detailed error messages)
  - Missing required claims in valid JWT (clear error messages)
  - User roles change in Keycloak (requires re-login, documented)
  - OAuth2 disabled in configuration (all endpoints accessible)

- [ ] T068 Review code against project standards:
  - All public APIs have XML documentation (csharp-docs skill)
  - Nullable reference types: no warnings (`#nullable enable`)
  - Async methods use `ConfigureAwait(false)` (csharp-async skill)
  - No hardcoded values (configuration-driven)
  - SOLID principles followed (single responsibility, DI, etc.)

- [ ] T069 Create unit test summary in `specs/001-keycloak-sso/checklists/test-coverage.md`:
  - Authentication tests: 15+ tests
  - Authorization tests: 10+ tests
  - Integration tests: 8+ tests
  - Edge case tests: 8+ tests
  - **Total target**: 40+ tests per SC-001
  - Coverage target: 80%+ for auth modules (SC-002)

- [ ] T070 Update project README in `README.md` with OAuth2:
  - Add "Authentication" section
  - Quick start for Keycloak setup
  - Links to documentation
  - Feature status badge (OAuth2 implemented)

- [ ] T071 Create GitHub issue templates for OAuth2 bugs/features in `.github/ISSUE_TEMPLATE/`:
  - Bug report template for authentication issues
  - Feature request template for OAuth2 enhancements

---

## Summary & Success Criteria

| Metric                    | Target      | Final Status              |
| ------------------------- | ----------- | ------------------------- |
| **User Stories Complete** | 6/6         | ⏳ Pending Implementation |
| **Tasks Total**           | 71          | ⏳ Pending Implementation |
| **Unit Tests**            | 40+         | ⏳ Pending Implementation |
| **Code Coverage (Auth)**  | 80%+        | ⏳ Pending Implementation |
| **Build Status**          | Zero errors | ⏳ Pending Implementation |
| **Keycloak Setup Time**   | <30 sec     | ⏳ Pending Implementation |
| **Token Validation**      | <100ms      | ⏳ Pending Implementation |
| **Documentation Pages**   | 7+          | ⏳ Pending Implementation |

### Task Breakdown by User Story

| Story          | ID                      | Count                   | Phase   | Priority    |
| -------------- | ----------------------- | ----------------------- | ------- | ----------- |
| **US1**        | Keycloak Setup          | 7                       | Phase 3 | P1 🚀       |
| **US2**        | OAuth2 Configuration    | 8                       | Phase 4 | P1          |
| **US3**        | Claims Extraction       | 6                       | Phase 5 | P1          |
| **US4**        | Authorization Policies  | 5                       | Phase 6 | P2          |
| **US5**        | Endpoint Protection     | 6                       | Phase 7 | P2          |
| **US6**        | Logout                  | 5                       | Phase 8 | P3          |
| **Foundation** | Port Interfaces & DI    | 9                       | Phase 2 | ⚠️ BLOCKING |
| **Setup**      | Project Configuration   | 5                       | Phase 1 | 🔧 INIT     |
| **Polish**     | Documentation & Quality | 13                      | Phase 9 | 📚 FINAL    |
|                |                         | **64** base tasks       |         |             |
|                |                         | **7** additional polish |         |             |
|                |                         | **71** total            |         |             |

### Parallel Execution Example

**Week 1** (Foundation & Keycloak):

- Parallel: T001-T005 (Setup)
- Parallel: T006-T014 (Port interfaces & services)
- Sequential: T015-T021 (Keycloak setup & testing)

**Week 2** (OAuth2 Integration):

- Parallel: T022-T023 (US2 tests), T030-T031 (US3 tests), T038-T039 (US4 tests), T045-T046 (US5 tests)
- Sequential: T024-T037 (OAuth2 & Claims implementation)

**Week 3** (Authorization & Endpoint Protection):

- Parallel: T040-T044 (Policy system), T047-T052 (Endpoint protection)
- Sequential with previous week's completion

**Week 4** (Logout & Polish):

- Sequential: T053-T058 (Logout implementation)
- Parallel: T059-T071 (Documentation & testing)

---

## Implementation Notes

### Architecture Decisions

1. **Port-First Design**: IClaimsExtractor and IAuthorizationPolicyProvider are ports, allowing multiple implementations for different OAuth2 providers
2. **Configuration-Driven**: OAuth2 can be enabled/disabled via config without code changes
3. **Separation of Concerns**: Authentication (identity) and Authorization (permissions) are separate concerns in separate services
4. **Keycloak-Agnostic Core**: App.Core has no Keycloak dependencies; adapters in App.Data handle Keycloak specifics

### Testing Strategy

- **Unit Tests** (TDD Red-Green-Refactor): AuthService, AuthorizationService, policy logic - no Keycloak needed
- **Integration Tests**: Full OAuth2 flow with Keycloak running in Docker
- **Contract Tests**: OpenAPI/Swagger verification of endpoint contracts
- **Performance Tests**: Token validation, authorization checks under load

### MVP Scope

**Minimum Viable Product = User Stories 1-3 (P1)**

- Keycloak setup automated ✓
- OAuth2 middleware integrated ✓
- Claims extraction functional ✓
- Allows shipping authenticated API by end of week 2

**Phase 2 (Week 3-4)**: Authorization policies + logout = production-ready

---

## Dependencies & File Structure

```
src/
├── App.Core/
│   ├── Authentication/
│   │   ├── Ports/IClaimsExtractor.cs
│   │   ├── AuthenticatedUser.cs
│   │   ├── AuthService.cs
│   │   ├── AuthenticationExtensions.cs
│   │   └── ...
│   ├── Authorization/
│   │   ├── Ports/IAuthorizationPolicyProvider.cs
│   │   ├── AuthenticationPolicy.cs
│   │   ├── AuthorizationService.cs
│   │   └── ...
│   └── ...
├── App.Data/
│   ├── Authentication/
│   │   ├── KeycloakClaimsExtractor.cs
│   │   └── ...
│   ├── Authorization/
│   │   ├── DefaultAuthorizationPolicyProvider.cs
│   │   └── ...
│   └── ...
├── App.Api/
│   ├── Authentication/
│   │   ├── AuthStatusEndpoints.cs
│   │   ├── AuthLogoutEndpoints.cs
│   │   ├── OpenIdConnectExtensions.cs
│   │   ├── AuthorizationMiddleware.cs
│   │   └── ...
│   ├── Admin/
│   │   ├── AdminEndpoints.cs
│   │   └── ...
│   ├── Todo/
│   │   └── TodoEndpoints.cs (updated)
│   ├── appsettings.json (updated)
│   └── Program.cs (updated)
└── ...

test/
├── App.Core.Tests/
│   ├── Authentication/
│   │   ├── ClaimsExtractorTests.cs
│   │   ├── AuthServiceTests.cs
│   │   ├── OpenIdConnectConfigurationTests.cs
│   │   ├── AuthenticationPerformanceTests.cs
│   │   └── ...
│   └── Authorization/
│       ├── AuthorizationPolicyTests.cs
│       ├── AuthorizationServiceTests.cs
│       └── ...
├── App.Api.Tests/
│   └── Authentication/
│       ├── OpenIdConnectMiddlewareTests.cs
│       ├── AuthorizedEndpointsTests.cs
│       ├── AuthenticationContractTests.cs
│       ├── LogoutTests.cs
│       ├── OAuth2IntegrationTests.cs
│       └── ...
└── ...

docs/
├── KEYCLOAK_SETUP.md
├── OAUTH2_ARCHITECTURE.md
├── OAUTH2_TROUBLESHOOTING.md
├── OAUTH2_SECURITY.md
├── OAUTH2_CONFIG_REFERENCE.md
└── OAUTH2_EXAMPLES.md

scripts/
└── keycloak/
    ├── realm-export.json
    ├── client-config.json
    ├── users-roles.json
    ├── docker-compose.yml (or podman-compose.yml)
    └── keycloak-setup.sh
```

---

**Created**: February 27, 2026  
**Last Updated**: February 27, 2026  
**Status**: Ready for Implementation
