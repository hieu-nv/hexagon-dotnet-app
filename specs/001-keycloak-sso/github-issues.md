# GitHub Issues: Keycloak SSO Implementation

This document maps all 71 tasks to GitHub issues for tracking implementation progress.

**Create these issues in order by phase to maintain dependencies.**

---

## Epic Issue

### Epic: Keycloak SSO Support - OAuth2/OpenID Connect Implementation

**Title**: Epic: Keycloak SSO Support - OAuth2/OpenID Connect Implementation  
**Labels**: `feature`, `authentication`, `keycloak`, `oauth2`, `epic`

**Body**:

```markdown
# Keycloak SSO OAuth2 Implementation Epic

Complete implementation of OAuth2/OpenID Connect authentication and authorization
framework for the Hexagon .NET application using Keycloak as the identity provider.

## Scope

- **Total Tasks**: 71 across 9 phases
- **User Stories**: 6 (P1: 3, P2: 2, P3: 1)
- **MVP Target**: User Stories 1-3
- **Time Estimate**: 4 weeks

## Architecture

- Hexagonal (ports & adapters) pattern
- .NET 10 with ASP.NET Core Minimal APIs
- Ports: IClaimsExtractor, IAuthorizationPolicyProvider
- Configuration-driven OAuth2

## Success Criteria

- 100% auth tests pass (40+ tests)
- All 84 app tests pass
- Zero compilation errors
- Keycloak setup: <30 seconds
- OAuth2 flow: <2 seconds
- Coverage: 80%+ auth modules

## Child Issues

- #PHASE1 [Phase 1] Setup & Project Configuration
- #PHASE2 [Phase 2] Foundational - Port Interfaces & DI
- #PHASE3 [Phase 3] US1 - Keycloak Admin Setup (MVP)
- #PHASE4 [Phase 4] US2 - OAuth2 Configuration (MVP)
- #PHASE5 [Phase 5] US3 - Claims Extraction (MVP)
- #PHASE6 [Phase 6] US4 - Authorization Policies
- #PHASE7 [Phase 7] US5 - Endpoint Protection
- #PHASE8 [Phase 8] US6 - Logout Functionality
- #PHASE9 [Phase 9] Polish & Documentation

**Branch**: `001-keycloak-sso`  
**Spec**: specs/001-keycloak-sso/spec.md  
**Tasks**: specs/001-keycloak-sso/tasks.md
```

---

## Phase Issues

### Phase 1: Setup & Project Configuration

**Title**: [P1] Setup & Project Configuration (5 tasks)  
**Labels**: `phase-1`, `setup`, `oauth2`, `infrastructure`

**Body**:

```markdown
## Phase 1: Setup & Project Configuration

Initialize OAuth2/OpenID Connect infrastructure, project structure, and NuGet dependencies.

### Tasks

- [ ] T001 Add NuGet dependencies to `src/App.Api/App.Api.csproj`
  - Microsoft.AspNetCore.Authentication.OpenIdConnect
  - System.IdentityModel.Tokens.Jwt
- [ ] T002 Create authentication configuration in `src/App.Core/Authentication/`
- [ ] T003 Create authorization configuration in `src/App.Core/Authorization/`
- [ ] T004 Update `.github/copilot-instructions.md` with OAuth2 patterns
- [ ] T005 Create Keycloak config directory at `scripts/keycloak/`

### Checkpoint

✓ Project structure initialized  
✓ NuGet dependencies added  
✓ Ready for Phase 2

### Dependencies

None (first phase)

### Enables

Phase 2: Foundational

### Time Estimate

1-2 hours
```

---

### Phase 2: Foundational - Port Interfaces & DI

**Title**: [P2] Foundational - Port Interfaces & DI (9 tasks) ⚠️ BLOCKING  
**Labels**: `phase-2`, `foundational`, `blocking`, `authentication`, `authorization`

**Body**:

