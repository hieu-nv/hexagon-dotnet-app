using System.Security.Claims;

namespace App.Api.Auth;

/// <summary>
/// Defines the contract for extracting claims from OAuth2 access tokens and constructing an AuthenticatedUser.
/// </summary>
public interface IClaimsExtractor
{
    /// <summary>
    /// Extracts an <see cref="AuthenticatedUser"/> from the given <see cref="ClaimsPrincipal"/>.
    /// </summary>
    /// <param name="principal">The incoming claims principal.</param>
    /// <returns>An <see cref="AuthenticatedUser"/> if successful, otherwise null.</returns>
    AuthenticatedUser? ExtractFromPrincipal(ClaimsPrincipal principal);

    /// <summary>
    /// Validates whether the given <see cref="ClaimsPrincipal"/> has the required claims to be processed.
    /// </summary>
    /// <param name="principal">The incoming claims principal.</param>
    /// <returns>True if the principal has all required claims, false otherwise.</returns>
    bool IsValidPrincipal(ClaimsPrincipal principal);
}
