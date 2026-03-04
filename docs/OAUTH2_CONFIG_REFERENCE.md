# OAuth2 Configuration Reference

## `appsettings.json` Settings

### JwtBearer (active configuration)

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

| Key | Type | Description | Default |
|---|---|---|---|
| `Enabled` | bool | Enables JWT bearer authentication | `true` |
| `Authority` | string | Keycloak realm base URL | — |
| `Audience` | string | Expected `aud` claim value | `"account"` |
| `RequireHttpsMetadata` | bool | Require HTTPS for Keycloak endpoint | `true` (prod) |

### OpenIdConnect (future/alternate configuration)

```json
{
  "OpenIdConnect": {
    "Enabled": true,
    "Authority": "http://localhost:8080/realms/hexagon",
    "ClientId": "hexagon-app",
    "ClientSecret": "...",
    "Scopes": ["openid", "profile", "email"],
    "RequiredClaims": ["sub", "email"]
  }
}
```

## Environment Variable Overrides

ASP.NET Core maps `__` (double underscore) to `:` in env vars:

```bash
export JwtBearer__Authority="https://keycloak.prod.example.com/realms/hexagon"
export JwtBearer__Audience="hexagon-api"
export JwtBearer__RequireHttpsMetadata="true"
```

## Authorization Policies

| Policy | Required Roles | Used On |
|---|---|---|
| `AdminOnly` | `admin` | `GET /admin/stats` |
| `UserAccess` | `user` OR `admin` | Todo CRUD |
| `TodoAccess` | `user` OR `admin` | POST/PUT/DELETE `/todos` |

## Protected Endpoints

| Endpoint | Auth Required | Policy |
|---|---|---|
| `GET /auth/status` | No (returns 401 if unauthed) | None |
| `GET /auth/me` | Yes | Default |
| `POST /auth/logout` | Yes | Default |
| `GET /admin/stats` | Yes | AdminOnly |
| `POST /todos` | Yes | TodoAccess |
| `PUT /todos/{id}` | Yes | TodoAccess |
| `DELETE /todos/{id}` | Yes | Default |
| `GET /todos` | No | None |
| `GET /pokemons/*` | No | None |

## Production vs Development Defaults

| Setting | Development | Production |
|---|---|---|
| `RequireHttpsMetadata` | `false` | **`true`** |
| `Cors:AllowedOrigins` | Any | Must be set explicitly |
| `JwtBearer:Enabled` | `true` | `true` |
