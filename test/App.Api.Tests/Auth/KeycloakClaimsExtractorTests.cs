using System.Security.Claims;
using System.Text.Json;
using App.Api.Auth;

namespace App.Api.Tests.Auth;

public class KeycloakClaimsExtractorTests
{
    private readonly KeycloakClaimsExtractor _extractor = new();

    [Fact]
    public void IsValidPrincipal_ValidClaims_ReturnsTrue()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidPrincipal(principal);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidPrincipal_MissingEmail_ReturnsFalse()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidPrincipal(principal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidPrincipal_Unauthenticated_ReturnsFalse()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims); // No authentication type
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidPrincipal(principal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExtractFromPrincipal_ValidPrincipal_ExtractsAllClaims()
    {
        // Arrange
        var realmAccess = new { roles = new[] { "admin", "user" } };
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345"),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim("name", "Admin User"),
            new Claim("realm_access", JsonSerializer.Serialize(realmAccess)),
            new Claim("custom_department", "Engineering")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var user = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.NotNull(user);
        Assert.Equal("12345", user.Id);
        Assert.Equal("admin@example.com", user.Email);
        Assert.Equal("Admin User", user.Name);

        Assert.Contains("admin", user.Roles);
        Assert.Contains("user", user.Roles);
        Assert.Equal(2, user.Roles.Count);

        Assert.True(user.CustomClaims.ContainsKey("custom_department"));
        Assert.Equal("Engineering", user.CustomClaims["custom_department"]);
    }

    [Fact]
    public void ExtractFromPrincipal_StandardRoleClaims_ExtractsRoles()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.Role, "manager"),
            new Claim(ClaimTypes.Role, "employee")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var user = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.NotNull(user);
        Assert.Contains("manager", user.Roles);
        Assert.Contains("employee", user.Roles);
        Assert.Equal(2, user.Roles.Count);
    }

    [Fact]
    public void ExtractFromPrincipal_InvalidPrincipal_ReturnsNull()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345") // Missing email
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var user = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public void IsValidPrincipal_MissingNameIdentifier_ReturnsFalse()
    {
        // Arrange — email present but NameIdentifier missing
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "user@example.com")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidPrincipal(principal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidPrincipal_AuthenticatedButMissingEmail_ReturnsFalse()
    {
        // Arrange — NameIdentifier present but email absent
        // Covers the second operand of the && short-circuit: HasClaim(Email) => false
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = _extractor.IsValidPrincipal(principal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExtractFromPrincipal_RealmAccessWithNullRolesArray_ReturnsUserWithNoRoles()
    {
        // Arrange — realm_access JSON exists but "roles" key is null
        // Covers the realmAccess?.Roles != null branch in ExtractRoles
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Email, "nullroles@example.com"),
            new Claim("realm_access", "{\"roles\":null}") // roles field present but null
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var user = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.NotNull(user);
        Assert.Equal("42", user.Id);
        Assert.Empty(user.Roles);
    }

    [Fact]
    public void ExtractFromPrincipal_WithStandardRoleClaim_IncludesRoleFromClaimType()
    {
        // Arrange — role provided via ClaimTypes.Role (not realm_access)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "55"),
            new Claim(ClaimTypes.Email, "roleuser@example.com"),
            new Claim(ClaimTypes.Role, "editor")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var user = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.NotNull(user);
        Assert.Contains("editor", user.Roles);
    }

    [Fact]
    public void ExtractFromPrincipal_MalformedRealmAccessJson_IgnoresErrorAndReturnsUser()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "99"),
            new Claim(ClaimTypes.Email, "safe@example.com"),
            new Claim("realm_access", "THIS IS NOT VALID JSON")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act — should not throw despite malformed JSON
        var user = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.NotNull(user);
        Assert.Equal("99", user.Id);
        Assert.Empty(user.Roles); // No roles parsed from bad JSON
    }

    [Fact]
    public void ExtractFromPrincipal_WithPreferredUsernameFallback_UsesPreferredUsername()
    {
        // Arrange — "name" claim absent, but "preferred_username" is present
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "77"),
            new Claim(ClaimTypes.Email, "fallback@example.com"),
            new Claim("preferred_username", "jdoe")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var user = _extractor.ExtractFromPrincipal(principal);

        // Assert
        Assert.NotNull(user);
        Assert.Equal("jdoe", user.Name);
    }
}
