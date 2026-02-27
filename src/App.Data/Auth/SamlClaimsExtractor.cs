using System.Security.Claims;
using App.Core.Auth;

namespace App.Data.Auth;

/// <summary>
/// SAML 2.0 claims extractor implementation.
/// Extracts user identity and claims from SAML assertions contained in ClaimsPrincipal.
/// </summary>
public class SamlClaimsExtractor : ISamlClaimsExtractor
{
    private const string EmailClaimType = ClaimTypes.Email;
    private const string NameClaimType = ClaimTypes.Name;
    private const string NameIdClaimType = ClaimTypes.NameIdentifier;
    private const string RoleClaimType = ClaimTypes.Role;

    /// <summary>
    /// Extracts user identity claims from a SAML assertion (ClaimsPrincipal).
    /// </summary>
    /// <param name="claimsPrincipal">The ClaimsPrincipal from SAML assertion</param>
    /// <returns>Extracted user identity or null if extraction fails</returns>
    public AuthenticatedUser? ExtractFromPrincipal(ClaimsPrincipal claimsPrincipal)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);

        if (!claimsPrincipal.Identity?.IsAuthenticated ?? true)
        {
            return null;
        }

        // Extract required claims
        var email = claimsPrincipal.FindFirst(EmailClaimType)?.Value;
        var nameId = claimsPrincipal.FindFirst(NameIdClaimType)?.Value;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nameId))
        {
            return null;
        }

        // Extract optional name claim
        var name = claimsPrincipal.FindFirst(NameClaimType)?.Value;

        // Extract all roles
        var roles = claimsPrincipal
            .FindAll(RoleClaimType)
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToList();

        // Extract custom claims (all claims except standard ones)
        var customClaims = ExtractCustomClaims(claimsPrincipal);

        return new AuthenticatedUser
        {
            Id = nameId,
            Email = email,
            Name = name,
            Roles = roles,
            CustomClaims = customClaims,
        };
    }

    /// <summary>
    /// Validates that the ClaimsPrincipal contains required claims for authentication.
    /// </summary>
    /// <param name="claimsPrincipal">The ClaimsPrincipal to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValidSamlPrincipal(ClaimsPrincipal claimsPrincipal)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);

        if (!claimsPrincipal.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        // Check for required claims
        var hasEmail = claimsPrincipal.FindFirst(EmailClaimType) is not null;
        var hasNameId = claimsPrincipal.FindFirst(NameIdClaimType) is not null;

        return hasEmail && hasNameId;
    }

    /// <summary>
    /// Extracts custom claims from ClaimsPrincipal (those not in standard claim types).
    /// </summary>
    private static IReadOnlyDictionary<string, string> ExtractCustomClaims(
        ClaimsPrincipal claimsPrincipal
    )
    {
        var standardClaimTypes = new[]
        {
            EmailClaimType,
            NameClaimType,
            NameIdClaimType,
            RoleClaimType,
            "iss", // issuer
            "sub", // subject
            "aud", // audience
            "iat", // issued at
            "exp", // expiration
            "nonce",
        };

        return claimsPrincipal
            .Claims.Where(c =>
                !standardClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase)
            )
            .DistinctBy(c => c.Type)
            .ToDictionary(c => c.Type, c => c.Value)
            .AsReadOnly();
    }
}