```markdown
## Phase 2: Foundational - Port Interfaces & DI

Establish port interfaces and foundational value objects.
**⚠️ CRITICAL: Must complete before any user story begins.**

### Port Interfaces

- [ ] T006 [P] IClaimsExtractor in `src/App.Core/Authentication/Ports/`
  - ExtractFromPrincipal(ClaimsPrincipal): AuthenticatedUser?
  - IsValidPrincipal(ClaimsPrincipal): bool

- [ ] T007 [P] IAuthorizationPolicyProvider in `src/App.Core/Authorization/Ports/`
  - GetPolicy(string): AuthenticationPolicy?
  - AuthorizeUser(AuthenticatedUser, string): bool
  - AuthorizeByRoles(AuthenticatedUser, IReadOnlyList<string>): bool

### Value Objects

- [ ] T008 [P] AuthenticatedUser in `src/App.Core/Authentication/`
  - Properties: Id, Email, Name, Roles, CustomClaims

- [ ] T009 [P] AuthenticationPolicy in `src/App.Core/Authorization/`
  - Properties: Name, Description, RequiredRoles

- [ ] T010 [P] OpenIdConnectConfiguration in `src/App.Core/Authentication/`
  - Properties: Enabled, Authority, ClientId, ClientSecret, Scopes, RequiredClaims

### Domain Services

- [ ] T011 AuthService in `src/App.Core/Authentication/`
  - Depends on IClaimsExtractor
  - Methods: GetAuthenticatedUser, IsAuthenticated
  - XML documentation required

- [ ] T012 AuthorizationService in `src/App.Core/Authorization/`
  - Depends on IAuthorizationPolicyProvider
  - Methods: AuthorizeUser, AuthorizeByRoles
  - XML documentation required

### Extension Methods

- [ ] T013 AuthenticationExtensions.cs in `src/App.Core/Authentication/`
  - UseAppAuthentication (builder)
  - UseAppAuthentication (app)

- [ ] T014 AuthorizationExtensions.cs in `src/App.Core/Authorization/`
  - UseAppAuthorization (builder)

### Checkpoint

✓ All port interfaces defined  
✓ Value objects created  
✓ Domain services implemented  
✓ DI wiring complete  
✓ Ready for parallel user story work

### Dependencies

- Depends on: Phase 1

### Enables

- Phase 3: US1 (Keycloak Setup)
- Phase 4: US2 (OAuth2 Config)
- Phase 5: US3 (Claims Extraction)
- Phase 6: US4 (Authorization Policies)
- Phase 7: US5 (Endpoint Protection)
- Phase 8: US6 (Logout)

### Time Estimate

2-3 days
```

---

### Phase 3: User Story 1 - Keycloak Admin Setup (MVP)

**Title**: [P3] US1 - Keycloak Admin Setup (7 tasks) 🚀 MVP  
**Labels**: `phase-3`, `user-story-1`, `p1-priority`, `keycloak`, `mvp`

**Body**:

