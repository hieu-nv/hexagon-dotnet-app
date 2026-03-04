using System.Security.Claims;

namespace App.Api.Auth;

/// <summary>
/// Endpoint handler for <c>GET /auth/status</c>.
/// Returns the current authentication status of the calling user.
/// </summary>
internal sealed class AuthStatusEndpoints(AuthService authService, ILogger<AuthStatusEndpoints> logger)
{
    private readonly AuthService _authService =
        authService ?? throw new ArgumentNullException(nameof(authService));

    private readonly ILogger<AuthStatusEndpoints> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Returns authentication status for the current user.
    /// Returns <c>200 OK</c> with user details if authenticated; <c>401</c> otherwise.
    /// </summary>
    public IResult GetAuthStatus(ClaimsPrincipal user)
    {
        _logger.LogInformation("Auth status check requested");

        var authenticatedUser = _authService.GetAuthenticatedUser(user);
        if (authenticatedUser is null)
        {
            _logger.LogInformation("Auth status check: unauthenticated");
            return Results.Unauthorized();
        }

        _logger.LogInformation(
            "Auth status check: authenticated as {Email} with roles [{Roles}]",
            authenticatedUser.Email,
            string.Join(", ", authenticatedUser.Roles));

        return Results.Ok(new AuthStatusResponse(
            IsAuthenticated: true,
            UserId: authenticatedUser.Id,
            Email: authenticatedUser.Email,
            Name: authenticatedUser.Name,
            Roles: authenticatedUser.Roles));
    }
}

/// <summary>Response DTO for <c>GET /auth/status</c>.</summary>
internal sealed record AuthStatusResponse(
    bool IsAuthenticated,
    string UserId,
    string Email,
    string? Name,
    IReadOnlyList<string> Roles);
