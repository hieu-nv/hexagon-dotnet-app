namespace App.Api.Auth;

/// <summary>
/// Domain service for evaluating authorization decisions against named policies or role lists.
/// Delegates to the injected <see cref="IAuthorizationPolicyProvider"/>.
/// </summary>
internal sealed class AuthorizationService
{
    private readonly IAuthorizationPolicyProvider _policyProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthorizationService"/>.
    /// </summary>
    /// <param name="policyProvider">The policy provider to query for authorization decisions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policyProvider"/> is null.</exception>
    public AuthorizationService(IAuthorizationPolicyProvider policyProvider)
    {
        ArgumentNullException.ThrowIfNull(policyProvider);
        _policyProvider = policyProvider;
    }

    /// <summary>
    /// Determines whether the user satisfies the requirements of the named policy.
    /// </summary>
    /// <param name="user">The authenticated user to evaluate.</param>
    /// <param name="policyName">The name of the policy to evaluate against.</param>
    /// <returns>True if the user satisfies the policy; false if the policy is unknown or the user lacks required roles.</returns>
    public bool AuthorizeUser(AuthenticatedUser user, string policyName)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        return _policyProvider.AuthorizeUser(user, policyName);
    }

    /// <summary>
    /// Determines whether the user has at least one of the specified roles (OR logic).
    /// </summary>
    /// <param name="user">The authenticated user to evaluate.</param>
    /// <param name="requiredRoles">The collection of roles of which the user must have at least one.</param>
    /// <returns>True if the user holds any of the required roles; otherwise false.</returns>
    public bool AuthorizeByRoles(AuthenticatedUser user, IReadOnlyList<string> requiredRoles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(requiredRoles);

        return _policyProvider.AuthorizeByRoles(user, requiredRoles);
    }
}
