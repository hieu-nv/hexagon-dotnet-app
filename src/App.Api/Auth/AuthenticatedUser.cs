namespace App.Api.Auth;

/// <summary>
/// Represents an authenticated user extracted from OAuth2 access tokens.
/// </summary>
public record AuthenticatedUser(
    string Id,
    string Email,
    string? Name,
    IReadOnlyList<string> Roles,
    IReadOnlyDictionary<string, string> CustomClaims)
{
    public bool HasRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
