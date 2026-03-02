namespace App.Api.Auth;

/// <summary>
/// Represents an authorization policy requirement.
/// </summary>
public record AuthenticationPolicy(
    string Name,
    string? Description,
    IReadOnlyList<string> RequiredRoles);

/// <summary>
/// Defines the standard authorization policies used in the application.
/// </summary>
public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string UserAccess = "UserAccess";
}
