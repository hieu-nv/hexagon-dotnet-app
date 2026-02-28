#!/bin/bash
set -e

echo "🔄 Updating GitHub issues with correct descriptions..."
echo ""

# Get all open issues
ISSUES=$(gh issue list --state open --json number --jq '.[].number')

for issue_num in $ISSUES; do
  title=$(gh issue view $issue_num --json title --jq '.title')
  
  if [[ "$title" == *"Phase 1"* ]]; then
    echo "✓ Phase 1 (#$issue_num): Setup & Project Configuration"
    gh issue edit $issue_num --body "Initialize OAuth2/OpenID Connect infrastructure and NuGet dependencies.

## Tasks
- [ ] T001 Add NuGet dependencies to App.Api.csproj
- [ ] T002 Create Authentication configuration structure in src/App.Core/Authentication/
- [ ] T003 Create Authorization configuration structure in src/App.Core/Authorization/
- [ ] T004 Update .github/copilot-instructions.md with OAuth2/OpenID Connect patterns
- [ ] T005 Create Keycloak configuration directory at /scripts/keycloak/

## Checkpoint
✓ Project structure initialized
✓ NuGet dependencies added
✓ Ready for Phase 2

**Enables**: Phase 2: Foundational"

  elif [[ "$title" == *"Phase 2"* ]]; then
    echo "✓ Phase 2 (#$issue_num): Foundational - Port Interfaces & DI (9 tasks)"
    gh issue edit $issue_num --body "Establish port interfaces and foundational value objects that all user stories depend on.

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

  elif [[ "$title" == *"Phase 3"* ]]; then
    echo "✓ Phase 3 (#$issue_num): US1 - Keycloak Admin Setup (7 tasks)"
    gh issue edit $issue_num --body "Automate Keycloak infrastructure: realm, OAuth2 client, test users, and roles configuration. First user story in MVP scope.

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

  elif [[ "$title" == *"Phase 4"* ]]; then
    echo "✓ Phase 4 (#$issue_num): US2 - OAuth2 Configuration (8 tasks)"
    gh issue edit $issue_num --body "Enable OpenID Connect middleware in application. Developer configures OAuth2 in settings and app redirects to Keycloak login. Second user story in MVP scope.

## Acceptance Criteria
- When OAuth2 disabled: all endpoints accessible without authentication
- When OAuth2 enabled: unauthenticated users redirected to Keycloak login
- When OAuth2 enabled: valid Keycloak JWT accepted by middleware, user authenticated
- Middleware can be toggled on/off via OpenIdConnect:Enabled setting

## Tasks
- [ ] T022 [P] Create OpenIdConnectConfigurationTests.cs unit test
- [ ] T023 [P] Create OpenIdConnectMiddlewareTests.cs integration test
- [ ] T024 [P] Add OpenIdConnect config to appsettings.json
- [ ] T025 Create OpenIdConnectConfiguration.cs loader
- [ ] T026 Implement OpenIdConnectExtensions.cs middleware registration
- [ ] T027 Update Program.cs to wire OpenID Connect
- [ ] T028 Create GET /auth/status test endpoint
- [ ] T029 Add comprehensive OAuth2 event logging

## Success Criteria
✓ OAuth2 middleware integrated and toggleable
✓ Configuration validated at startup
✓ Keycloak redirect working for unauthenticated users
✓ Swagger documents authentication
✓ All integration tests pass

✅ **MVP Checkpoint 2 of 3**: OAuth2 middleware integrated

**Enables**: Phase 5 (Claims extraction) to complete MVP
**Depends On**: Phase 1, Phase 2, Phase 3"

  elif [[ "$title" == *"Phase 5"* ]]; then
    echo "✓ Phase 5 (#$issue_num): US3 - Claims Extraction (8 tasks)"
    gh issue edit $issue_num --body "Extract user identity from OAuth2 ID tokens into AuthenticatedUser objects. Third user story in MVP scope - completes end-to-end OAuth2 authentication.

## Acceptance Criteria
- AuthService.GetAuthenticatedUser() extracts user identity from OAuth2 ClaimsPrincipal
- Extracted AuthenticatedUser includes email, name, roles from ID token
- Missing required claims (email, subject) are detected and rejected
- Custom claims beyond standard OpenID Connect are preserved
- Unit tests verify all scenarios WITHOUT requiring Keycloak running

## Tasks
- [ ] T030 [P] Create ClaimsExtractorTests.cs comprehensive unit tests
- [ ] T031 [P] Create AuthServiceTests.cs unit tests
- [ ] T032 [P] Implement KeycloakClaimsExtractor adapter (IClaimsExtractor)
- [ ] T033 Create OpenIdConnectClaims.cs constants file
- [ ] T034 Create AuthServiceExtensions.cs convenience methods
- [ ] T035 Create OpenIdConnectTokenHandler.cs for JWT validation
- [ ] T036 Create AuthenticationEventHandler.cs subscribing to OpenIdConnectEvents
- [ ] T037 Create GET /auth/me endpoint in AuthInfoEndpoints.cs

## Success Criteria
✓ Claims extraction fully functional (unit tests all pass)
✓ Authenticated users have complete identity information
✓ AuthService integrated into OIDC event pipeline
✓ /auth/me endpoint returns user details correctly
✓ All 40+ unit tests for authentication module pass
✓ 80%+ code coverage for auth modules