```markdown
## Phase 3: User Story 1 - Administrator Sets Up Keycloak Identity Provider

**Priority**: P1 (MVP)  
**Goal**: Automate Keycloak realm, OAuth2 client, test users, and roles.

### Independent Test

Administrator can run setup script and verify realm, client, users in Keycloak console.

### Tasks

#### Tests

- [ ] T015 [P] KeycloakSetupTests.cs in `test/App.Core.Tests/Authentication/`
  - Keycloak health check returns 200
  - Setup script creates "hexagon" realm
  - OAuth2 client "hexagon-app" configured
  - Test users exist with correct roles

#### Implementation

- [ ] T016 [P] realm-export.json in `scripts/keycloak/`
  - Realm name: "hexagon"
  - Event logs enabled
  - Password policy: 8 chars, uppercase, number

- [ ] T017 [P] client-config.json in `scripts/keycloak/`
  - Client ID: "hexagon-app"
  - Redirect URI: http://localhost:5112/signin-oidc
  - Authorization Code + PKCE
  - RS256 signing

- [ ] T018 [P] users-roles.json in `scripts/keycloak/`
  - admin@example.com: [admin, user]
  - user@example.com: [user]

- [ ] T019 keycloak-setup.sh in `scripts/`
  - Health check Keycloak running
  - Create realm/client/users via Admin API
  - <30 seconds completion (NFR-002)
  - Error handling & rollback

- [ ] T020 podman-compose.yml in `scripts/keycloak/`
  - Keycloak 22+ service
  - PostgreSQL 15+ backend
  - Volume persistence
  - Health check configured

- [ ] T021 KEYCLOAK_SETUP.md in `docs/`
  - Prerequisites
  - Quick start: podman-compose + setup.sh
  - Configuration reference
  - Troubleshooting
  - Test credentials

### Success Criteria

- Setup script <30 seconds
- Admin verifies realm in console
- All 3 test users configured correctly
- Integration tests pass

### Checkpoint

✓ Keycloak fully automated  
✓ OAuth2 infrastructure ready  
✓ Ready for US2

### Dependencies

- Depends on: Phase 2

### Enables

- Phase 4: US2 (oauth2 configuration)
- Phase 5: US3 (claims extraction)
- MVP scope complete with these 3 stories

### Time Estimate

2-3 days
```

---

### Phase 4: User Story 2 - OAuth2 Configuration (MVP)

**Title**: [P4] US2 - OAuth2 Configuration (8 tasks) 🚀 MVP  
**Labels**: `phase-4`, `user-story-2`, `p1-priority`, `oauth2`, `mvp`

**Body**:

````markdown
## Phase 4: User Story 2 - Developer Configures OAuth2 Authentication

**Priority**: P1 (MVP)  
**Goal**: Enable OpenID Connect middleware, make OAuth2 configurable & toggleable.

### Independent Test

Developer sets OpenIdConnect:Enabled=true in appsettings.json, starts app with
running Keycloak, gets redirected to Keycloak login for protected endpoints.
Disabling makes all endpoints accessible.

### Tasks

#### Tests

- [ ] T022 [P] OpenIdConnectConfigurationTests.cs in `test/App.Core.Tests/Authentication/`
  - Configuration loads from appsettings.json
  - Validates required properties
  - Handles missing config gracefully

- [ ] T023 [P] OpenIdConnectMiddlewareTests.cs in `test/App.Api.Tests/Authentication/`
  - OAuth2 disabled: endpoints accessible without auth
  - OAuth2 enabled: unauthenticated → Keycloak redirect
  - Valid JWT from Keycloak accepted

#### Implementation

- [ ] T024 [P] appsettings.json & appsettings.Development.json in `src/App.Api/`
  ```json
  {
    "OpenIdConnect": {
      "Enabled": true,
      "Authority": "http://localhost:8080/realms/hexagon",
      "ClientId": "hexagon-app",
      "ClientSecret": "your-secret",
      "Scopes": ["openid", "profile", "email"],
      "RequiredClaims": ["sub", "email"]
    }
  }
  ```
````

- [ ] T025 OpenIdConnectConfiguration.cs in `src/App.Api/Configuration/`
  - Bind from IConfiguration
  - Validate required properties at startup
  - Strongly-typed access

- [ ] T026 OpenIdConnectExtensions.cs in `src/App.Api/Authentication/`
  - Register auth schemes: "oidc", cookie
  - Configure OIDC events
  - Handle validation failures

- [ ] T027 Program.cs update in `src/App.Api/`
  - AddAuthentication() with OIDC
  - UseAuthentication() + UseAuthorization()
  - Conditional enable based on config
  - Call UseAppAuthentication()

- [ ] T028 AuthStatusEndpoints.cs in `src/App.Api/Authentication/`
  - GET /auth/status
  - 200 if authenticated (with user info)
  - 401 if not authenticated
  - Include email & roles for debugging

- [ ] T029 Logging in `src/App.Api/Logging/`
  - Log OIDC events (started, token, failed)
  - Correlate with trace IDs
  - No sensitive data

