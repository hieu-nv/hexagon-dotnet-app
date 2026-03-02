# Feature Specification: Keycloak SSO Support

**Feature Branch**: `001-keycloak-sso`  
**Created**: February 27, 2026  
**Status**: Completed  
**Input**: User description: "Keycloak Single Sign-On (SSO) support with OAuth2/JwtBearer authentication and authorization"

## User Scenarios & Testing _(mandatory)_

### User Story 1 - Administrator Sets Up Keycloak Identity Provider (Priority: P1)

As a DevOps engineer, I want to set up Keycloak as the centralized identity provider for the Hexagon application so that enterprise users can authenticate using a single identity managed in Keycloak.

**Why this priority**: Keycloak setup is the foundation upon which all authentication and SSO functionality depends. Without a working Keycloak instance, no users can authenticate.

**Independent Test**: An administrator can run a setup script that automatically configures Keycloak with a realm, OAuth2 client, test users, and roles. After setup, accessing the Keycloak admin console confirms successful configuration.

**Acceptance Scenarios**:

1. **Given** Keycloak container is running, **When** the setup script runs, **Then** a realm named "hexagon" is created
2. **Given** the setup script completes, **When** admin logs into Keycloak console, **Then** they can see the "hexagon" realm with a configured OAuth2 client
3. **Given** the setup completes, **When** an admin checks the realm's users, **Then** test users (admin@example.com, user@example.com) exist with correct roles

---

### User Story 2 - Developer Configures OAuth2 Authentication in Application (Priority: P1)

As a developer, I want the application to authenticate users via OAuth2/JwtBearer from Keycloak so that I can leverage centralized authentication without managing passwords in the application.

**Why this priority**: This is the critical integration layer. Without OAuth2 configuration, authentication doesn't work.

**Independent Test**: A developer can configure OAuth2 settings in appsettings.json, enable the feature, and the application starts with JwtBearer middleware active. The application can be tested with the setup Keycloak instance.

**Acceptance Scenarios**:

1. **Given** OAuth2 is disabled in configuration, **When** a request comes to a protected endpoint, **Then** the endpoint is accessible without authentication
2. **Given** OAuth2 is enabled and Keycloak is running, **When** an unauthenticated user accesses the application, **Then** they receive a 401 Unauthorized response
3. **Given** a user obtains a valid access token from Keycloak, **When** they present this token in the Authorization header to a protected endpoint, **Then** the user is authenticated and the request is processed

---

### User Story 3 - Application Extracts and Uses User Claims from OAuth2 Access Tokens (Priority: P1)

As a developer, I want the application to extract user identity information (email, name, roles) from OAuth2 Access Tokens so that I can authorize users based on their roles and personalize their experience.

**Why this priority**: Without claims extraction, authenticated users lack identity information needed for authorization and functionality.

**Independent Test**: After OAuth2 authentication succeeds, the AuthService can extract an AuthenticatedUser object from the ClaimsPrincipal. Unit tests verify claims extraction with mocked OAuth2 principals.

**Acceptance Scenarios**:

1. **Given** a user is authenticated via OAuth2, **When** the application calls AuthService.GetAuthenticatedUser(), **Then** an AuthenticatedUser object is returned with email, name, and roles
2. **Given** an access token has role claims, **When** claims are extracted, **Then** the AuthenticatedUser.Roles collection contains all role values
3. **Given** an access token lacks required claims, **When** validation occurs, **Then** the principal is marked invalid and extraction returns null

---

### User Story 4 - Developers Define and Apply Authorization Policies (Priority: P2)

As a developer, I want to define authorization policies (e.g., "AdminOnly", "TodoAccess") based on user roles so that only authorized users can access certain endpoints.

**Why this priority**: Authorization policies enable fine-grained access control but can be added incrementally after basic authentication works.

**Independent Test**: Authorization policies can be created and tested independently using unit tests with mock AuthenticatedUser objects. Developers can apply policies to endpoints without a running Keycloak instance (using test data).

**Acceptance Scenarios**:

1. **Given** an "AdminOnly" policy requires "admin" role, **When** a user with "admin" role accesses a protected endpoint, **Then** access is granted
2. **Given** an "AdminOnly" policy requires "admin" role, **When** a user with only "user" role accesses a protected endpoint, **Then** access is denied with 403 Forbidden
3. **Given** a custom policy allows "admin" OR "moderator" roles, **When** a user with "moderator" role accesses the endpoint, **Then** access is granted

---

### User Story 5 - Developer Protects API Endpoints with OAuth2 Authentication (Priority: P2)

