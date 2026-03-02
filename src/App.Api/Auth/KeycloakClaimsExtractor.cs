using System.Security.Claims;
using System.Text.Json;

namespace App.Api.Auth;

/// <summary>
/// Extracts claims from a ClaimsPrincipal where roles are nested in a custom Keycloak structure (realm_access.roles).
/// </summary>
public class KeycloakClaimsExtractor : IClaimsExtractor
{
    private const string NameClaimType = "name";
    private const string PreferredUsernameClaimType = "preferred_username";

    public AuthenticatedUser? ExtractFromPrincipal(ClaimsPrincipal principal)
    {
        if (!IsValidPrincipal(principal))
        {
            return null;
        }

        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var email = principal.FindFirstValue(ClaimTypes.Email)!;
        var name = principal.FindFirstValue(NameClaimType) ?? principal.FindFirstValue(PreferredUsernameClaimType);

        var roles = ExtractRoles(principal);
        var customClaims = ExtractCustomClaims(principal);

        return new AuthenticatedUser(id, email, name, roles, customClaims);
    }

    public bool IsValidPrincipal(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // We require NameIdentifier (sub) and Email for a valid identity
        return principal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier) &&
               principal.HasClaim(c => c.Type == ClaimTypes.Email);
    }

    private static IReadOnlyList<string> ExtractRoles(ClaimsPrincipal principal)
    {
        var roles = new List<string>();

        // 1. Standard role claims (if mapped by Keycloak client scopes)
        roles.AddRange(principal.FindAll(ClaimTypes.Role).Select(c => c.Value));

        // 2. Keycloak realm_access claim
        var realmAccessClaim = principal.FindFirst("realm_access");
        if (realmAccessClaim != null)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var realmAccess = JsonSerializer.Deserialize<RealmAccess>(realmAccessClaim.Value, options);
                if (realmAccess?.Roles != null)
                {
                    roles.AddRange(realmAccess.Roles);
                }
            }
            catch (JsonException)
            {
                // Ignored: Ignore malformed realm_access json
            }
        }

        return roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static IReadOnlyDictionary<string, string> ExtractCustomClaims(ClaimsPrincipal principal)
    {
        // Add any non-standard claims to the dictionary
        var standardClaims = new HashSet<string>
        {
            ClaimTypes.NameIdentifier,
            ClaimTypes.Email,
            NameClaimType,
            PreferredUsernameClaimType,
            ClaimTypes.Role,
            "realm_access",
            "resource_access",
            "scope",
            "azp",
            "iss",
            "aud",
            "exp",
            "iat",
            "jti",
            "typ"
        };

        return principal.Claims
            .Where(c => !standardClaims.Contains(c.Type))
            .DistinctBy(c => c.Type)
            .ToDictionary(c => c.Type, c => c.Value);
    }

    private class RealmAccess
    {
        public string[]? Roles { get; set; }
    }
}