### Success Criteria

- OAuth2 middleware integrated
- Application authenticates via Keycloak
- Configuration is toggleable
- All tests pass
- Swagger documents auth

### Checkpoint

✓ OpenID Connect middleware active  
✓ Configuration-driven  
✓ Ready for US3 (claims extraction)  
✓ Ready for MVP delivery

### Dependencies

- Depends on: Phase 2, Phase 3 (Keycloak running)

### Enables

- Phase 5: US3 (claims extraction)
- Phase 6: US4 (authorization policies)
- **MVP Complete with US1-3**

### Time Estimate

2-3 days

````

---

### Phase 5: User Story 3 - Claims Extraction (MVP)

**Title**: [P5] US3 - Claims Extraction (6 tasks) 🚀 MVP
**Labels**: `phase-5`, `user-story-3`, `p1-priority`, `claims`, `mvp`

**Body**:
```markdown
## Phase 5: User Story 3 - Application Extracts User Claims From ID Tokens

**Priority**: P1 (MVP)
**Goal**: Extract user identity (email, name, roles) from OAuth2 ID tokens → AuthenticatedUser.

### Independent Test
Unit tests verify AuthService.GetAuthenticatedUser() correctly extracts AuthenticatedUser
from mocked ClaimsPrincipal with various claim combinations. No Keycloak required.

### Tasks

#### Tests
- [ ] T030 [P] ClaimsExtractorTests.cs in `test/App.Core.Tests/Authentication/`
  - Extract user with all claims
  - Extract with required only
  - Multiple roles extraction
  - Custom claims preservation
  - Fail: missing required claims
  - Fail: malformed claims

- [ ] T031 [P] AuthServiceTests.cs in `test/App.Core.Tests/Authentication/`
  - GetAuthenticatedUser() returns user (valid principal)
  - GetAuthenticatedUser() returns null (invalid)
  - IsAuthenticated() true/false
  - Custom claims preserved

#### Implementation
- [ ] T032 [P] KeycloakClaimsExtractor.cs in `src/App.Data/Authentication/`
  - Implement IClaimsExtractor
  - Extract: subject, email, name, roles
  - Map Keycloak role claim path
  - Extract custom claims
  - Validate required claims (sub, email)
  - Handle missing/null gracefully

- [ ] T033 OpenIdConnectClaims.cs in `src/App.Core/Authentication/`
  - Claim type constants
  - Keycloak-specific paths
  - Custom claim prefixes
  - Role separator handling

- [ ] T034 AuthServiceExtensions.cs in `src/App.Core/Authentication/`
  - Extension: GetAuthenticatedUser(IServiceProvider, ClaimsPrincipal)
  - Resolve AuthService from DI
  - Convenience for endpoint handlers

- [ ] T035 OpenIdConnectTokenHandler.cs in `src/App.Data/`
  - Validate JWT structure
  - Parse for debugging
  - Log validation results
  - Handle parsing exceptions

- [ ] T036 AuthenticationEventHandler.cs in `src/App.Api/Authentication/`
  - Subscribe to OnTokenValidated
  - Extract AuthenticatedUser
  - Attach to Principal
  - Log extraction

- [ ] T037 AuthInfoEndpoints.cs in `src/App.Api/Authentication/`
  - GET /auth/me returns user info
  - 401 if not authenticated
  - Include email, name, roles, custom claims
  - Demonstrates claims extraction

### Success Criteria
- Claims extraction fully functional
- Authenticated users have identity
- All tests pass
- AuthService ready for authorization

### Checkpoint
✓ User identity information available
✓ Claims extraction working
✓ **MVP DELIVERABLE: Full authentication flow**
✓ Ready for Phase 6 (authorization policies)

### Dependencies
- Depends on: Phase 2, Phase 4 (OAuth2 middleware)

### Enables
- Phase 6: US4 (authorization policies)
- Phase 7: US5 (endpoint protection)
- Phase 8: US6 (logout)

### Time Estimate
1-2 days

### MVP Complete
✅ **User Stories 1-3 (P1) complete → end-to-end OAuth2 authentication working**
````

---

### Phase 6: User Story 4 - Authorization Policies

**Title**: [P6] US4 - Authorization Policies (5 tasks)  
**Labels**: `phase-6`, `user-story-4`, `p2-priority`, `authorization`

**Body**:

```markdown
## Phase 6: User Story 4 - Developers Define Authorization Policies

