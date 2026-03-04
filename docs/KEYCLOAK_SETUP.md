# Keycloak SSO Setup Guide

## Prerequisites

- [Podman](https://podman.io/) or [Docker](https://www.docker.com/)
- `curl` and `jq` (for the setup script)
- .NET 10 SDK

## Quick Start

```bash
# 1. Start Keycloak
docker-compose -f docker-compose.keycloak.yml up -d

# 2. Wait ~10 seconds for Keycloak to boot, then run the setup script
./scripts/keycloak-setup.sh
```

The setup script automatically creates the `hexagon` realm, the `hexagon-api` client, two realm roles (`admin`, `user`), and two test user accounts.

## Configuration Reference

| Setting | Value |
|---|---|
| Keycloak URL | `http://localhost:8080` |
| Realm | `hexagon` |
| Client ID | `hexagon-api` |
| Admin console | `http://localhost:8080/admin` |

### Application Settings (`appsettings.json`)

```json
{
  "JwtBearer": {
    "Enabled": true,
    "Authority": "http://localhost:8080/realms/hexagon",
    "Audience": "account",
    "RequireHttpsMetadata": false
  }
}
```

## Test Credentials

| User | Password | Roles |
|---|---|---|
| `admin@example.com` | `Admin1234!` | admin, user |
| `user@example.com` | `User1234!` | user |

> **Note**: Passwords are temporary and must be changed on first login.

## Keycloak Configuration Files

Located in `scripts/keycloak/`:

| File | Purpose |
|---|---|
| `realm-export.json` | Realm settings (event logs, password policy, brute-force protection) |
| `client-config.json` | OAuth2 client definition (PKCE, RS256, redirect URIs) |
| `users-roles.json` | Test user definitions and role assignments |

## Obtaining a JWT for API Testing

```bash
# Admin user token
TOKEN=$(curl -s -X POST "http://localhost:8080/realms/hexagon/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=hexagon-api" \
  -d "username=admin@example.com" \
  -d "password=Admin1234!" \
  -d "grant_type=password" | jq -r .access_token)

# Use the token
curl -H "Authorization: Bearer $TOKEN" http://localhost:5112/auth/status
curl -H "Authorization: Bearer $TOKEN" http://localhost:5112/auth/me
curl -H "Authorization: Bearer $TOKEN" http://localhost:5112/admin/stats
```

## Logout Flow

```bash
# POST /auth/logout returns the Keycloak logout URL
curl -X POST -H "Authorization: Bearer $TOKEN" http://localhost:5112/auth/logout
# Response: { "loggedOut": true, "logoutUrl": "http://localhost:8080/realms/hexagon/protocol/openid-connect/logout?..." }

# Redirect the browser to logoutUrl to complete SSO logout
```

## Troubleshooting

| Problem | Solution |
|---|---|
| `401 Unauthorized` on all endpoints | Ensure `JwtBearer:Enabled: true` and Keycloak is running |
| Token validation fails | Verify `Authority` matches the Keycloak realm URL exactly |
| `realm_access` roles missing from token | Confirm client scope includes `roles` mapper |
| Setup script fails | Check Keycloak container is healthy: `curl http://localhost:8080/health/ready` |
