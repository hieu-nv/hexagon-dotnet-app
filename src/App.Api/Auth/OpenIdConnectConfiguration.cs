using System.ComponentModel.DataAnnotations;

namespace App.Api.Auth;

/// <summary>
/// Represents the OpenID Connect / OAuth2 configuration bound from application settings.
/// </summary>
/// <example>
/// Bind from <c>appsettings.json</c>:
/// <code>
/// {
///   "OpenIdConnect": {
///     "Enabled": true,
///     "Authority": "http://localhost:8080/realms/hexagon",
///     "ClientId": "hexagon-app",
///     "ClientSecret": "...",
///     "Scopes": ["openid", "profile", "email"],
///     "RequiredClaims": ["sub", "email"]
///   }
/// }
/// </code>
/// </example>
internal sealed class OpenIdConnectConfiguration
{
    /// <summary>Gets or sets whether OpenID Connect authentication is enabled.</summary>
    public bool Enabled { get; init; }

    /// <summary>Gets or sets the authority URL (Keycloak realm base URL).</summary>
    [Required]
    public string Authority { get; init; } = string.Empty;

    /// <summary>Gets or sets the OAuth2 client identifier.</summary>
    [Required]
    public string ClientId { get; init; } = string.Empty;

    /// <summary>Gets or sets the OAuth2 client secret.</summary>
    public string? ClientSecret { get; init; }

    /// <summary>Gets or sets the OAuth2 scopes to request.</summary>
    public IReadOnlyList<string> Scopes { get; init; } = ["openid", "profile", "email"];

    /// <summary>Gets or sets the claim types that must be present in the identity token.</summary>
    public IReadOnlyList<string> RequiredClaims { get; init; } = ["sub", "email"];

    /// <summary>
    /// Validates that required fields are populated when <see cref="Enabled"/> is true.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required fields are missing.</exception>
    public void Validate()
    {
        if (!Enabled) return;

        if (string.IsNullOrWhiteSpace(Authority))
            throw new InvalidOperationException("OpenIdConnect:Authority must be set when OpenIdConnect is enabled.");

        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException("OpenIdConnect:ClientId must be set when OpenIdConnect is enabled.");
    }
}
