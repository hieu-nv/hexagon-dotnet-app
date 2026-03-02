using System.Security.Claims;

namespace App.Api.Auth;

/// <summary>
/// Domain service for evaluating authentication and authorization state in the application.
/// </summary>
public class AuthService
{
    private readonly IClaimsExtractor _claimsExtractor;

    public AuthService(IClaimsExtractor claimsExtractor)
    {
        ArgumentNullException.ThrowIfNull(claimsExtractor);
        _claimsExtractor = claimsExtractor;
    }

    /// <summary>
    /// Checks if the provided principal represents a valid authenticated user.
    /// </summary>
    public bool IsAuthenticated(ClaimsPrincipal principal)
    {
        return _claimsExtractor.IsValidPrincipal(principal);
    }

    /// <summary>
    /// Gets the <see cref="AuthenticatedUser"/> from the given claims principal.
    /// </summary>
    public AuthenticatedUser? GetAuthenticatedUser(ClaimsPrincipal principal)
    {
        return _claimsExtractor.ExtractFromPrincipal(principal);
    }

    /// <summary>
    /// Checks if the authenticated user satisfies the given policy.
    /// Note: Endpoint authorization is handled by ASP.NET Core middleware, so this
    /// is purely for domain-level authorization checks.
    /// </summary>
    public bool AuthorizeUser(AuthenticatedUser user, AuthenticationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(policy);

        // An empty requirements list means no specific roles are needed, just authenticated.
        if (!policy.RequiredRoles.Any()) return true;

        // For this simple implementation: User must have ANY of the required roles to satisfy policy.
        return policy.RequiredRoles.Any(role => user.HasRole(role));
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    public bool AuthorizeByRoles(AuthenticatedUser user, IEnumerable<string> requiredRoles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(requiredRoles);

        return requiredRoles.Any(role => user.HasRole(role));
    }
}
