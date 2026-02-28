#!/bin/bash
set -e

echo "🚀 Finding existing Phase issues..."
echo ""

# Find issue numbers for each phase
PHASE1=$(gh issue list --label phase-1 --json number --jq '.[0].number' 2>/dev/null || echo "")
PHASE2=$(gh issue list --label phase-2 --json number --jq '.[0].number' 2>/dev/null || echo "")
PHASE3=$(gh issue list --label phase-3 --json number --jq '.[0].number' 2>/dev/null || echo "")
PHASE4=$(gh issue list --label phase-4 --json number --jq '.[0].number' 2>/dev/null || echo "")
PHASE5=$(gh issue list --label phase-5 --json number --jq '.[0].number' 2>/dev/null || echo "")
PHASE6=$(gh issue list --label phase-6 --json number --jq '.[0].number' 2>/dev/null || echo "")
PHASE7=$(gh issue list --label phase-7 --json number --jq '.[0].number' 2>/dev/null || echo "")
PHASE8=$(gh issue list --label phase-8 --json number --jq '.[0].number' 2>/dev/null || echo "")
PHASE9=$(gh issue list --label phase-9 --json number --jq '.[0].number' 2>/dev/null || echo "")

echo "Found issues:"
echo "  Phase 1: #$PHASE1"
echo "  Phase 2: #$PHASE2"
echo "  Phase 3: #$PHASE3"
echo "  Phase 4: #$PHASE4"
echo "  Phase 5: #$PHASE5"
echo "  Phase 6: #$PHASE6"
echo "  Phase 7: #$PHASE7"
echo "  Phase 8: #$PHASE8"
echo "  Phase 9: #$PHASE9"
echo ""

# Update Phase 2
if [ -n "$PHASE2" ]; then
  echo "Updating Phase 2 (#$PHASE2)..."
  gh issue edit $PHASE2 --body "Establish port interfaces and foundational value objects that all user stories depend on.

⚠️ **CRITICAL**: Phase 2 MUST complete before any user story implementation begins. These are the contracts and domain models for the entire OAuth2 authentication system.

## Port Interfaces (defines contracts for adapters)
- [ ] T006 [P] Create IClaimsExtractor port interface in src/App.Core/Authentication/Ports/
  - ExtractFromPrincipal(ClaimsPrincipal): AuthenticatedUser?
  - IsValidPrincipal(ClaimsPrincipal): bool
- [ ] T007 [P] Create IAuthorizationPolicyProvider port interface in src/App.Core/Authorization/Ports/
  - GetPolicy(policyName): AuthenticationPolicy?
  - AuthorizeUser(user, policyName): bool
  - AuthorizeByRoles(user, requiredRoles): bool

## Value Objects (domain models)
- [ ] T008 [P] Create AuthenticatedUser value object (Id, Email, Name, Roles, CustomClaims)
- [ ] T009 [P] Create AuthenticationPolicy value object (Name, Description, RequiredRoles)
- [ ] T010 [P] Create OpenIdConnectConfiguration value object (Enabled, Authority, ClientId, ClientSecret, Scopes, RequiredClaims)

## Domain Services (business logic)
- [ ] T011 Create AuthService with dependency on IClaimsExtractor (GetAuthenticatedUser, IsAuthenticated)
- [ ] T012 Create AuthorizationService with dependency on IAuthorizationPolicyProvider (AuthorizeUser, AuthorizeByRoles)

## Extension Methods (DI wiring)
- [ ] T013 Create AuthenticationExtensions.cs with UseAppAuthentication() registration
- [ ] T014 Create AuthorizationExtensions.cs with UseAppAuthorization() registration

## Success Criteria
✓ All port interfaces defined with complete method signatures
✓ Value objects created with proper properties per spec
✓ Domain services have XML documentation
✓ Extension methods wire services into DI container
✓ Nullable reference types enabled, zero warnings

## Checkpoint
✅ Foundation is complete! User stories 1-6 can now be implemented in parallel (Phases 3-8)

**Enables**: All subsequent user stories (Phases 3-8)
**Depends On**: Phase 1"
fi

# Update Phase 3
if [ -n "$PHASE3" ]; then
  echo "Updating Phase 3 (#$PHASE3)..."
  gh issue edit $PHASE3 --body "Automate Keycloak infrastructure: realm, OAuth2 client, test users, and roles configuration. First user story in MVP scope.

