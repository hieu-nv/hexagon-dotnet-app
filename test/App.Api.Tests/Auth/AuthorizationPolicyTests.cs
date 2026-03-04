using App.Api.Auth;

namespace App.Api.Tests.Auth;

public class AuthorizationPolicyTests
{
    private readonly DefaultAuthorizationPolicyProvider _provider = new();

    // ─── GetPolicy ────────────────────────────────────────────────────────────

    [Fact]
    public void GetPolicy_AdminOnly_ReturnsPolicy()
    {
        var policy = _provider.GetPolicy(AuthorizationPolicies.AdminOnly);
        Assert.NotNull(policy);
        Assert.Equal(AuthorizationPolicies.AdminOnly, policy.Name);
        Assert.Contains("admin", policy.RequiredRoles);
    }

    [Fact]
    public void GetPolicy_UserAccess_ReturnsPolicy()
    {
        var policy = _provider.GetPolicy(AuthorizationPolicies.UserAccess);
        Assert.NotNull(policy);
        Assert.Contains("user", policy.RequiredRoles);
        Assert.Contains("admin", policy.RequiredRoles);
    }

    [Fact]
    public void GetPolicy_TodoAccess_ReturnsPolicy()
    {
        var policy = _provider.GetPolicy(AuthorizationPolicies.TodoAccess);
        Assert.NotNull(policy);
        Assert.Contains("user", policy.RequiredRoles);
    }

    [Fact]
    public void GetPolicy_UnknownPolicy_ReturnsNull()
    {
        var policy = _provider.GetPolicy("NonExistentPolicy");
        Assert.Null(policy);
    }

    [Fact]
    public void GetPolicy_CaseInsensitive_ReturnsPolicy()
    {
        var policy = _provider.GetPolicy("adminonly");
        Assert.NotNull(policy);
    }

    // ─── AuthorizeUser by policy name ─────────────────────────────────────────

    [Fact]
    public void AuthorizeUser_AdminOnlyPolicy_WithAdminRole_ReturnsTrue()
    {
        var user = CreateUser(["admin"]);
        Assert.True(_provider.AuthorizeUser(user, AuthorizationPolicies.AdminOnly));
    }

    [Fact]
    public void AuthorizeUser_AdminOnlyPolicy_WithUserRole_ReturnsFalse()
    {
        var user = CreateUser(["user"]);
        Assert.False(_provider.AuthorizeUser(user, AuthorizationPolicies.AdminOnly));
    }

    [Fact]
    public void AuthorizeUser_UserAccessPolicy_WithUserRole_ReturnsTrue()
    {
        var user = CreateUser(["user"]);
        Assert.True(_provider.AuthorizeUser(user, AuthorizationPolicies.UserAccess));
    }

    [Fact]
    public void AuthorizeUser_UserAccessPolicy_WithAdminRole_ReturnsTrue()
    {
        var user = CreateUser(["admin"]);
        Assert.True(_provider.AuthorizeUser(user, AuthorizationPolicies.UserAccess));
    }

    [Fact]
    public void AuthorizeUser_UserAccessPolicy_WithNoRoles_ReturnsFalse()
    {
        var user = CreateUser([]);
        Assert.False(_provider.AuthorizeUser(user, AuthorizationPolicies.UserAccess));
    }

    [Fact]
    public void AuthorizeUser_UnknownPolicy_ReturnsFalse()
    {
        var user = CreateUser(["admin"]);
        Assert.False(_provider.AuthorizeUser(user, "UnknownPolicy"));
    }

    [Fact]
    public void AuthorizeUser_WithNullUser_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _provider.AuthorizeUser(null!, AuthorizationPolicies.AdminOnly));
    }

    // ─── AuthorizeByRoles ─────────────────────────────────────────────────────

    [Fact]
    public void AuthorizeByRoles_WithMatchingRole_ReturnsTrue()
    {
        var user = CreateUser(["admin", "user"]);
        Assert.True(_provider.AuthorizeByRoles(user, ["admin"]));
    }

    [Fact]
    public void AuthorizeByRoles_WithNoMatchingRole_ReturnsFalse()
    {
        var user = CreateUser(["user"]);
        Assert.False(_provider.AuthorizeByRoles(user, ["admin"]));
    }

    [Fact]
    public void AuthorizeByRoles_WithEmptyRequiredRoles_ReturnsTrue()
    {
        var user = CreateUser([]);
        Assert.True(_provider.AuthorizeByRoles(user, []));
    }

    [Fact]
    public void AuthorizeByRoles_WithNullUser_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _provider.AuthorizeByRoles(null!, ["admin"]));
    }

    // ─── AuthorizationHelpers ─────────────────────────────────────────────────

    [Fact]
    public void HasRole_MatchingRole_ReturnsTrue()
    {
        var user = CreateUser(["admin"]);
        Assert.True(AuthorizationHelpers.HasRole(user, "admin"));
    }

    [Fact]
    public void HasRole_CaseInsensitive_ReturnsTrue()
    {
        var user = CreateUser(["ADMIN"]);
        Assert.True(AuthorizationHelpers.HasRole(user, "admin"));
    }

    [Fact]
    public void HasRole_MissingRole_ReturnsFalse()
    {
        var user = CreateUser(["user"]);
        Assert.False(AuthorizationHelpers.HasRole(user, "admin"));
    }

    [Fact]
    public void HasAnyRole_UserHasOneOfRequiredRoles_ReturnsTrue()
    {
        var user = CreateUser(["user"]);
        Assert.True(AuthorizationHelpers.HasAnyRole(user, ["admin", "user"]));
    }

    [Fact]
    public void HasAnyRole_UserHasNoneOfRequiredRoles_ReturnsFalse()
    {
        var user = CreateUser(["viewer"]);
        Assert.False(AuthorizationHelpers.HasAnyRole(user, ["admin", "user"]));
    }

    [Fact]
    public void HasAllRoles_UserHasAllRoles_ReturnsTrue()
    {
        var user = CreateUser(["admin", "user"]);
        Assert.True(AuthorizationHelpers.HasAllRoles(user, ["admin", "user"]));
    }

    [Fact]
    public void HasAllRoles_UserMissingOneRole_ReturnsFalse()
    {
        var user = CreateUser(["user"]);
        Assert.False(AuthorizationHelpers.HasAllRoles(user, ["admin", "user"]));
    }

    // ─── AuthorizationPoliciesRegistry ───────────────────────────────────────

    [Fact]
    public void Registry_All_ContainsAllStandardPolicies()
    {
        Assert.Contains(AuthorizationPolicies.AdminOnly, AuthorizationPoliciesRegistry.All.Keys);
        Assert.Contains(AuthorizationPolicies.UserAccess, AuthorizationPoliciesRegistry.All.Keys);
        Assert.Contains(AuthorizationPolicies.TodoAccess, AuthorizationPoliciesRegistry.All.Keys);
    }

    [Fact]
    public void Registry_AdminOnly_HasDescription()
    {
        var policy = AuthorizationPoliciesRegistry.AdminOnly;
        Assert.NotNull(policy.Description);
        Assert.NotEmpty(policy.Description);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static AuthenticatedUser CreateUser(string[] roles) =>
        new("test-id", "test@example.com", "Test User", roles, new Dictionary<string, string>());
}