**Priority**: P2  
**Goal**: Create flexible authorization policy system for role-based access control.

### Independent Test

Unit tests create policies and verify role-based authorization without Keycloak.
Use mock AuthenticatedUser objects.

### Tasks

#### Tests

- [ ] T038 [P] AuthorizationPolicyTests.cs in `test/App.Core.Tests/Authorization/`
  - Single role policy
  - Multiple roles (OR logic)
  - Authorization succeeds/fails
  - Edge cases (empty, null)

- [ ] T039 [P] AuthorizationServiceTests.cs in `test/App.Core.Tests/Authorization/`
  - AuthorizeUser() returns true/false
  - AuthorizeByRoles() with OR logic
  - Multiple user roles
  - Case-insensitive matching

#### Implementation

- [ ] T040 [P] AuthorizationPoliciesRegistry.cs in `src/App.Core/Authorization/`
  - "AdminOnly": admin role
  - "UserAccess": user OR admin
  - "TodoAccess": user OR admin
  - Easy extension
  - Include descriptions

- [ ] T041 DefaultAuthorizationPolicyProvider.cs in `src/App.Data/Authorization/`
  - Implement IAuthorizationPolicyProvider
  - Return policies from registry
  - Support dynamic policies (future)
  - Handle unknown policies

- [ ] T042 AuthorizationService updates in `src/App.Core/Authorization/`
  - AuthorizeUser(user, policy): bool
  - AuthorizeByRoles(user, roles): bool with OR
  - Clear logging
  - Thread-safe

- [ ] T043 AuthorizationHelpers.cs in `src/App.Core/Authorization/`
  - HasRole()
  - HasAnyRole() (OR)
  - HasAllRoles() (AND)
  - Case-insensitive matching

- [ ] T044 AUTHORIZATION_POLICIES.md in `docs/`
  - Explain policy system
  - List built-in policies
  - Custom policy creation
  - Usage examples
  - Edge cases

### Success Criteria

- Policy system functional
- All tests pass
- Ready for endpoint protection

### Checkpoint

✓ Authorization policies designed  
✓ Ready for Phase 7

### Dependencies

- Depends on: Phase 2, Phase 5

### Enables

- Phase 7: US5 (endpoint protection)
- Phase 8: US6 (logout)

### Time Estimate

2 days
```

---

### Phase 7: User Story 5 - Endpoint Protection

**Title**: [P7] US5 - Endpoint Protection (6 tasks)  
**Labels**: `phase-7`, `user-story-5`, `p2-priority`, `endpoints`

**Body**:

```markdown
## Phase 7: User Story 5 - Developer Protects API Endpoints

**Priority**: P2  
**Goal**: Make it easy to protect endpoints with [Authorize] attributes.

### Independent Test

Endpoints with [Authorize] attributes return 401 (unauthenticated) and 403 (unauthorized).
200 for authorized users. Verified via HTTP integration tests.

### Tasks

#### Tests

- [ ] T045 [P] AuthorizedEndpointsTests.cs in `test/App.Api.Tests/Authentication/`
  - Unprotected accessible without auth
  - [Authorize] → 401 unauthenticated
  - [Authorize("AdminOnly")] → 403 non-admin
  - [Authorize("AdminOnly")] → 200 admin
  - [Authorize("TodoAccess")] accessible to user/admin

