namespace App.Api.Auth;

/// <summary>
/// Registry of standard authorization policies used throughout the application.
/// Each policy defines the role(s) required to gain access.
/// </summary>
internal static class AuthorizationPoliciesRegistry
{
    /// <summary>
    /// Only users with the <c>admin</c> role may access resources protected by this policy.
    /// </summary>
    public static readonly AuthenticationPolicy AdminOnly = new(
        Name: AuthorizationPolicies.AdminOnly,
        Description: "Requires the 'admin' role. Grants access to administrative operations.",
        RequiredRoles: ["admin"]);

    /// <summary>
    /// Users with either the <c>user</c> or <c>admin</c> role may access resources protected by this policy.
    /// </summary>
    public static readonly AuthenticationPolicy UserAccess = new(
        Name: AuthorizationPolicies.UserAccess,
        Description: "Requires the 'user' or 'admin' role. Grants access to standard user operations.",
        RequiredRoles: ["user", "admin"]);

    /// <summary>
    /// Users with either the <c>user</c> or <c>admin</c> role may access Todo domain resources.
    /// </summary>
    public static readonly AuthenticationPolicy TodoAccess = new(
        Name: AuthorizationPolicies.TodoAccess,
        Description: "Requires the 'user' or 'admin' role. Grants access to Todo CRUD operations.",
        RequiredRoles: ["user", "admin"]);

    /// <summary>
    /// Retrieves all registered policies indexed by name.
    /// </summary>
    public static IReadOnlyDictionary<string, AuthenticationPolicy> All { get; } =
        new Dictionary<string, AuthenticationPolicy>(StringComparer.OrdinalIgnoreCase)
        {
            [AdminOnly.Name] = AdminOnly,
            [UserAccess.Name] = UserAccess,
            [TodoAccess.Name] = TodoAccess,
        };
}
