namespace App.Api.Auth;

/// <summary>
/// Defines claim type constants for OpenID Connect and Keycloak-specific claims.
/// Centralises string literals to avoid magic strings throughout the codebase.
/// </summary>
internal static class OpenIdConnectClaims
{
    // ─── Standard OpenID Connect claim types ─────────────────────────────────

    /// <summary>Subject identifier — the unique user ID (<c>sub</c>).</summary>
    public const string Subject = "sub";

    /// <summary>User's email address.</summary>
    public const string Email = "email";

    /// <summary>Full display name.</summary>
    public const string Name = "name";

    /// <summary>Preferred username (login name).</summary>
    public const string PreferredUsername = "preferred_username";

    /// <summary>JWT identifier — unique token ID.</summary>
    public const string JwtId = "jti";

    /// <summary>Issued-at time (Unix timestamp).</summary>
    public const string IssuedAt = "iat";

    /// <summary>Expiration time (Unix timestamp).</summary>
    public const string Expiration = "exp";

    // ─── Keycloak-specific claim paths ───────────────────────────────────────

    /// <summary>Keycloak realm-level roles, encoded as <c>{"roles":["admin","user"]}</c>.</summary>
    public const string RealmAccess = "realm_access";

    /// <summary>Keycloak client-level roles.</summary>
    public const string ResourceAccess = "resource_access";

    /// <summary>Authorized party — the client ID that the token was issued for.</summary>
    public const string AuthorizedParty = "azp";

    /// <summary>OAuth2 scope string as a space-separated list.</summary>
    public const string Scope = "scope";

    // ─── Custom claim conventions ─────────────────────────────────────────────

    /// <summary>Prefix used for application-specific custom claims.</summary>
    public const string CustomClaimPrefix = "app_";

    /// <summary>Separator used between role values when encoded as a single string.</summary>
    public const char RoleSeparator = ' ';
}
