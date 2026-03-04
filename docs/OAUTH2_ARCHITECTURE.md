# OAuth2 Architecture Guide

## Overview

This application uses **JwtBearer** authentication backed by **Keycloak** as the OAuth2/OpenID Connect identity provider. The architecture follows a hexagonal (ports & adapters) pattern.

## Component Diagram

```
┌──────────────────────────────────────────────────────────────────────────┐
│                          HTTP Request                                     │
└──────────────────────────────────┬───────────────────────────────────────┘
                                   │
                                   ▼
┌──────────────────────────────────────────────────────────────────────────┐
│ ASP.NET Core Pipeline                                                     │
│  UseAuthentication → UseAuthorization → Endpoint                         │
│                                                                           │
│  JwtBearerMiddleware validates JWT (RS256) against Keycloak JWKS endpoint│
└──────────────────────────────────┬───────────────────────────────────────┘
                                   │ ClaimsPrincipal
                                   ▼
┌──────────────────────────────────────────────────────────────────────────┐
│ Auth Layer (App.Api/Auth/)                                                │
│                                                                           │
│  IClaimsExtractor ◄─── KeycloakClaimsExtractor                          │
│  AuthService ◄─────────── DI injected                                   │
│  IAuthorizationPolicyProvider ◄─── DefaultAuthorizationPolicyProvider   │
│  AuthorizationService ◄──── DI injected                                 │
└──────────────────────────────────────────────────────────────────────────┘
                                   │
                                   ▼
┌────────────────────┐    ┌────────────────────┐    ┌────────────────────┐
│  /auth/status      │    │  /auth/me          │    │  /admin/stats      │
│  (open)            │    │  [Authorize]       │    │  [AdminOnly]       │
└────────────────────┘    └────────────────────┘    └────────────────────┘
```

## Authentication Flow

1. Client sends `Authorization: Bearer <JWT>` header
2. JwtBearerMiddleware fetches Keycloak's JWKS endpoint and validates the signature
3. Token claims are deserialized into a `ClaimsPrincipal`
4. `KeycloakClaimsExtractor` maps the principal to an `AuthenticatedUser` domain object
5. `AuthorizationService` evaluates policies to allow/deny access

## Token Lifecycle

```
Client → POST /token → Keycloak
Keycloak → { access_token, refresh_token, expires_in }

Client → GET /api/resource + Bearer <access_token>
App → validates signature via JWKS URI → 200 OK

access_token expires → Client → POST /token (refresh_token grant)
Keycloak → new access_token
```

## Claims Extraction Pipeline

```
ClaimsPrincipal
  └── ClaimTypes.NameIdentifier  →  AuthenticatedUser.Id
  └── ClaimTypes.Email           →  AuthenticatedUser.Email
  └── "name"                     →  AuthenticatedUser.Name
  └── "realm_access.roles"       →  AuthenticatedUser.Roles (JSON deserialized)
  └── ClaimTypes.Role            →  AuthenticatedUser.Roles (standard mapping)
  └── all other non-standard     →  AuthenticatedUser.CustomClaims
```

## Authorization Decision Flow

```
Request arrives at [Authorize("AdminOnly")] endpoint
  → ASP.NET Core evaluates the "AdminOnly" policy
  → Policy uses RequireAssertion
    → Resolves IClaimsExtractor from DI
    → Calls ExtractFromPrincipal(context.User)
    → If null → false (deny)
    → AuthService.AuthorizeByRoles(user, ["admin"])
      → user.HasRole("admin") (case-insensitive)
      → true → allow | false → 403 Forbidden
```

## Key Files

| File | Role |
|---|---|
| `Auth/IClaimsExtractor.cs` | Port interface for claims extraction |
| `Auth/KeycloakClaimsExtractor.cs` | Keycloak-specific claims adapter |
| `Auth/AuthService.cs` | Authentication domain service |
| `Auth/IAuthorizationPolicyProvider.cs` | Port interface for policies |
| `Auth/DefaultAuthorizationPolicyProvider.cs` | In-memory policy registry adapter |
| `Auth/AuthorizationService.cs` | Authorization domain service |
| `Auth/AuthorizationPoliciesRegistry.cs` | Defines AdminOnly, UserAccess, TodoAccess |
| `Program.cs` | Wires JwtBearer + Authorization policies |
