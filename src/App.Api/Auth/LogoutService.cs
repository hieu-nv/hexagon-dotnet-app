namespace App.Api.Auth;

/// <summary>
/// Provides methods for building Keycloak logout redirect URIs.
/// </summary>
internal static class LogoutService
{
    /// <summary>
    /// Builds the Keycloak logout endpoint URL including an optional post-logout redirect URI.
    /// </summary>
    /// <param name="authority">The Keycloak realm authority base URL (e.g. <c>http://localhost:8080/realms/hexagon</c>).</param>
    /// <param name="postLogoutRedirectUri">
    /// The URI to redirect the browser to after Keycloak completes logout. May be null.
    /// </param>
    /// <returns>The fully formed logout redirect URI.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="authority"/> is null or whitespace.</exception>
    public static Uri BuildLogoutRedirectUri(string authority, string? postLogoutRedirectUri = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authority);

        var logoutPath = $"{authority.TrimEnd('/')}/protocol/openid-connect/logout";

        if (string.IsNullOrWhiteSpace(postLogoutRedirectUri))
        {
            return new Uri(logoutPath);
        }

        var encoded = Uri.EscapeDataString(postLogoutRedirectUri);
        return new Uri($"{logoutPath}?post_logout_redirect_uri={encoded}");
    }
}
