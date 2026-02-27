namespace App.Api.Auth;

/// <summary>
/// Configuration options for SAML 2.0 authentication.
/// </summary>
public sealed class Saml2Options
{
    /// <summary>
    /// Keycloak realm name.
    /// </summary>
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// Keycloak server base URL (e.g., http://localhost:8080).
    /// </summary>
    public string KeycloakUrl { get; set; } = string.Empty;

    /// <summary>
    /// SAML client ID registered in Keycloak.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Application's Assertion Consumer Service (ACS) URL.
    /// </summary>
    public string AssertionConsumerServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Application's Single Logout Service (SLS) URL.
    /// </summary>
    public string SingleLogoutServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Whether to validate SSL certificates (disable for development with self-signed certs).
    /// </summary>
    public bool ValidateSslCertificate { get; set; } = true;

    /// <summary>
    /// Gets the Keycloak SAML metadata URL.
    /// </summary>
    public string GetMetadataUrl()
    {
        return $"{KeycloakUrl}/realms/{Realm}/protocol/saml/descriptor";
    }

    /// <summary>
    /// Gets the Keycloak SAML SSO endpoint.
    /// </summary>
    public string GetSsoUrl()
    {
        return $"{KeycloakUrl}/realms/{Realm}/protocol/saml";
    }
}
