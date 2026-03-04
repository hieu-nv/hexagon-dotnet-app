# OAuth2 Security Checklist

## HTTPS Enforcement

- [ ] Set `JwtBearer:RequireHttpsMetadata: true` in production
- [ ] Redirect URI must use `https://` in production Keycloak client
- [ ] Configure HTTPS redirect middleware in production

## Client Secret Management

- [ ] Never commit `ClientSecret` to source control
- [ ] Use environment variables or secret managers (Azure Key Vault, AWS Secrets Manager)
- [ ] Example: `JwtBearer__Authority` → ASP.NET Core environment variable override
- [ ] Rotate secrets periodically in Keycloak client settings

## PKCE for SPA/Mobile

- [ ] Set `pkce.code.challenge.method: S256` in client config
- [ ] `publicClient: false` for confidential server-side apps
- [ ] `publicClient: true` + PKCE for JavaScript SPA clients

## Token Validation

- [ ] Validate `iss` (issuer) claim matches expected authority
- [ ] Validate `aud` (audience) claim matches `JwtBearer:Audience`
- [ ] Short token lifespans (`accessTokenLifespan: 3600` = 1 hour)
- [ ] RS256 signature validation via JWKS endpoint (handled automatically by JwtBearer)

## Cookie Security (if using cookie auth)

- [ ] `HttpOnly: true` — prevents JavaScript access
- [ ] `Secure: true` — HTTPS only
- [ ] `SameSite: Strict` — CSRF protection

## CORS

```json
{
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com"]
  }
}
```

In production, never set `AllowAnyOrigin()`. The application enforces this in `Program.cs`.

## Rate Limiting on Auth Endpoints

The application uses a fixed-window rate limiter (100 req/min). For additional protection on auth endpoints, consider reducing the limit specifically for `/auth/` routes.

## Audit Logging

All authentication events are logged at `Information` level. Authorization denials are logged at `Warning`. No sensitive data (tokens, secrets, passwords) is ever logged.