## Acceptance Criteria
- Administrator runs setup script that creates Keycloak realm 'hexagon'
- Script auto-configures OAuth2 client 'hexagon-app' with proper redirect URIs
- Test users (admin@example.com, user@example.com) created with correct roles
- Admin can verify all configuration in Keycloak console within 30 seconds

## Tasks
- [ ] T015 [P] Create KeycloakSetupTests.cs integration test verifying:
  - Keycloak health endpoint responds
  - Realm 'hexagon' created successfully
  - Client 'hexagon-app' discoverable via .well-known endpoint
  - Test users exist with assigned roles
- [ ] T016 [P] Create realm-export.json with realm config (name: hexagon, SMTP optional, password policy: 8+ chars)
- [ ] T017 [P] Create client-config.json OAuth2 client (hexagon-app, Auth Code + PKCE, redirects: http://localhost:5112/signin-oidc)
- [ ] T018 [P] Create users-roles.json test users (admin@example.com [admin,user], user@example.com [user])
- [ ] T019 Create keycloak-setup.sh script to:
  - Verify Keycloak running via health check
  - Use Admin API to import realm, client, users, roles from JSON files
  - Validate completion in <30 seconds per NFR-002
  - Include error handling and rollback
- [ ] T020 Create podman-compose.yml in scripts/keycloak/ with:
  - Keycloak 22+ service
  - PostgreSQL 15+ backend (or SQLite for simpler setup)
  - Health check configured
  - Volume mapping for persistence
- [ ] T021 Create KEYCLOAK_SETUP.md documentation (prerequisites, quick start, troubleshooting)

## Success Criteria
✓ Setup script <30 seconds (NFR-002)
✓ Admin verifies realm, client, users in Keycloak console
✓ Integration tests pass with live Keycloak
✓ Setup is reproducible and idempotent

✅ **MVP Checkpoint 1 of 3**: Keycloak infrastructure ready for OAuth2 integration

**Enables**: Phase 4 (OAuth2 configuration in app) and Phase 5 (Claims extraction)
**Depends On**: Phase 1, Phase 2"
fi

# Update Phase 4
if [ -n "$PHASE4" ]; then
  echo "Updating Phase 4 (#$PHASE4)..."
  gh issue edit $PHASE4 --body "Enable OpenID Connect middleware in application. Developer configures OAuth2 in settings and app redirects to Keycloak login. Second user story in MVP scope.

## Acceptance Criteria
- When OAuth2 disabled: all endpoints accessible without authentication
- When OAuth2 enabled: unauthenticated users redirected to Keycloak login
- When OAuth2 enabled: valid Keycloak JWT accepted by middleware, user authenticated
- Middleware can be toggled on/off via OpenIdConnect:Enabled setting

## Tasks
- [ ] T022 [P] Create OpenIdConnectConfigurationTests.cs unit test:
  - Configuration loads from appsettings.json
  - Validates required properties (Authority, ClientId, ClientSecret)
  - Handles missing configuration gracefully
- [ ] T023 [P] Create OpenIdConnectMiddlewareTests.cs integration test:
  - OAuth2 disabled = unrestricted access to protected endpoints
  - OAuth2 enabled = unauthenticated redirects to Keycloak
  - Valid JWT from Keycloak accepted by middleware
- [ ] T024 [P] Add OpenIdConnect config to appsettings.json and appsettings.Development.json:
  - Enabled: true/false
  - Authority: http://localhost:8080/realms/hexagon (Keycloak URL)
  - ClientId: hexagon-app
  - ClientSecret: (per environment)
  - Scopes: [openid, profile, email]
  - RequiredClaims: [sub, email]
- [ ] T025 Create OpenIdConnectConfiguration.cs loader in src/App.Api/Configuration/:
  - Binds from IConfiguration
  - Validates required properties with clear error messages
  - Provides strongly-typed access to OAuth2 settings
- [ ] T026 Implement OpenIdConnectExtensions.cs middleware registration:
  - Registers 'oidc' authentication scheme and cookie handler
  - Configures OIDC events (OnAuthenticationFailed, OnTokenValidated)
  - Handles token validation failures gracefully
  - Supports configurable ID token validation rules
- [ ] T027 Update Program.cs to wire OpenID Connect:
  - Add builder.Services.AddAuthentication() with OIDC config
  - Add app.UseAuthentication() and app.UseAuthorization()
  - Conditionally enable based on OpenIdConnect:Enabled setting
  - Call builder.UseAppAuthentication() extension
- [ ] T028 Create GET /auth/status test endpoint for manual verification:
  - Returns 200 with authenticated status if user is authenticated
  - Returns 401 if user is not authenticated
  - Includes email, roles in response (for debugging)
  - Documented in Swagger
- [ ] T029 Add comprehensive OAuth2 event logging to src/App.Api/Logging/:
  - Log OIDC middleware events (auth started, token received, validation failed)
  - Correlate logs with trace IDs for debugging
  - Do NOT log secrets or sensitive data

## Success Criteria
✓ OAuth2 middleware integrated and toggleable
✓ Configuration validated at startup
✓ Keycloak redirect working for unauthenticated users
✓ Swagger documents authentication
✓ All integration tests pass

✅ **MVP Checkpoint 2 of 3**: OAuth2 middleware integrated, app can authenticate users via Keycloak

**Enables**: Phase 5 (Claims extraction) to complete MVP
**Depends On**: Phase 1, Phase 2, Phase 3"
fi

# Update Phase 5
if [ -n "$PHASE5" ]; then
  echo "Updating Phase 5 (#$PHASE5)..."
  gh issue edit $PHASE5 --body "Extract user identity from OAuth2 ID tokens into AuthenticatedUser objects. Third user story in MVP scope - completes end-to-end OAuth2 authentication.

## Acceptance Criteria
- AuthService.GetAuthenticatedUser() extracts user identity from OAuth2 ClaimsPrincipal
- Extracted AuthenticatedUser includes email, name, roles from ID token
- Missing required claims (email, subject) are detected and rejected
- Custom claims beyond standard OpenID Connect are preserved
- Unit tests verify all scenarios WITHOUT requiring Keycloak running

## Tasks
- [ ] T030 [P] Create ClaimsExtractorTests.cs comprehensive unit tests:
  - Extract valid user with all claims (email, name, roles)
  - Extract user with only required claims (email, subject)
  - Extract user with multiple roles and custom claims
  - Validation fails when required claims missing
  - Validation fails when claims are malformed (empty, null)
- [ ] T031 [P] Create AuthServiceTests.cs unit tests:
  - GetAuthenticatedUser() returns AuthenticatedUser for valid principal
  - GetAuthenticatedUser() returns null for invalid principal
  - IsAuthenticated() returns true/false correctly
  - Custom claims are preserved in AuthenticatedUser
- [ ] T032 [P] Implement KeycloakClaimsExtractor adapter (IClaimsExtractor):
  - Extract standard OpenID Connect claims: subject, email, name
  - Map Keycloak role claim path (configurable, default: realm_access.roles)
  - Extract custom claims beyond standard OIDC
  - Validate required claims (subject, email) before returning AuthenticatedUser
  - Handle missing/null/empty claim values gracefully
- [ ] T033 Create OpenIdConnectClaims.cs constants file defining:
  - Standard claim type constants (subject, email, name, jti, iat, exp)
  - Keycloak-specific claim paths (realm roles, client roles)
  - Custom claim prefixes for extensibility
  - Role claim separator configuration
- [ ] T034 Create AuthServiceExtensions.cs convenience methods:
  - Extension method: GetAuthenticatedUser(this IServiceProvider, ClaimsPrincipal): AuthenticatedUser?
  - Resolves AuthService from DI for use in endpoint handlers
- [ ] T035 Create OpenIdConnectTokenHandler.cs for JWT validation:
  - Validate ID token structure (header, payload, signature)
  - Parse JWT without validation for debugging failed tokens
  - Log token validation results at appropriate levels
  - Handle JWT parsing exceptions (malformed tokens)
- [ ] T036 Create AuthenticationEventHandler.cs subscribing to OpenIdConnectEvents:
  - OnTokenValidated: Extract AuthenticatedUser and attach to context.Principal
  - Set custom claim principal for later retrieval in endpoints
  - Log token validation and extraction results
- [ ] T037 Create GET /auth/me endpoint in AuthInfoEndpoints.cs:
  - Returns authenticated user info (email, name, roles, custom claims)
  - Returns 401 if not authenticated
  - Demonstrates claims extraction in action

## Success Criteria
✓ Claims extraction fully functional (unit tests all pass)
✓ Authenticated users have complete identity information
✓ AuthService integrated into OIDC event pipeline
✓ /auth/me endpoint returns user details correctly
✓ All 40+ unit tests for authentication module pass
✓ 80%+ code coverage for auth modules

✅ **MVP COMPLETE**: End-to-end OAuth2 authentication fully working!
- User Stories 1-3 (P1) are complete
- Keycloak setup automated
- OAuth2 middleware integrated
- User identity extracted and available to application

## What's Next?
✓ Authorization policies (US4 - Phase 6)
✓ Endpoint protection (US5 - Phase 7)
✓ Logout functionality (US6 - Phase 8)
✓ Polish & documentation (Phase 9)

**Enables**: MVP is complete! Phases 6-9 are independent enhancements
**Depends On**: Phase 1, Phase 2, Phase 3, Phase 4"
fi

# Update Phase 6
if [ -n "$PHASE6" ]; then
  echo "Updating Phase 6 (#$PHASE6)..."
  gh issue edit $PHASE6 --body "Create flexible authorization policy system for role-based access control. No Keycloak required for these tests - uses mock AuthenticatedUser objects.

## Acceptance Criteria
- Developers can define authorization policies (e.g., 'AdminOnly', 'TodoAccess')
- Policies based on user roles (single role, multiple roles with OR logic)
- Authorization logic tested with mocked AuthenticatedUser objects
- Policies can be created and tested without running Keycloak instance

## Tasks
- [ ] T038 [P] Create AuthorizationPolicyTests.cs unit tests:
  - Policy creation with single required role
  - Policy creation with multiple required roles (OR logic)
  - Authorization succeeds when user has required role
  - Authorization fails when user lacks required role
  - Authorization with no roles (any authenticated user allowed)
  - Edge cases (empty role list, null user)
- [ ] T039 [P] Create AuthorizationServiceTests.cs unit tests:
  - AuthorizeUser() returns true/false for specific policy
  - AuthorizeByRoles() returns true/false for role list (OR logic)
  - Multiple roles per user handled correctly
  - Case-insensitive role matching (if applicable)
- [ ] T040 [P] Create AuthorizationPoliciesRegistry.cs defining standard policies:
  - 'AdminOnly': requires 'admin' role
  - 'UserAccess': requires 'user' or 'admin' role
  - 'TodoAccess': requires 'user' or 'admin' role
  - Easy to extend with new policies
  - Includes policy descriptions for documentation
- [ ] T041 Implement DefaultAuthorizationPolicyProvider adapter (IAuthorizationPolicyProvider):
  - Returns predefined policies from AuthorizationPoliciesRegistry
  - Supports dynamic policy creation (future enhancement)
  - Handles unknown policy names gracefully
- [ ] T042 Update AuthorizationService implementation:
  - AuthorizeUser(user, policyName): bool using IAuthorizationPolicyProvider
  - AuthorizeByRoles(user, requiredRoles): bool with OR logic (user has ANY required role)
  - Clear authorization decision logging
  - Thread-safe for concurrent requests
- [ ] T043 Create AuthorizationHelpers.cs utility methods:
  - HasRole(AuthenticatedUser, role): bool
  - HasAnyRole(AuthenticatedUser, roles): bool (OR logic)
  - HasAllRoles(AuthenticatedUser, roles): bool (AND logic)
  - Case-insensitive role matching utility
- [ ] T044 Create AUTHORIZATION_POLICIES.md documentation:
  - Explain policy system and how it maps to roles
  - List built-in policies and their requirements
  - Show how to create custom policies
  - Examples of policy usage in endpoints
  - Edge cases and limitations

## Success Criteria
✓ Policy system fully testable with mocked users
✓ Built-in policies for common scenarios (Admin, User, Todo Access)
✓ All unit tests pass (10+ authorization tests)
✓ Ready for endpoint protection (Phase 7)

**Priority**: P2 (after MVP delivered in Phase 5)
**Parallel to**: Can run while Phase 7 endpoint protection is being implemented

**Depends On**: Phase 2 (AuthenticationPolicy value object)"
fi

# Update Phase 7
if [ -n "$PHASE7" ]; then
  echo "Updating Phase 7 (#$PHASE7)..."
  gh issue edit $PHASE7 --body "Protect API endpoints with OAuth2 authentication and authorization. Endpoints return 401 (unauthenticated) and 403 (unauthorized) when appropriate.

## Acceptance Criteria
- Endpoints without [Authorize] attribute are accessible without authentication
- Endpoints with [Authorize] return 401 for unauthenticated users
- Endpoints with [Authorize(\"policy\")] return 403 for unauthorized users
- Protected endpoints return 200 for authorized users
- Swagger/OpenAPI properly documents authentication requirements

## Tasks
- [ ] T045 [P] Create AuthorizedEndpointsTests.cs integration tests:
  - Endpoint without [Authorize] accessible without authentication
  - Endpoint with [Authorize] returns 401 unauthenticated
  - Endpoint with [Authorize(\"AdminOnly\")] returns 403 for non-admin
  - Endpoint with [Authorize(\"AdminOnly\")] returns 200 for admin
  - Endpoint with [Authorize(\"TodoAccess\")] accessible to user and admin roles
- [ ] T046 [P] Create AuthenticationContractTests.cs tests:
  - Swagger/OpenAPI documents [Authorize] requirements
  - 401 response defined for unauthenticated
  - 403 response defined for unauthorized
  - Security scheme configured in OpenAPI spec
- [ ] T047 [P] Create AuthorizationMiddleware.cs helper:
  - Middleware that checks if endpoint requires authentication
  - Returns 401 for missing/invalid token
  - Returns 403 for insufficient permissions
  - Works with ASP.NET Core's built-in [Authorize] attribute
- [ ] T048 Update TodoEndpoints.cs with authorization:
  - Protect POST /todos with [Authorize(\"TodoAccess\")]
  - Protect DELETE /todos/{id} with [Authorize]
  - Protect PUT /todos/{id} with [Authorize(\"TodoAccess\")]
  - GET /todos accessible to all or authenticated only (per requirements)
  - Document which endpoints require authentication in XML comments
- [ ] T049 Create AdminEndpoints.cs example protected endpoint:
  - GET /admin/stats with [Authorize(\"AdminOnly\")]
  - Returns system statistics (only for admins)
  - Demonstrates admin-only endpoint pattern for other developers
  - Uses AuthorizationHelpers for clear authorization logic
- [ ] T050 Update Swagger/OpenAPI configuration in Program.cs:
  - Register OpenID Connect security scheme in Swagger
  - Mark [Authorize] endpoints as requiring authentication in generated spec
  - Include 401/403 responses in generated OpenAPI spec
  - Provide 'Authorize' button in Swagger UI for interactive testing
- [ ] T051 Create EndpointAuthorizationExtensions.cs fluent API:
  - Extension method: RequireAuthorization(this RouteHandlerBuilder, string? policy)
  - Alternative to [Authorize] attribute for minimal APIs
  - Enable authorization without changing handler signature
  - Works with TodoEndpoints, AdminEndpoints, etc.
- [ ] T052 Add AuthorizationLogging.cs audit logging:
  - Log which user (email) accessed which endpoint
  - Log authorization decision (allowed/denied with reason)
  - Include correlation IDs for audit trails
  - Respect log levels (info for access, warning for denial)

## Success Criteria
✓ All endpoints properly protected with [Authorize] attributes
✓ Authentication & authorization enforced (401, 403 status codes)
✓ Swagger documents security requirements
✓ Audit logging tracks all authorization decisions
✓ All integration tests pass (6+ endpoint protection tests)

**Priority**: P2 (after MVP delivered)
**Parallel to**: Can run while Phase 6 authorization policies are being finalized

**Depends On**: Phase 1, Phase 2, Phase 3, Phase 4, Phase 5, Phase 6"
fi

# Update Phase 8
if [ -n "$PHASE8" ]; then
  echo "Updating Phase 8 (#$PHASE8)..."
  gh issue edit $PHASE8 --body "Implement logout endpoint that clears OAuth2 session and cookies. User must re-authenticate after logout.

## Acceptance Criteria
- User can access POST/GET /auth/logout endpoint
- Calling logout clears authentication cookie and session
- After logout, accessing protected endpoint redirects to Keycloak
- Logout gracefully handles edge cases (double logout, invalid requests)

## Tasks
- [ ] T053 [P] Create LogoutTests.cs integration test verifying logout behavior
- [ ] T054 [P] Create AuthLogoutEndpoints.cs with POST/GET /auth/logout handlers
- [ ] T055 Implement LogoutService.cs building logout redirect URIs
- [ ] T056 Create LogoutExtensions.cs convenience methods
- [ ] T057 Create logout-success.html post-logout confirmation page
- [ ] T058 Update KEYCLOAK_SETUP.md with logout flow documentation

## Success Criteria
✓ Logout functional and tested
✓ Session completely cleared after logout
✓ All integration tests pass
✓ Logout flow documented

✅ **FEATURE COMPLETE**: All 6 user stories implemented and working!
- US1: Keycloak setup ✓
- US2: OAuth2 configuration ✓
- US3: Claims extraction ✓
- US4: Authorization policies ✓
- US5: Endpoint protection ✓
- US6: Logout ✓

**Priority**: P3 (nice-to-have after MVP)
**Depends On**: Phase 1-5"
fi

# Update Phase 9
if [ -n "$PHASE9" ]; then
  echo "Updating Phase 9 (#$PHASE9)..."
  gh issue edit $PHASE9 --body "Documentation, edge case handling, performance validation, and feature completeness. All user stories implemented - now production-harden and document.

## Documentation (7 tasks)
- [ ] T059 Create OAUTH2_ARCHITECTURE.md with component diagrams and data flows
- [ ] T060 Create OAUTH2_TROUBLESHOOTING.md with common errors and solutions
- [ ] T063 Create OAUTH2_SECURITY.md with hardening checklist (HTTPS, secrets, PKCE, CORS)
- [ ] T064 Create OAUTH2_CONFIG_REFERENCE.md with all configuration options
- [ ] T065 Create OAUTH2_EXAMPLES.md with code samples for common tasks
- [ ] T070 Update README.md with OAuth2 authentication section
- [ ] T071 Create GitHub issue templates for auth bugs/features (.github/ISSUE_TEMPLATE/)

## Testing & Performance (6 tasks)
- [ ] T061 [P] Create OAuth2IntegrationTests.cs with real Keycloak container
  - End-to-end authentication flow
  - Token refresh (if implemented)
  - Session timeout scenarios
  - Token expiration handling
- [ ] T062 [P] Create AuthenticationPerformanceTests.cs with benchmarks:
  - Claims extraction: <10ms per operation (NFR-001)
  - Authorization check: <5ms per operation
  - Batch operations (100 concurrent requests) under load
- [ ] T067 [P] Handle edge cases in authentication and authorization:
  - Keycloak unavailable during token validation (graceful degradation)
  - Token validation timeout (configurable with sensible default)
  - Malformed ID tokens (detailed error messages)
  - Missing required claims in valid JWT (clear error messages)
  - User roles change in Keycloak (requires re-login, documented)
  - OAuth2 disabled in configuration (all endpoints accessible)
- [ ] T066 Update OpenAPI/Swagger documentation:
  - Security scheme configuration for OIDC in OpenAPI spec
  - [Authorize] attributes propagated to generated spec
  - All endpoints marked as public or authenticated
  - /auth/status, /auth/me, /auth/logout documented
- [ ] T068 Code quality review against project standards:
  - All public APIs have XML documentation (csharp-docs skill)
  - Nullable reference types: zero warnings
  - Async methods use ConfigureAwait(false) (csharp-async skill)
  - No hardcoded values (all configuration-driven)
  - SOLID principles followed
- [ ] T069 Create test coverage summary in specs/001-keycloak-sso/checklists/test-coverage.md:
  - Authentication tests: 15+ tests ✓
  - Authorization tests: 10+ tests ✓
  - Integration tests: 8+ tests ✓
  - Edge case tests: 8+ tests ✓
  - Total: 40+ tests per SC-001 ✓
  - Coverage: 80%+ for auth modules per SC-002 ✓

## Final Validation Checklist
✅ All 71 tasks complete
✅ 6/6 user stories implemented
✅ 40+ unit tests passing
✅ 80%+ code coverage (auth modules)
✅ Zero build errors
✅ All integration tests pass
✅ Keycloak setup <30 seconds
✅ Token validation <100ms
✅ 7+ documentation pages
✅ Production-ready

**Priority**: 📚 Final polish and documentation
**Depends On**: Phase 1-8 complete"
fi

echo ""
echo "✅ All GitHub issues updated successfully!"
