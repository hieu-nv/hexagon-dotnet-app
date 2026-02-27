namespace App.Core.Auth;

/// <summary>
/// Value object representing an authentication/authorization policy.
/// </summary>
public sealed class AuthenticationPolicy
{
    /// <summary>
    /// Policy name/identifier.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description of the policy.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Roles required to satisfy this policy (any of these roles is sufficient).
    /// </summary>
    public IReadOnlyList<string> RequiredRoles { get; init; } = [];

    /// <summary>
    /// Checks if an authenticated user satisfies this policy.
    /// </summary>
    /// <param name="user">The authenticated user</param>
    /// <returns>True if user satisfies the policy requirements, false otherwise</returns>
    public bool IsSatisfiedBy(AuthenticatedUser user)
    {
        if (RequiredRoles.Count == 0)
        {
            // No specific roles required - just needs to be authenticated
            return true;
        }

        return user.HasAnyRole(RequiredRoles.ToArray());
    }

    /// <summary>
    /// Creates an admin policy requiring the "admin" role.
    /// </summary>
    public static AuthenticationPolicy AdminOnly() =>
        new()
        {
            Name = "AdminOnly",
            Description = "Requires admin role",
            RequiredRoles = new[] { "admin" },
        };

    /// <summary>
    /// Creates an authenticated policy requiring only authentication (no specific roles).
    /// </summary>
    public static AuthenticationPolicy AuthenticatedOnly() =>
        new()
        {
            Name = "AuthenticatedOnly",
            Description = "Requires authentication",
            RequiredRoles = [],
        };
}