- [ ] T046 [P] AuthenticationContractTests.cs in `test/App.Api.Tests/Authentication/`
  - Swagger documents [Authorize] requirements
  - 401/403 responses defined
  - Security scheme configured

#### Implementation

- [ ] T047 [P] AuthorizationMiddleware.cs in `src/App.Api/Authentication/`
  - Check endpoint auth requirements
  - 401 for missing/invalid token
  - 403 for insufficient permissions
  - Works with [Authorize]

- [ ] T048 TodoEndpoints.cs updates in `src/App.Api/Todo/`
  - POST /todos: [Authorize("TodoAccess")]
  - DELETE /todos/{id}: [Authorize]
  - PUT /todos/{id}: [Authorize("TodoAccess")]
  - GET /todos: public or auth-only
  - XML documented

- [ ] T049 AdminEndpoints.cs in `src/App.Api/Admin/`
  - GET /admin/stats: [Authorize("AdminOnly")]
  - Return statistics
  - Demonstrates admin-only pattern

- [ ] T050 Program.cs Swagger config update in `src/App.Api/`
  - Register OpenID Connect security scheme
  - Mark [Authorize] endpoints
  - 401/403 in spec
  - Swagger "Authorize" button

- [ ] T051 EndpointAuthorizationExtensions.cs in `src/App.Api/Authentication/`
  - RequireAuthorization(RouteHandlerBuilder, policy)
  - Fluent API alternative to [Authorize]
  - Works with minimal APIs
  - No handler signature changes

- [ ] T052 AuthorizationLogging.cs in `src/App.Api/Authentication/`
  - Log user email + endpoint
  - Log allow/deny reason
  - Correlation IDs
  - Audit trail

### Success Criteria

- Endpoints protected
- Authentication & authorization working
- Swagger documents security
- All tests pass

### Checkpoint

✓ All endpoints protected  
✓ Authorization enforced  
✓ Ready for Phase 8

### Dependencies

- Depends on: Phase 2, Phase 6

### Enables

- Phase 8: US6 (logout)

### Time Estimate

2 days
```

---

### Phase 8: User Story 6 - Logout Functionality

**Title**: [P8] US6 - Logout Functionality (5 tasks)  
**Labels**: `phase-8`, `user-story-6`, `p3-priority`, `logout`

**Body**:

```markdown
## Phase 8: User Story 6 - Users Log Out and Session Ends

**Priority**: P3  
**Goal**: Implement logout that clears OAuth2 session, requires re-authentication.

### Independent Test

Logout clears session. Protected endpoints redirect to Keycloak after logout.
Browser confirms session cleared.

### Tasks

#### Tests

- [ ] T053 [P] LogoutTests.cs in `test/App.Api.Tests/Authentication/`
  - Unauthenticated logout: error/redirect
  - Authenticated logout: clears session
  - After logout: protected → Keycloak
  - Logout returns redirect
  - Double logout: graceful

#### Implementation

- [ ] T054 [P] AuthLogoutEndpoints.cs in `src/App.Api/Authentication/`
  - POST/GET /auth/logout
  - Clear authentication cookie
  - Clear session state
  - Build Keycloak logout redirect
  - Return 200 redirect or 302

- [ ] T055 LogoutService.cs in `src/App.Core/Authentication/`
  - BuildLogoutRedirectUri()
  - Encode redirect parameters
  - Keycloak endpoint: /protocol/openid-connect/logout

- [ ] T056 LogoutExtensions.cs in `src/App.Core/Authentication/`
  - LogoutAsync(HttpContext, LogoutService)
  - SignOutAsync
  - Clear cookies/session
  - For endpoint handlers

- [ ] T057 logout-success.html in `src/App.Api/Static/` (or Razor page)
  - Post-logout confirmation
  - Friendly message
  - Link to login

- [ ] T058 KEYCLOAK_SETUP.md update in `docs/`
  - Logout behavior docs
  - Endpoint usage
  - Redirect URI config
  - Manual testing

### Success Criteria

- Logout functional
- Session completely cleared
- All tests pass
- **All 6 user stories complete**

