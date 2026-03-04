using System.Security.Claims;

namespace App.Api.Auth;

/// <summary>
/// Defines the contract for retrieving and evaluating authorization policies.
/// </summary>
internal interface IAuthorizationPolicyProvider
{
    /// <summary>
    /// Retrieves a named authorization policy.
    /// </summary>
    /// <param name="policyName">The name of the policy to look up.</param>
    /// <returns>The <see cref="AuthenticationPolicy"/> if found; otherwise null.</returns>
    AuthenticationPolicy? GetPolicy(string policyName);

    /// <summary>
    /// Determines whether the given user is authorized under the specified named policy.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="policyName">The policy name to evaluate.</param>
    /// <returns>True if the user satisfies the policy; otherwise false.</returns>
    bool AuthorizeUser(AuthenticatedUser user, string policyName);

    /// <summary>
    /// Determines whether the given user has at least one of the specified roles (OR logic).
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="requiredRoles">The list of roles of which the user must have at least one.</param>
    /// <returns>True if the user has any of the required roles; otherwise false.</returns>
    bool AuthorizeByRoles(AuthenticatedUser user, IReadOnlyList<string> requiredRoles);
}
