#!/bin/bash
# Script to create GitHub issues from task list
# Usage: ./create-github-issues.sh

set -e

REPO="."
LABELS_PHASE1="phase-1,setup,oauth2,infrastructure"
LABELS_PHASE2="phase-2,foundational,blocking,authentication"
LABELS_PHASE3="phase-3,user-story-1,p1-priority,keycloak,mvp"
LABELS_PHASE4="phase-4,user-story-2,p1-priority,oauth2,mvp"
LABELS_PHASE5="phase-5,user-story-3,p1-priority,claims,mvp"
LABELS_PHASE6="phase-6,user-story-4,p2-priority,authorization"
LABELS_PHASE7="phase-7,user-story-5,p2-priority,endpoints"
LABELS_PHASE8="phase-8,user-story-6,p3-priority,logout"
LABELS_PHASE9="phase-9,documentation,polish,testing"

echo "Creating GitHub Issues for Keycloak SSO Implementation..."
echo ""

# Function to create an issue
create_issue() {
    local title="$1"
    local labels="$2"
    local body="$3"
    
    echo "Creating: $title"
    gh issue create \
        --title "$title" \
        --label "$labels" \
        --body "$body" \
        2>/dev/null || echo "  ⚠️  Issue creation failed (check gh auth)"
    echo ""
}

# Phase 1
create_issue \
    "[Phase 1] Setup & Project Configuration (5 tasks)" \
    "$LABELS_PHASE1" \
    "Initialize OAuth2/OpenID Connect infrastructure and NuGet dependencies.

## Tasks
- [ ] T001 Add NuGet dependencies to App.Api.csproj
- [ ] T002 Create Authentication configuration structure  
- [ ] T003 Create Authorization configuration structure
- [ ] T004 Update copilot-instructions.md with OAuth patterns
- [ ] T005 Create Keycloak configuration directory

## Checkpoint
✓ Project structure initialized
✓ NuGet dependencies added  
✓ Ready for Phase 2

**Enables**: Phase 2: Foundational"

# Phase 2  
create_issue \
    "[Phase 2] Foundational - Port Interfaces & DI (9 tasks) ⚠️ BLOCKING" \
    "$LABELS_PHASE2" \
    "Establish port interfaces and foundational value objects.

⚠️ **CRITICAL**: Phase 2 MUST complete before any user story begins.

## Port Interfaces & Value Objects
- [ ] T006 [P] IClaimsExtractor interface
- [ ] T007 [P] IAuthorizationPolicyProvider interface
- [ ] T008 [P] AuthenticatedUser value object
- [ ] T009 [P] AuthenticationPolicy value object
- [ ] T010 [P] OpenIdConnectConfiguration value object

## Domain Services & Extension Methods
- [ ] T011 AuthService implementation
- [ ] T012 AuthorizationService implementation
- [ ] T013 AuthenticationExtensions.cs
- [ ] T014 AuthorizationExtensions.cs

## Checkpoint
✓ All port interfaces defined
✓ Value objects created
✓ Services implemented
✓ Ready for parallel user story work (Phases 3-8)

**Depends On**: Phase 1"

# Phase 3
create_issue \
    "[Phase 3] US1 - Keycloak Admin Setup (7 tasks) 🚀 MVP" \
    "$LABELS_PHASE3" \
    "Automate Keycloak realm, OAuth2 client, test users, and roles configuration.

## Goal
Administrator can run setup script and verify realm, client, users in Keycloak console.

## Tasks
- [ ] T015 [P] KeycloakSetupTests.cs integration test
- [ ] T016 [P] realm-export.json configuration
- [ ] T017 [P] client-config.json configuration
- [ ] T018 [P] users-roles.json test users
- [ ] T019 keycloak-setup.sh automation script (<30s)
- [ ] T020 podman-compose.yml for Keycloak
- [ ] T021 KEYCLOAK_SETUP.md documentation

## Success Criteria
✓ Setup script completes <30 seconds
✓ Admin verifies realm, client, users
✓ Integration tests pass

**Part of MVP scope**: User Stories 1-3 = end-to-end authentication working"

# Phase 4
create_issue \
    "[Phase 4] US2 - OAuth2 Configuration (8 tasks) 🚀 MVP" \
    "$LABELS_PHASE4" \
    "Enable OpenID Connect middleware in application.

## Goal
Developer sets OpenIdConnect:Enabled=true in appsettings.json, starts app, 
gets redirected to Keycloak login. Configuration is toggleable.

## Tasks  
- [ ] T022 [P] OpenIdConnectConfigurationTests.cs
- [ ] T023 [P] OpenIdConnectMiddlewareTests.cs
- [ ] T024 [P] appsettings.json OIDC config
- [ ] T025 OpenIdConnectConfiguration.cs loader
- [ ] T026 OpenIdConnectExtensions.cs middleware
- [ ] T027 Program.cs wiring with conditional enable
- [ ] T028 GET /auth/status test endpoint
- [ ] T029 OAuth2 event logging

## Success Criteria
✓ OAuth2 middleware integrated
✓ Configuration is toggleable
✓ Swagger documents auth
✓ All tests pass

**Part of MVP scope**: Enables end-to-end testing with Keycloak"

# Phase 5
create_issue \
    "[Phase 5] US3 - Claims Extraction (6 tasks) 🚀 MVP" \
    "$LABELS_PHASE5" \
    "Extract user identity from OAuth2 ID tokens into AuthenticatedUser objects.

## Goal
Unit tests verify AuthService.GetAuthenticatedUser() correctly extracts claims 
from mocked ClaimsPrincipal. No Keycloak required for unit tests.

