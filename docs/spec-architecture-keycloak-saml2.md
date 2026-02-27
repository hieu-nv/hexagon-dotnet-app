---
title: Keycloak SAML2 Integration Specification
version: 1.0
date_created: 2026-02-26
tags: [authentication, security, architecture, saml2]
---

# Keycloak SAML2 Integration Specification

## 1. Purpose & Scope

This specification defines the security architecture for integrating Keycloak as a SAML2 Identity Provider (IdP) with the Hexagon .NET application. All API endpoints will require SAML2 authentication. The scope includes:

- **SAML2 authentication configuration** in ASP.NET Core
- **Keycloak container setup** using podman
- **Authorization middleware** for protected endpoints
- **User claims mapping** from SAML assertions
- **Security headers and policies**

**Audience**: Backend engineers, DevOps, Security team

## 2. Definitions

- **SAML 2.0**: Security Assertion Markup Language - XML-based protocol for authentication and authorization
- **IdP**: Identity Provider (Keycloak)
- **SP**: Service Provider (Hexagon .NET App)
- **Assertion**: XML document containing authentication credentials
- **Metadata**: XML file describing SAML endpoints and certificates
- **Claims**: User attributes (email, roles, groups) extracted from SAML assertion

## 3. Requirements, Constraints & Guidelines

### Authentication & Authorization

- **REQ-AUTH-001**: All API endpoints require valid SAML2 authentication
- **REQ-AUTH-002**: User identity extracted from SAML assertion (email, name, roles)
- **REQ-AUTH-003**: Role-based access control (RBAC) for specific endpoints
- **REQ-AUTH-004**: Session management using HTTP-only, secure cookies

### SAML Configuration

- **REQ-SAML-001**: SAML2.0 Web Browser SSO Profile
- **REQ-SAML-002**: HTTP POST/Redirect bindings for assertion delivery
- **REQ-SAML-003**: Digitally signed SAML responses from Keycloak
- **REQ-SAML-004**: Support for single logout (SLO) from Keycloak

### Security

- **SEC-001**: HTTPS/TLS only in production (no plain HTTP)
- **SEC-002**: SAML assertions must be encrypted and signed
- **SEC-003**: Secure cookie settings (HttpOnly, Secure, SameSite)
- **SEC-004**: CSRF protection on all endpoints
- **SEC-005**: No sensitive user data in logs

### Infrastructure

- **INF-001**: Keycloak containerized with podman for local development
- **INF-002**: SQLite persistent volume for Keycloak data
- **INF-003**: Service discovery via Aspire AppHost

### Code Quality

- **PAT-001**: Follow hexagonal architecture (ports & adapters)
- **PAT-002**: Separate authentication concern (Auth domain)
- **PAT-003**: Dependency injection for SAML configuration
- **PAT-004**: Comprehensive test coverage for auth policies

## 4. Interfaces & Data Contracts

### SAML Assertion Claims (from Keycloak)

```xml
<saml:Assertion>
  <saml:AuthnStatement>
    <saml:Subject>
      <saml:NameID Format="urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress">
        user@example.com
      </saml:NameID>
    </saml:Subject>
  </saml:AuthnStatement>
  <saml:AttributeStatement>
    <saml:Attribute Name="email" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:basic">
      <saml:AttributeValue>user@example.com</saml:AttributeValue>
    </saml:Attribute>
    <saml:Attribute Name="name" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:basic">
      <saml:AttributeValue>John Doe</saml:AttributeValue>
    </saml:Attribute>
    <saml:Attribute Name="groups" NameFormat="urn:oasis:names:tc:SAML:2.0:attrname-format:basic">
      <saml:AttributeValue>admin</saml:AttributeValue>
      <saml:AttributeValue>users</saml:AttributeValue>
    </saml:Attribute>
  </saml:AttributeStatement>
</saml:Assertion>
```

### Application User Principal (ClaimsPrincipal)

```csharp
new ClaimsPrincipal(new ClaimsIdentity(
    new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, "user@example.com"),
        new(ClaimTypes.Email, "user@example.com"),
        new(ClaimTypes.Name, "John Doe"),
        new(ClaimTypes.Role, "admin"),
        new(ClaimTypes.Role, "users")
    },
    "SAML2"
));
```

### SAML Metadata Endpoint

```
GET https://localhost:5112/saml/metadata
```

Returns SAML Service Provider metadata for Keycloak integration.

## 5. Acceptance Criteria

- **AC-001**: Given a user logs into Keycloak, When they access the .NET API, Then they receive valid SAML assertion
- **AC-002**: Given a protected endpoint `/todos`, When accessed without SAML authentication, Then returns 401 Unauthorized
- **AC-003**: Given authenticated user, When they access endpoint, Then User.Identity.Name contains their email
- **AC-004**: Given user with "admin" role, When they access admin endpoint, Then request is allowed
- **AC-005**: Given user without required role, When they access protected endpoint, Then returns 403 Forbidden
- **AC-006**: Given authenticated user, When they logout from Keycloak, Then subsequent API calls fail with 401

## 6. Test Automation Strategy

- **Unit Tests**: SAML claim extraction, role validation (xUnit/Moq)
- **Integration Tests**: SAML metadata generation, auth middleware behavior
- **Manual Tests**: Keycloak SSO flow, logout flow
- **Security Tests**: CSRF prevention, secure headers, session handling
- **Coverage Target**: >80% for authentication module

## 7. Rationale & Context

SAML2 provides:

- Enterprise-grade authentication
- Centralized identity management via Keycloak
- Single Sign-On (SSO) capabilities
- Strong security (encryption, signatures)
- Compliance with enterprise standards

Hexagonal architecture requirement:

- Auth logic abstracted as a domain/port
- Testable without Keycloak for unit tests
- Infrastructure (SAML handler) pluggable

## 8. Dependencies & External Integrations

### External Systems

- **EXT-001**: Keycloak IdP - Provides SAML 2.0 authentication service
- **EXT-002**: Keycloak Realm - Contains users, roles, SAML clients

### Required NuGet Packages

- **Kentor.AuthServices** - SAML 2.0 handling for ASP.NET Core
- **Sustainsys.Saml2.AspNetCore2** - SAML middleware integration
- **System.Security.Cryptography.Xml** - SAML assertion signing/encryption

### Infrastructure

- **INF-001**: Podman container runtime
- **INF-002**: Keycloak image (keycloak/keycloak:latest)
- **INF-003**: SQLite database for Keycloak persistence

## 9. Implementation Layers (Hexagonal)

### Core Domain (`App.Core/Auth/`)

```
ITrustedClaimsPrincipalProvider    - Port (interface)
ISamlClaimsExtractor               - Port for claims extraction
AuthenticationPolicy               - Domain policy/value object
```

### Infrastructure (`App.Data/Auth/`)

```
SamlClaimsExtractor                - Adapter implementing claims extraction
```

### API (`App.Api/Auth/`)

```
AuthenticationMiddleware           - SAML HTTP adapter
SamlMetadataEndpoint              - SAML metadata endpoint
```

---

**Next Steps**:

1. Create Keycloak podman setup
2. Add SAML NuGet packages
3. Implement Auth domain (Core)
4. Configure SAML middleware
5. Protect endpoints with authorization policies
6. Add comprehensive tests
