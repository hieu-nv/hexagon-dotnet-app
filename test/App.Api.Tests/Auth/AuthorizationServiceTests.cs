using App.Api.Auth;

namespace App.Api.Tests.Auth;

public class AuthorizationServiceTests
{
    private readonly DefaultAuthorizationPolicyProvider _policyProvider = new();
    private readonly AuthorizationService _authService;

    public AuthorizationServiceTests()
    {
        _authService = new AuthorizationService(_policyProvider);
    }

    // ─── Constructor ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithNullPolicyProvider_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthorizationService(null!));
    }

    // ─── AuthorizeUser ────────────────────────────────────────────────────────

    [Fact]
    public void AuthorizeUser_AdminPolicyAdminRole_ReturnsTrue()
    {
        var user = MakeUser(["admin"]);
        Assert.True(_authService.AuthorizeUser(user, AuthorizationPolicies.AdminOnly));
    }

    [Fact]
    public void AuthorizeUser_AdminPolicyUserRole_ReturnsFalse()
    {
        var user = MakeUser(["user"]);
        Assert.False(_authService.AuthorizeUser(user, AuthorizationPolicies.AdminOnly));
    }

    [Fact]
    public void AuthorizeUser_UserAccessPolicyAdminRole_ReturnsTrue()
    {
        var user = MakeUser(["admin"]);
        Assert.True(_authService.AuthorizeUser(user, AuthorizationPolicies.UserAccess));
    }

    [Fact]
    public void AuthorizeUser_UserAccessPolicyUserRole_ReturnsTrue()
    {
        var user = MakeUser(["user"]);
        Assert.True(_authService.AuthorizeUser(user, AuthorizationPolicies.UserAccess));
    }

    [Fact]
    public void AuthorizeUser_UserAccessPolicyNoRoles_ReturnsFalse()
    {
        var user = MakeUser([]);
        Assert.False(_authService.AuthorizeUser(user, AuthorizationPolicies.UserAccess));
    }

    [Fact]
    public void AuthorizeUser_MultipleRolesUserHasAny_ReturnsTrue()
    {
        // User has "moderator" not "admin", but UserAccess policy allows "user" or "admin"
        var user = MakeUser(["user", "moderator"]);
        Assert.True(_authService.AuthorizeUser(user, AuthorizationPolicies.UserAccess));
    }

    [Fact]
    public void AuthorizeUser_UnknownPolicyName_ReturnsFalse()
    {
        var user = MakeUser(["admin"]);
        Assert.False(_authService.AuthorizeUser(user, "GhostPolicy"));
    }

    [Fact]
    public void AuthorizeUser_NullUser_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _authService.AuthorizeUser(null!, AuthorizationPolicies.AdminOnly));
    }

    [Fact]
    public void AuthorizeUser_EmptyPolicyName_ThrowsArgumentException()
    {
        var user = MakeUser(["admin"]);
        Assert.Throws<ArgumentException>(() => _authService.AuthorizeUser(user, ""));
    }

    // ─── AuthorizeByRoles ─────────────────────────────────────────────────────

    [Fact]
    public void AuthorizeByRoles_UserHasOneOfRequired_ReturnsTrue()
    {
        var user = MakeUser(["user"]);
        Assert.True(_authService.AuthorizeByRoles(user, ["admin", "user"]));
    }

    [Fact]
    public void AuthorizeByRoles_UserHasNoneOfRequired_ReturnsFalse()
    {
        var user = MakeUser(["viewer"]);
        Assert.False(_authService.AuthorizeByRoles(user, ["admin", "user"]));
    }

    [Fact]
    public void AuthorizeByRoles_EmptyRequiredRoles_ReturnsTrue()
    {
        var user = MakeUser([]);
        Assert.True(_authService.AuthorizeByRoles(user, []));
    }

    [Fact]
    public void AuthorizeByRoles_CaseInsensitiveRoleMatch_ReturnsTrue()
    {
        var user = MakeUser(["Admin"]); // Capital A
        Assert.True(_authService.AuthorizeByRoles(user, ["admin"])); // Lowercase
    }

    [Fact]
    public void AuthorizeByRoles_NullUser_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _authService.AuthorizeByRoles(null!, ["admin"]));
    }

    [Fact]
    public void AuthorizeByRoles_NullRequiredRoles_ThrowsArgumentNullException()
    {
        var user = MakeUser(["admin"]);
        Assert.Throws<ArgumentNullException>(() =>
            _authService.AuthorizeByRoles(user, null!));
    }

    // ─── Helper ──────────────────────────────────────────────────────────────

    private static AuthenticatedUser MakeUser(string[] roles) =>
        new("id-1", "a@b.com", "Test", roles, new Dictionary<string, string>());
}
