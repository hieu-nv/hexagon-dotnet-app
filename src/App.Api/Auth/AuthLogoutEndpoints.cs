using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace App.Api.Auth;

/// <summary>
/// Endpoint handler for <c>POST /auth/logout</c>.
/// Clears authentication session and redirects the client to the Keycloak logout endpoint.
/// </summary>
internal sealed class AuthLogoutEndpoints(
    IConfiguration configuration,
    ILogger<AuthLogoutEndpoints> logger)
{
    private readonly IConfiguration _configuration =
        configuration ?? throw new ArgumentNullException(nameof(configuration));

    private readonly ILogger<AuthLogoutEndpoints> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Logs out the currently authenticated user and returns the Keycloak logout URL.
    /// The client should redirect the browser to the returned URL to complete the SSO logout.
    /// Returns <c>200 OK</c> with the logout redirect URL; <c>401</c> if not authenticated.
    /// </summary>
    public async Task<IResult> LogoutAsync(ClaimsPrincipal user, HttpContext httpContext)
    {
        _logger.LogInformation("Logout requested");

        if (user.Identity?.IsAuthenticated != true)
        {
            _logger.LogInformation("Logout: not authenticated");
            return Results.Unauthorized();
        }

        var authority = _configuration["JwtBearer:Authority"]
            ?? _configuration["OpenIdConnect:Authority"]
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(authority))
        {
            _logger.LogWarning("Logout: authority not configured, performing local sign-out only");
            await TrySignOutAsync(httpContext).ConfigureAwait(false);
            return Results.Ok(new AuthLogoutResponse(LoggedOut: true, LogoutUrl: null));
        }

        var postLogoutRedirectUri = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/";
        var logoutUri = LogoutService.BuildLogoutRedirectUri(authority, postLogoutRedirectUri);

        // JWT Bearer is stateless — SignOutAsync may throw; ignore the error and clear cookies.
        await TrySignOutAsync(httpContext).ConfigureAwait(false);
        foreach (var cookie in httpContext.Request.Cookies.Keys)
        {
            httpContext.Response.Cookies.Delete(cookie);
        }

        _logger.LogInformation("Logout successful, redirecting to {LogoutUrl}", logoutUri);

        return Results.Ok(new AuthLogoutResponse(LoggedOut: true, LogoutUrl: logoutUri.ToString()));
    }

    private async Task TrySignOutAsync(HttpContext httpContext)
    {
        try
        {
            await httpContext.SignOutAsync().ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            // JwtBearer is stateless and does not support SignOut — this is expected.
            _logger.LogDebug(ex, "SignOutAsync skipped (stateless JWT bearer scheme)");
        }
    }
}

/// <summary>Response DTO for <c>POST /auth/logout</c>.</summary>
internal sealed record AuthLogoutResponse(
    bool LoggedOut,
    string? LogoutUrl);