✅ **MVP COMPLETE**: End-to-end OAuth2 authentication fully working!

**Enables**: MVP is complete! Phases 6-9 are independent enhancements
**Depends On**: Phase 1, Phase 2, Phase 3, Phase 4"

  elif [[ "$title" == *"Phase 6"* ]]; then
    echo "✓ Phase 6 (#$issue_num): US4 - Authorization Policies (7 tasks)"
    gh issue edit $issue_num --body "Create flexible authorization policy system for role-based access control. No Keycloak required for these tests - uses mock AuthenticatedUser objects.

## Acceptance Criteria
- Developers can define authorization policies (e.g., 'AdminOnly', 'TodoAccess')
- Policies based on user roles (single role, multiple roles with OR logic)
- Authorization logic tested with mocked AuthenticatedUser objects
- Policies can be created and tested without running Keycloak instance

## Tasks
- [ ] T038 [P] Create AuthorizationPolicyTests.cs unit tests
- [ ] T039 [P] Create AuthorizationServiceTests.cs unit tests
- [ ] T040 [P] Create AuthorizationPoliciesRegistry.cs defining standard policies
- [ ] T041 Implement DefaultAuthorizationPolicyProvider adapter
- [ ] T042 Update AuthorizationService implementation
- [ ] T043 Create AuthorizationHelpers.cs utility methods
- [ ] T044 Create AUTHORIZATION_POLICIES.md documentation

## Success Criteria
✓ Policy system fully testable with mocked users
✓ Built-in policies for common scenarios (Admin, User, Todo Access)
✓ All unit tests pass (10+ authorization tests)
✓ Ready for endpoint protection (Phase 7)

**Priority**: P2 (after MVP delivered)

**Depends On**: Phase 2"

  elif [[ "$title" == *"Phase 7"* ]]; then
    echo "✓ Phase 7 (#$issue_num): US5 - Endpoint Protection (8 tasks)"
    gh issue edit $issue_num --body "Protect API endpoints with OAuth2 authentication and authorization. Endpoints return 401 (unauthenticated) and 403 (unauthorized) when appropriate.

## Acceptance Criteria
- Endpoints without [Authorize] attribute are accessible without authentication
- Endpoints with [Authorize] return 401 for unauthenticated users
- Endpoints with [Authorize(\"policy\")] return 403 for unauthorized users
- Protected endpoints return 200 for authorized users
- Swagger/OpenAPI properly documents authentication requirements

## Tasks
- [ ] T045 [P] Create AuthorizedEndpointsTests.cs integration tests
- [ ] T046 [P] Create AuthenticationContractTests.cs tests
- [ ] T047 [P] Create AuthorizationMiddleware.cs helper
- [ ] T048 Update TodoEndpoints.cs with authorization
- [ ] T049 Create AdminEndpoints.cs example protected endpoint
- [ ] T050 Update Swagger/OpenAPI configuration in Program.cs
- [ ] T051 Create EndpointAuthorizationExtensions.cs fluent API
- [ ] T052 Add AuthorizationLogging.cs audit logging

## Success Criteria
✓ All endpoints properly protected with [Authorize] attributes
✓ Authentication & authorization enforced (401, 403 status codes)
✓ Swagger documents security requirements
✓ Audit logging tracks all authorization decisions
✓ All integration tests pass (6+ endpoint protection tests)

**Priority**: P2 (after MVP delivered)

**Depends On**: Phase 1-6"

  elif [[ "$title" == *"Phase 8"* ]]; then
    echo "✓ Phase 8 (#$issue_num): US6 - Logout (5 tasks)"
    gh issue edit $issue_num --body "Implement logout endpoint that clears OAuth2 session and cookies. User must re-authenticate after logout.

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

**Priority**: P3 (nice-to-have after MVP)
**Depends On**: Phase 1-5"

  elif [[ "$title" == *"Phase 9"* ]]; then
    echo "✓ Phase 9 (#$issue_num): Polish & Documentation (13 tasks)"
    gh issue edit $issue_num --body "Documentation, edge case handling, performance validation, and feature completeness. All user stories implemented - now production-harden and document.

## Documentation (7 tasks)
- [ ] T059 Create OAUTH2_ARCHITECTURE.md with component diagrams and data flows
- [ ] T060 Create OAUTH2_TROUBLESHOOTING.md with common errors and solutions
- [ ] T063 Create OAUTH2_SECURITY.md with hardening checklist (HTTPS, secrets, PKCE, CORS)
- [ ] T064 Create OAUTH2_CONFIG_REFERENCE.md with all configuration options
- [ ] T065 Create OAUTH2_EXAMPLES.md with code samples for common tasks
- [ ] T070 Update README.md with OAuth2 authentication section
- [ ] T071 Create GitHub issue templates for auth bugs/features

## Testing & Performance (6 tasks)
- [ ] T061 [P] Create OAuth2IntegrationTests.cs with real Keycloak container
- [ ] T062 [P] Create AuthenticationPerformanceTests.cs with benchmarks
- [ ] T067 [P] Handle edge cases in authentication and authorization
- [ ] T066 Update OpenAPI/Swagger documentation
- [ ] T068 Code quality review against project standards
- [ ] T069 Create test coverage summary in specs/001-keycloak-sso/checklists/test-coverage.md

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
done

echo ""
echo "✅ All GitHub issues updated successfully!"
