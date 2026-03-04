using System.Security.Claims;

namespace App.Api.Auth;

/// <summary>
/// Endpoint handler for <c>GET /auth/me</c>.
/// Returns the current authenticated user's identity information extracted from OAuth2 claims.
/// </summary>
internal sealed class AuthInfoEndpoints(AuthService authService, ILogger<AuthInfoEndpoints> logger)
{
    private readonly AuthService _authService =
        authService ?? throw new ArgumentNullException(nameof(authService));

    private readonly ILogger<AuthInfoEndpoints> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Returns the current authenticated user's profile.
    /// Returns <c>200 OK</c> with user details; <c>401</c> if not authenticated.
    /// </summary>
    public IResult GetCurrentUser(ClaimsPrincipal user)
    {
        _logger.LogInformation("GET /auth/me requested");

        var authenticatedUser = _authService.GetAuthenticatedUser(user);
        if (authenticatedUser is null)
        {
            _logger.LogInformation("GET /auth/me: unauthenticated request");
            return Results.Unauthorized();
        }

        _logger.LogInformation(
            "GET /auth/me: returning profile for {Email}",
            authenticatedUser.Email);

        return Results.Ok(new AuthMeResponse(
            Id: authenticatedUser.Id,
            Email: authenticatedUser.Email,
            Name: authenticatedUser.Name,
            Roles: authenticatedUser.Roles,
            CustomClaims: authenticatedUser.CustomClaims));
    }
}

/// <summary>Response DTO for <c>GET /auth/me</c>.</summary>
internal sealed record AuthMeResponse(
    string Id,
    string Email,
    string? Name,
    IReadOnlyList<string> Roles,
    IReadOnlyDictionary<string, string> CustomClaims);
