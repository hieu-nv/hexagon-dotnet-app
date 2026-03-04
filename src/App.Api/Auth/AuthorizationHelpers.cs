namespace App.Api.Auth;

/// <summary>
/// Static helper methods for role-based authorization checks against an <see cref="AuthenticatedUser"/>.
/// All comparisons are case-insensitive.
/// </summary>
internal static class AuthorizationHelpers
{
    /// <summary>
    /// Returns true if the user has the specified role (case-insensitive).
    /// </summary>
    /// <param name="user">The user to inspect.</param>
    /// <param name="role">The role name to check for.</param>
    public static bool HasRole(AuthenticatedUser user, string role)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        return user.HasRole(role);
    }

    /// <summary>
    /// Returns true if the user has at least one of the specified roles (OR logic, case-insensitive).
    /// </summary>
    /// <param name="user">The user to inspect.</param>
    /// <param name="roles">The candidate roles of which the user must have at least one.</param>
    public static bool HasAnyRole(AuthenticatedUser user, IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);

        return roles.Any(role => user.HasRole(role));
    }

    /// <summary>
    /// Returns true if the user has all of the specified roles (AND logic, case-insensitive).
    /// </summary>
    /// <param name="user">The user to inspect.</param>
    /// <param name="roles">The roles that the user must all possess.</param>
    public static bool HasAllRoles(AuthenticatedUser user, IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);

        return roles.All(role => user.HasRole(role));
    }
}