As an API developer, I want to easily protect endpoints with OAuth2 authentication and authorization attributes so that only authenticated and authorized users can access sensitive functionality.

**Why this priority**: Endpoint protection is important but can be implemented incrementally as endpoints are built.

**Independent Test**: A developer can add RequireAuthorization() to minimal API endpoints and verify that unauthenticated requests return 401 and unauthorized requests return 403.

**Acceptance Scenarios**:

1. **Given** an endpoint has [Authorize] attribute, **When** an unauthenticated request arrives, **Then** the endpoint returns 401 Unauthorized
2. **Given** an endpoint requires "AdminOnly" policy, **When** an authenticated non-admin user accesses it, **Then** the endpoint returns 403 Forbidden
3. **Given** an endpoint requires "TodoAccess" policy, **When** an authenticated user with "user" or "admin" role accesses it, **Then** the endpoint processes normally

---

### User Story 6 - Users Log Out and Session Ends (Priority: P3)

As a user, I want to be able to log out from the application so that my session ends and I'm no longer authenticated.

**Why this priority**: Logout is important for security but can be implemented after basic authentication works.

**Independent Test**: A logout endpoint can be tested to confirm it clears the OAuth2 session and requires re-authentication.

**Acceptance Scenarios**:

1. **Given** a user is authenticated, **When** they access the logout endpoint, **Then** their session is cleared
2. **Given** a user has logged out, **When** they try to access a protected endpoint, **Then** they receive a 401 Unauthorized response

---

### Edge Cases

- What happens when Keycloak is unavailable during authentication? The OAuth2 middleware should handle timeouts gracefully.
- What happens if an access token is missing required claims? The application should reject the token and return 401 Unauthorized.
- What happens if a user's roles change in Keycloak? The user will get updated roles only after requesting a new access token.
- What happens when OAuth2 is disabled in configuration? All endpoints should be accessible without authentication.

---

## Requirements _(mandatory)_

### Functional Requirements

- **FR-001**: System MUST support OAuth2/JwtBearer authentication from Keycloak as the identity provider
- **FR-002**: System MUST extract user identity claims (email, name, ID) from OAuth2 access tokens
- **FR-003**: System MUST extract and track user roles from OAuth2 access tokens for authorization
- **FR-004**: System MUST allow enabling/disabling OAuth2 authentication via configuration setting (`JwtBearer:Enabled`)
- **FR-005**: System MUST support role-based authorization policies (AdminOnly, TodoAccess, etc.)
- **FR-006**: System MUST extract custom claims from OAuth2 access tokens for extensibility
- **FR-007**: System MUST provide a port interface (IClaimsExtractor) for pluggable claims extraction
- **FR-008**: System MUST persist Keycloak realm configuration in code/scripts for reproducibility
- **FR-009**: System MUST provide automated Keycloak setup via script (creates realm, client, test users)
- **FR-010**: System MUST validate access tokens before processing (email, subject claim required)

### Functional Constraints

- OAuth2 authentication must validate JWT Bearer tokens issued by Keycloak
- Claims extraction must handle access tokens from Keycloak OAuth2 protocol
- Authorization policies must integrate with ASP.NET Core's standard authorization pipeline

### Non-Functional Requirements

- **NFR-001**: Access token validation must complete in <100ms
- **NFR-002**: Keycloak setup script must complete realm creation in <30 seconds
- **NFR-003**: Claims extraction must support up to 50 roles per user
- **NFR-004**: System must remain backward compatible (OAuth2 can be disabled)

---

## Key Entities _(include if feature involves data)_

### AuthenticatedUser (Value Object)

Represents an authenticated user extracted from OAuth2 access tokens

- **Id** (string): Unique identifier from access token subject claim
- **Email** (string): User's email from access token
- **Name** (string?): User's display name (optional)
- **Roles** (IReadOnlyList<string>): List of roles assigned to user
- **CustomClaims** (IReadOnlyDictionary<string, string>): Additional claims from Keycloak

### AuthenticationPolicy (Value Object)

Represents an authorization policy based on required roles

- **Name** (string): Policy identifier
- **Description** (string?): Human-readable description
- **RequiredRoles** (IReadOnlyList<string>): Roles that satisfy the policy

### IClaimsExtractor (Port Interface)

Defines the contract for extracting claims from OAuth2 access tokens

- **ExtractFromPrincipal(ClaimsPrincipal)**: Returns AuthenticatedUser or null
- **IsValidPrincipal(ClaimsPrincipal)**: Validates required claims exist

### AuthService (Domain Service)

Orchestrates authentication and authorization logic

