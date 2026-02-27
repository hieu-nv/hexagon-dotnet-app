using System.Security.Claims;

namespace App.Core.Auth;

/// <summary>
/// Service for managing authentication and authorization.
/// </summary>
/// <param name="claimsExtractor">Port for extracting SAML claims</param>
public class AuthService
{
    private readonly ISamlClaimsExtractor _claimsExtractor;

    public AuthService(ISamlClaimsExtractor claimsExtractor)
    {
        ArgumentNullException.ThrowIfNull(claimsExtractor);
        _claimsExtractor = claimsExtractor;
    }

    /// <summary>
    /// Extracts authenticated user from a ClaimsPrincipal (from SAML assertion).
    /// </summary>
    /// <param name="claimsPrincipal">The ClaimsPrincipal to extract from</param>
    /// <returns>Extracted authenticated user or null if extraction fails</returns>
    public AuthenticatedUser? GetAuthenticatedUser(ClaimsPrincipal claimsPrincipal)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);

        if (!_claimsExtractor.IsValidSamlPrincipal(claimsPrincipal))
        {
            return null;
        }

        return _claimsExtractor.ExtractFromPrincipal(claimsPrincipal);
    }

    /// <summary>
    /// Checks if a ClaimsPrincipal is properly authenticated via SAML.
    /// </summary>
    /// <param name="claimsPrincipal">The ClaimsPrincipal to check</param>
    /// <returns>True if authenticated, false otherwise</returns>
    public bool IsAuthenticated(ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Authorizes an authenticated user against a policy.
    /// </summary>
    /// <param name="user">The authenticated user</param>
    /// <param name="policy">The authorization policy to check</param>
    /// <returns>True if user is authorized, false otherwise</returns>
    public bool AuthorizeUser(AuthenticatedUser user, AuthenticationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(policy);
        return policy.IsSatisfiedBy(user);
    }

    /// <summary>
    /// Authorizes an authenticated user against a list of required roles.
    /// </summary>
    /// <param name="user">The authenticated user</param>
    /// <param name="requiredRoles">Roles that user must have at least one of</param>
    /// <returns>True if user has at least one of the required roles, false otherwise</returns>
    public bool AuthorizeByRoles(AuthenticatedUser user, params string[] requiredRoles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(requiredRoles);

        if (requiredRoles.Length == 0)
        {
            // No specific roles required
            return true;
        }

        return user.HasAnyRole(requiredRoles);
    }

    /// <summary>
    /// Authorizes using ClaimsPrincipal and policy.
    /// </summary>
    /// <param name="claimsPrincipal">The ClaimsPrincipal to authorize</param>
    /// <param name="policy">The authorization policy to check</param>
    /// <returns>True if authorized, false otherwise</returns>
    public bool Authorize(ClaimsPrincipal claimsPrincipal, AuthenticationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);
        ArgumentNullException.ThrowIfNull(policy);

        var user = GetAuthenticatedUser(claimsPrincipal);
        return user is not null && policy.IsSatisfiedBy(user);
    }

    /// <summary>
    /// Authorizes using ClaimsPrincipal and roles.
    /// </summary>
    /// <param name="claimsPrincipal">The ClaimsPrincipal to authorize</param>
    /// <param name="requiredRoles">Roles that user must have at least one of</param>
    /// <returns>True if authorized, false otherwise</returns>
    public bool AuthorizeByRoles(ClaimsPrincipal claimsPrincipal, params string[] requiredRoles)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);
        ArgumentNullException.ThrowIfNull(requiredRoles);

        var user = GetAuthenticatedUser(claimsPrincipal);
        return user is not null && user.HasAnyRole(requiredRoles);
    }
}
