using Microsoft.AspNetCore.Authentication;

namespace App.Api.Auth;

/// <summary>
/// Extension methods for signing out of the current HTTP context.
/// </summary>
internal static class LogoutExtensions
{
    /// <summary>
    /// Signs the current user out, clearing cookies, and returns the Keycloak logout redirect URI.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="authority">The Keycloak authority base URL.</param>
    /// <param name="postLogoutRedirectUri">Optional URI to redirect to after Keycloak logout.</param>
    /// <returns>The Keycloak logout URI.</returns>
    public static async Task<Uri> LogoutAsync(
        this HttpContext httpContext,
        string authority,
        string? postLogoutRedirectUri = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        await httpContext.SignOutAsync().ConfigureAwait(false);

        foreach (var cookie in httpContext.Request.Cookies.Keys)
        {
            httpContext.Response.Cookies.Delete(cookie);
        }

        return LogoutService.BuildLogoutRedirectUri(authority, postLogoutRedirectUri);
    }
}

