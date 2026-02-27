using System.Security.Claims;

namespace App.Core.Auth;

/// <summary>
/// Port for extracting SAML claims from a ClaimsPrincipal.
/// </summary>
public interface ISamlClaimsExtractor
{
    /// <summary>
    /// Extracts user identity claims from a SAML assertion (ClaimsPrincipal).
    /// </summary>
    /// <param name="claimsPrincipal">The ClaimsPrincipal from SAML assertion</param>
    /// <returns>Extracted user identity or null if extraction fails</returns>
    AuthenticatedUser? ExtractFromPrincipal(ClaimsPrincipal claimsPrincipal);

    /// <summary>
    /// Validates that the ClaimsPrincipal contains required claims for authentication.
    /// </summary>
    /// <param name="claimsPrincipal">The ClaimsPrincipal to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValidSamlPrincipal(ClaimsPrincipal claimsPrincipal);
}