### Checkpoint

✓ Logout working  
✓ **FEATURE COMPLETE: All 6 user stories done**  
✓ Ready for Phase 9 (documentation & polish)

### Dependencies

- Depends on: Phase 2, Phase 7

### Enables

- Phase 9: Polish & Documentation

### Time Estimate

1 day
```

---

### Phase 9: Polish & Documentation

**Title**: [P9] Polish & Documentation (13 tasks)  
**Labels**: `phase-9`, `documentation`, `polish`, `testing`

**Body**:

```markdown
## Phase 9: Polish & Cross-Cutting Concerns

**Goal**: Documentation, edge case handling, performance validation, feature completeness.

### Documentation Tasks

- [ ] T059 OAUTH2_ARCHITECTURE.md in `docs/`
  - Architecture diagram (ASCII/Mermaid)
  - Component interactions
  - Authentication data flow
  - Token lifecycle
  - Claims extraction pipeline
  - Authorization decision flow

- [ ] T060 OAUTH2_TROUBLESHOOTING.md in `docs/`
  - Common errors & solutions
  - Token validation failures
  - Keycloak connectivity
  - Configuration mistakes
  - Debugging tips

- [ ] T063 OAUTH2_SECURITY.md in `docs/`
  - HTTPS enforcement
  - Client secret management
  - PKCE for mobile/SPA
  - Token storage/expiration
  - CORS for Keycloak
  - Rate limiting

- [ ] T064 OAUTH2_CONFIG_REFERENCE.md in `docs/`
  - All config options
  - Environment variables
  - Prod vs. dev defaults
  - Keycloak realm config

- [ ] T065 OAUTH2_EXAMPLES.md in `docs/`
  - Use authenticated user
  - Authorization policies
  - Extract custom claims
  - Advanced scenarios

- [ ] T070 README.md update in root
  - Add "Authentication" section
  - Keycloak quick start
  - Links to docs
  - Feature status badge

- [ ] T071 GitHub issue templates in `.github/ISSUE_TEMPLATE/`
  - Auth bug report
  - OAuth2 feature request

### Testing Tasks

- [ ] T061 [P] OAuth2IntegrationTests.cs in `test/App.Api.Tests/Authentication/`
  - End-to-end with real Keycloak (Docker)
  - Token refresh (if implemented)
  - Session timeout
  - Token expiration

- [ ] T062 [P] AuthenticationPerformanceTests.cs in `test/App.Core.Tests/Authentication/`
  - Claims extraction: <10ms (NFR-001)
  - Auth policy: <5ms
  - 100 concurrent requests

- [ ] T069 test-coverage.md in `specs/001-keycloak-sso/checklists/`
  - Auth tests: 15+
  - Authorization tests: 10+
  - Integration tests: 8+
  - Edge case tests: 8+
  - Total: 40+ tests
  - Coverage: 80%+

### Edge Cases & Quality

- [ ] T067 [P] Edge case handling in `src/App.Core/Authentication/` & `src/App.Api/Authentication/`
  - Keycloak unavailable
  - Token validation timeout
  - Malformed ID tokens
  - Missing required claims
  - Role changes in Keycloak
  - OAuth2 disabled config

- [ ] T066 Swagger/OpenAPI in `src/App.Api/Program.cs`
  - OIDC security scheme
  - [Authorize] attributes
  - /auth/status documented
  - /auth/logout documented
  - Public vs. auth endpoints

- [ ] T068 Code review against standards
  - XML documentation (csharp-docs skill)
  - No nullable warnings
  - ConfigureAwait(false) (csharp-async skill)
  - No hardcoded values
  - SOLID principles

### Summary & Validation

- [ ] T059-T071 Complete implementations
- [ ] All 71 tasks completed
- [ ] 40+ unit tests passing
- [ ] 80%+ auth coverage
- [ ] Zero compilation errors
- [ ] All documentation reviewed

### Success Criteria