## Tasks
- [ ] T030 [P] ClaimsExtractorTests.cs (email, name, roles, custom claims)
- [ ] T031 [P] AuthServiceTests.cs
- [ ] T032 [P] KeycloakClaimsExtractor.cs adapter
- [ ] T033 OpenIdConnectClaims.cs constants
- [ ] T034 AuthServiceExtensions.cs convenience methods
- [ ] T035 OpenIdConnectTokenHandler.cs JWT validation
- [ ] T036 AuthenticationEventHandler.cs OIDC events
- [ ] T037 AuthInfoEndpoints.cs GET /auth/me endpoint

## Success Criteria  
✓ Claims extraction fully functional
✓ Authenticated users have identity information
✓ All tests pass

✅ **MVP COMPLETE**: Full end-to-end OAuth2 authentication working"

# Phase 6
create_issue \
    "[Phase 6] US4 - Authorization Policies (5 tasks)" \
    "$LABELS_PHASE6" \
    "Create flexible authorization policy system for role-based access control.

## Goal
Unit tests verify authorization logic with mock AuthenticatedUser. No Keycloak needed.

## Tasks
- [ ] T038 [P] AuthorizationPolicyTests.cs
- [ ] T039 [P] AuthorizationServiceTests.cs  
- [ ] T040 [P] AuthorizationPoliciesRegistry.cs (AdminOnly, UserAccess, TodoAccess)
- [ ] T041 DefaultAuthorizationPolicyProvider.cs adapter
- [ ] T042 AuthorizationService implementation
- [ ] T043 AuthorizationHelpers.cs utility methods
- [ ] T044 AUTHORIZATION_POLICIES.md documentation

## Success Criteria
✓ Policy system functional and testable
✓ All tests pass
✓ Ready for endpoint protection

**Priority**: P2 (after MVP delivered)"

# Phase 7
create_issue \
    "[Phase 7] US5 - Endpoint Protection (6 tasks)" \
    "$LABELS_PHASE7" \
    "Make it easy to protect endpoints with [Authorize] attributes and policies.

## Goal
Endpoints return 401 (unauthenticated) and 403 (unauthorized). 
Verified via HTTP integration tests.

## Tasks
- [ ] T045 [P] AuthorizedEndpointsTests.cs
- [ ] T046 [P] AuthenticationContractTests.cs OpenAPI verification
- [ ] T047 [P] AuthorizationMiddleware.cs
- [ ] T048 TodoEndpoints.cs protect with [Authorize]
- [ ] T049 AdminEndpoints.cs protected endpoints example
- [ ] T050 Swagger/OpenAPI config
- [ ] T051 EndpointAuthorizationExtensions.cs fluent API
- [ ] T052 AuthorizationLogging.cs audit logging

## Success Criteria
✓ All endpoints protected
✓ Authentication & authorization enforced
✓ Swagger documents security
✓ All tests pass

**Priority**: P2 (after MVP delivered)"

# Phase 8
create_issue \
    "[Phase 8] US6 - Logout (5 tasks)" \
    "$LABELS_PHASE8" \
    "Implement logout endpoint that clears OAuth2 session.

## Goal  
Logout clears session. Protected endpoints redirect to Keycloak after logout.

## Tasks
- [ ] T053 [P] LogoutTests.cs integration test
- [ ] T054 [P] AuthLogoutEndpoints.cs POST/GET /auth/logout
- [ ] T055 LogoutService.cs build logout redirect
- [ ] T056 LogoutExtensions.cs convenience methods
- [ ] T057 logout-success.html post-logout page
- [ ] T058 KEYCLOAK_SETUP.md logout documentation

## Success Criteria
✓ Logout functional  
✓ Session completely cleared
✓ All tests pass

✅ **FEATURE COMPLETE**: All 6 user stories done

**Priority**: P3 (nice-to-have)"

# Phase 9
create_issue \
    "[Phase 9] Polish & Documentation (13 tasks)" \
    "$LABELS_PHASE9" \
    "Documentation, edge cases, performance, completeness.

## Documentation
- [ ] T059 OAUTH2_ARCHITECTURE.md
- [ ] T060 OAUTH2_TROUBLESHOOTING.md  
- [ ] T063 OAUTH2_SECURITY.md
- [ ] T064 OAUTH2_CONFIG_REFERENCE.md
- [ ] T065 OAUTH2_EXAMPLES.md
- [ ] T070 README.md update
- [ ] T071 GitHub issue templates

## Testing & Quality
- [ ] T061 [P] OAuth2IntegrationTests.cs (real Keycloak)
- [ ] T062 [P] AuthenticationPerformanceTests.cs (<10ms claims, <5ms auth)
- [ ] T067 [P] Edge case handling
- [ ] T066 Swagger/OpenAPI complete
- [ ] T068 Code review (docs, nullability, async)
- [ ] T069 test-coverage.md (40+ tests, 80%+ coverage)

## Final Validation
✅ All 71 tasks complete
✅ 100% auth tests pass
✅ All 84 app tests pass
✅ Zero build errors  
✅ 80%+ auth coverage
✅ Complete documentation

✅ **FEATURE COMPLETE & PRODUCTION-READY**"

echo "✅ GitHub issues creation script completed!"
echo ""
echo "Next steps:"
echo "1. Install gh CLI: https://cli.github.com"
echo "2. Authenticate: gh auth login"
echo "3. Run: bash create-github-issues.sh"
echo ""
echo "For manual creation, see: specs/001-keycloak-sso/github-issues.md"
