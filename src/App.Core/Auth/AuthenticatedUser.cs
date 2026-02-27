namespace App.Core.Auth;

/// <summary>
/// Value object representing an authenticated user extracted from SAML assertions.
/// </summary>
public sealed class AuthenticatedUser
{
    /// <summary>
    /// Unique identifier for the user (typically email or NameID from SAML).
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User's display name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Roles assigned to the user in Keycloak.
    /// </summary>
    public IReadOnlyList<string> Roles { get; init; } = [];

    /// <summary>
    /// Additional custom claims from Keycloak (e.g., groups, attributes).
    /// </summary>
    public IReadOnlyDictionary<string, string> CustomClaims { get; init; } =
        new Dictionary<string, string>();

    /// <summary>
    /// Validates that the authenticated user has a specific role.
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if user has the role, false otherwise</returns>
    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates that the authenticated user has any of the specified roles.
    /// </summary>
    /// <param name="roles">The roles to check</param>
    /// <returns>True if user has at least one of the roles, false otherwise</returns>
    public bool HasAnyRole(params string[] roles)
    {
        return roles.Any(HasRole);
    }

    /// <summary>
    /// Validates that the authenticated user has all of the specified roles.
    /// </summary>
    /// <param name="roles">The roles to check</param>
    /// <returns>True if user has all of the roles, false otherwise</returns>
    public bool HasAllRoles(params string[] roles)
    {
        return roles.All(HasRole);
    }
}