- **GetAuthenticatedUser(ClaimsPrincipal)**: Extracts user from OAuth2 principal
- **IsAuthenticated(ClaimsPrincipal)**: Checks if user is authenticated
- **AuthorizeUser(user, policy)**: Checks if user satisfies policy
- **AuthorizeByRoles(user, requiredRoles)**: Role-based authorization

---

## Success Criteria

### Quantitative Metrics

- **SC-001**: 100% of authentication tests pass (40+ unit tests for auth module)
- **SC-002**: All 84 application tests pass with OAuth2 integration
- **SC-003**: Zero compilation errors with nullable reference types enabled
- **SC-004**: Keycloak setup script completes successfully in <30 seconds
- **SC-005**: OAuth2 authentication validation completes in <2 seconds (latency)

### Qualitative Metrics

- **SC-006**: Developers can enable/disable OAuth2 with single configuration property
- **SC-007**: Keycloak can be set up with a single script command (`./scripts/keycloak-setup.sh`)
- **SC-008**: Authorization policies are easy to define and apply to endpoints
- **SC-009**: Claims extraction handles both standard and custom OAuth2 access token claims
- **SC-010**: Documentation covers setup, configuration, and usage patterns

### Feature Completeness

- **SC-011**: JwtBearer middleware configured and integrated into ASP.NET Core pipeline
- **SC-012**: Keycloak containerization with podman-compose for local development
- **SC-013**: Automated realm, client, and user setup script
- **SC-014**: Authorization policy system for role-based access control
- **SC-015**: Comprehensive specification and setup documentation

---

## Assumptions

1. **Keycloak Availability**: Keycloak is deployed and accessible at the configured URL
2. **JwtBearer Configuration**: Keycloak OpenID Connect metadata is publicly accessible (for discovery)
3. **User Provisioning**: Users and roles are managed in Keycloak (not in the application database)
4. **Session Management**: JWT Bearer tokens are used for authentication
5. **Development Environment**: Development uses self-signed certificates (SSL validation can be disabled)
6. **Token Refresh**: The client application (not this API) is responsible for managing token refresh
7. **Attribute Mapping**: Access token claims from Keycloak follow its default JWT structure (e.g., realm_access for roles)
8. **Network Access**: Application can reach Keycloak over HTTP(S)

---

## Dependencies & External Integrations

### External Systems

- **Keycloak 22+**: Identity provider and OAuth2/OpenID Connect AuthServer
- **Keycloak REST Admin API**: For automated realm and user setup

### NuGet Dependencies

- **Microsoft.AspNetCore.Authentication.JwtBearer** (latest): JwtBearer authentication middleware for ASP.NET Core
- **System.IdentityModel.Tokens.Jwt**: JWT access token validation

### Infrastructure

- **Podman**: Container runtime for Keycloak (alternative to Docker)
- **SQLite**: Persistence for Keycloak data (development only)

---

## Acceptance Metrics

| Metric               | Target                          | Status                             |
| -------------------- | ------------------------------- | ---------------------------------- |
| Unit Tests Passing   | 40+                             | ⏳ Pending (OAuth2 implementation) |
| Code Coverage (Auth) | 80%+                            | ⏳ Pending (OAuth2 implementation) |
| Build Status         | Zero errors                     | ⏳ Pending (OAuth2 implementation) |
| Documentation        | Setup guide + Architecture spec | ⏳ Pending (OAuth2 implementation) |
| Keycloak Setup Time  | < 30 seconds                    | ⏳ Pending (OAuth2 implementation) |
| OAuth2 Flow          | Works end-to-end                | ⏳ Pending (OAuth2 implementation) |

---

## Rollout Plan

### Phase 1: Foundation (Current)

- ⏳ OAuth2/JwtBearer basic configuration
- ⏳ Claims extraction from access tokens
- ✅ Keycloak containerization
- ⏳ Unit tests
- ⏳ Documentation

### Phase 2: Integration (Next)

- Protect existing endpoints with OAuth2 authentication
- Define authorization policies for endpoint access
- Add OAuth2 logout functionality (for APIs this might just mean token revocation support, client handles clearing local storage)
- Integrate user identity into application features (e.g., audit logs)

### Phase 3: Enhancement (Future)

- Support for additional identity providers
- Advanced claim mappings and custom attributes
- Access token scope management for API authorization

---

## Notes

This specification documents the OAuth2/JwtBearer authentication framework for the Hexagon .NET application APIs. The feature follows hexagonal (ports & adapters) architecture patterns, making it testable and maintainable. OAuth2 authentication can be toggled via configuration, allowing gradual rollout and backward compatibility.

**Created**: February 27, 2026  
**Implementation Status**: ⏳ Ready for OAuth2 Implementation
