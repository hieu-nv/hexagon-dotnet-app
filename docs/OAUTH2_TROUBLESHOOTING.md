# OAuth2 Troubleshooting Guide

## Common Issues

### `401 Unauthorized` on all endpoints

| Check | Action |
|---|---|
| `JwtBearer:Enabled` is `true` | Check `appsettings.json` |
| Authorization header present | `Authorization: Bearer <token>` |
| Token not expired | Decode at [jwt.io](https://jwt.io) |
| Keycloak running | `curl http://localhost:8080/health/ready` |

### Token Validation Failures

**Expired token** (`exp` claim in the past):
```bash
# Re-fetch a fresh token
TOKEN=$(curl -s ... | jq -r .access_token)
```

**Invalid signature** (wrong Keycloak instance):
- Verify `JwtBearer:Authority` matches the Keycloak realm URL exactly
- Check `JwtBearer:Audience` matches the Keycloak client ID or `account`

**Missing claims** (email, sub):
- Ensure Keycloak client has `email` scope mapped
- Check the Keycloak client's "Default Client Scopes" includes `email` and `profile`

### Role claims missing from token

Roles are stored in `realm_access.roles` in the Keycloak JWT. If `KeycloakClaimsExtractor` returns no roles:
1. Open Keycloak admin console → Client Scopes → `roles` → Mappers
2. Ensure "realm roles" mapper is enabled and token claim name is `realm_access`
3. Alternatively enable direct role claims via the "roles" client scope

### Keycloak connectivity issues

```bash
# Check Keycloak is healthy
curl http://localhost:8080/health/ready

# Check realm exists
curl http://localhost:8080/realms/hexagon/.well-known/openid-configuration

# Re-run setup if realm is missing
./scripts/keycloak-setup.sh
```

### `403 Forbidden` with valid JWT

- User exists in Keycloak but has wrong roles
- Use the `/auth/me` endpoint to inspect user roles: `GET /auth/me`
- Assign the correct realm role in Keycloak admin console

## Debugging with `/auth/status` and `/auth/me`

```bash
# Check if your token is recognized
curl -H "Authorization: Bearer $TOKEN" http://localhost:5112/auth/status
# → { "isAuthenticated": true, "email": "...", "roles": [...] }

# Get full user profile
curl -H "Authorization: Bearer $TOKEN" http://localhost:5112/auth/me
# → { "id": "...", "email": "...", "roles": [...], "customClaims": {...} }
```

## Log Levels for Debugging

Temporarily set in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.Authentication": "Debug",
      "Microsoft.AspNetCore.Authorization": "Debug"
    }
  }
}
```
