namespace App.Api.Auth;

/// <summary>
/// Default implementation of <see cref="IAuthorizationPolicyProvider"/> backed by the
/// <see cref="AuthorizationPoliciesRegistry"/> of predefined policies.
/// </summary>
internal sealed class DefaultAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    /// <inheritdoc />
    public AuthenticationPolicy? GetPolicy(string policyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        AuthorizationPoliciesRegistry.All.TryGetValue(policyName, out var policy);
        return policy;
    }

    /// <inheritdoc />
    public bool AuthorizeUser(AuthenticatedUser user, string policyName)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        var policy = GetPolicy(policyName);
        if (policy is null) return false;

        // A policy with no required roles allows any authenticated user.
        if (policy.RequiredRoles.Count == 0) return true;

        // User must have at least one of the required roles (OR logic).
        return policy.RequiredRoles.Any(role => user.HasRole(role));
    }

    /// <inheritdoc />
    public bool AuthorizeByRoles(AuthenticatedUser user, IReadOnlyList<string> requiredRoles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(requiredRoles);

        if (requiredRoles.Count == 0) return true;

        return requiredRoles.Any(role => user.HasRole(role));
    }
}