- All 71 tasks complete
- 100% auth tests pass
- All 84 app tests pass
- Zero build errors
- 80%+ coverage auth modules
- Complete documentation
- Ready for production

### Checkpoint

✅ **FEATURE COMPLETE & PRODUCTION-READY**

### Dependencies

- Depends on: Phase 1-8 (all previous phases)

### Time Estimate

3-4 days

### Final Metrics

| Metric         | Target      | Status |
| -------------- | ----------- | ------ |
| Unit Tests     | 40+         | ✓      |
| Coverage       | 80%+        | ✓      |
| Build          | Zero errors | ✓      |
| Keycloak Setup | <30s        | ✓      |
| OAuth2 Flow    | <2s         | ✓      |
| Documentation  | Complete    | ✓      |
```

---

## User Story Issues (Alternative: Create These If Tracking Per-Story)

### US1: Keycloak Setup (Covered in Phase 3)

**Title**: [US1] Keycloak Identity Provider Setup (P1)  
**Labels**: `user-story-1`, `p1-priority`, `keycloak`  
**Related to**: Phase 3 Issue

### US2: OAuth2 Configuration (Covered in Phase 4)

**Title**: [US2] OAuth2 Authentication Configuration (P1)  
**Labels**: `user-story-2`, `p1-priority`, `oauth2`  
**Related to**: Phase 4 Issue

### US3: Claims Extraction (Covered in Phase 5)

**Title**: [US3] Claims Extraction from ID Tokens (P1)  
**Labels**: `user-story-3`, `p1-priority`, `claims`  
**Related to**: Phase 5 Issue

### US4: Authorization Policies (Covered in Phase 6)

**Title**: [US4] Authorization Policies (P2)  
**Labels**: `user-story-4`, `p2-priority`, `authorization`  
**Related to**: Phase 6 Issue

### US5: Endpoint Protection (Covered in Phase 7)

**Title**: [US5] Endpoint Protection (P2)  
**Labels**: `user-story-5`, `p2-priority`, `endpoints`  
**Related to**: Phase 7 Issue

### US6: Logout Functionality (Covered in Phase 8)

**Title**: [US6] Logout Functionality (P3)  
**Labels**: `user-story-6`, `p3-priority`, `logout`  
**Related to**: Phase 8 Issue

---

## Creating Issues

### Option 1: Manual Creation (Recommended for clarity)

1. Create each phase issue manually using the templates above
2. Use labels for tracking:
   - `phase-N` (phase number)
   - `user-story-N` (US1-6)
   - `p1-priority`, `p2-priority`, `p3-priority` (priority)
   - `oauth2`, `keycloak`, `authentication`, `authorization` (domain)
   - `mvp` (for MVP scope: US1-3)
   - `blocking` (for Phase 2)
3. Link issues using issue references (#N)

### Option 2: Scripted Creation (Using gh CLI)

```bash
# Create Epic
gh issue create \
  --title "Epic: Keycloak SSO Support..." \
  --body "..." \
  --label feature,authentication,keycloak,oauth2

# Create Phase 1
gh issue create \
  --title "[P1] Setup & Project Configuration (5 tasks)" \
  --body "..." \
  --label phase-1,setup,oauth2

# ... repeat for each phase
```

### Option 3: GitHub Project Board

1. Create a "Keycloak SSO" project
2. Add columns: Backlog, Phase 1, Phase 2, ..., Phase 9, Done
3. Link phase issues to appropriate columns
4. Add task checklists within each issue

---

## Progress Tracking

Use GitHub's project automation to:

1. **Move issues** between columns as phases progress
2. **Label** completed tasks with `done`
3. **Comment** with blockers or progress updates
4. **Link** related issues for dependencies
5. **Milestone** for release tracking (v1.0 = MVP, v1.1 = full feature)

---

**Created**: February 27, 2026  
**Status**: Ready to create in GitHub  
**All tasks**: Full details in `specs/001-keycloak-sso/tasks.md`
