using System.Security.Claims;
using App.Core.Auth;
using Moq;
using Xunit;

namespace App.Core.Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<ISamlClaimsExtractor> _extractorMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _extractorMock = new Mock<ISamlClaimsExtractor>();
        _service = new AuthService(_extractorMock.Object);
    }

    [Fact]
    public void Constructor_WhenExtractorIsNull_ThrowsArgumentNullException()
    {
#pragma warning disable CS8625
        _ = Assert.Throws<ArgumentNullException>(() => new AuthService(null));
#pragma warning restore CS8625
    }

    [Fact]
    public void GetAuthenticatedUser_WhenPrincipalIsNull_ThrowsArgumentNullException()
    {
#pragma warning disable CS8625
        _ = Assert.Throws<ArgumentNullException>(() => _service.GetAuthenticatedUser(null));
#pragma warning restore CS8625
    }

    [Fact]
    public void GetAuthenticatedUser_WhenInvalidPrincipal_ReturnsNull()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        _extractorMock.Setup(x => x.IsValidSamlPrincipal(principal)).Returns(false);

        // Act
        var result = _service.GetAuthenticatedUser(principal);

        // Assert
        Assert.Null(result);
        _extractorMock.Verify(x => x.IsValidSamlPrincipal(principal), Times.Once);
        _extractorMock.Verify(
            x => x.ExtractFromPrincipal(It.IsAny<ClaimsPrincipal>()),
            Times.Never
        );
    }

    [Fact]
    public void GetAuthenticatedUser_WhenValidPrincipal_ReturnsExtractedUser()
    {
        // Arrange
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "user@example.com") }, "SAML2")
        );
        var expectedUser = new AuthenticatedUser { Id = "user-id", Email = "user@example.com" };
        _extractorMock.Setup(x => x.IsValidSamlPrincipal(principal)).Returns(true);
        _extractorMock.Setup(x => x.ExtractFromPrincipal(principal)).Returns(expectedUser);

        // Act
        var result = _service.GetAuthenticatedUser(principal);

        // Assert
        Assert.Same(expectedUser, result);
    }

    [Fact]
    public void IsAuthenticated_WhenNotAuthenticated_ReturnsFalse()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = _service.IsAuthenticated(principal);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_WhenAuthenticated_ReturnsTrue()
    {
        // Arrange
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "user@example.com") }, "SAML2")
        );

        // Act
        var result = _service.IsAuthenticated(principal);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Authorize_WithNullUser_ThrowsArgumentNullException()
    {
        // Arrange
        var policy = AuthenticationPolicy.AuthenticatedOnly();

        // Act & Assert
#pragma warning disable CS8625
        _ = Assert.Throws<ArgumentNullException>(() => _service.Authorize(null, policy));
#pragma warning restore CS8625
    }

    [Fact]
    public void Authorize_WithNullPolicy_ThrowsArgumentNullException()
    {
        // Arrange
        var user = new AuthenticatedUser { Id = "user-id", Email = "user@example.com" };

        // Act & Assert
#pragma warning disable CS8625
        _ = Assert.Throws<ArgumentNullException>(() => _service.AuthorizeUser(user, null!));
#pragma warning restore CS8625
    }

    [Fact]
    public void Authorize_WhenUserSatisfiesPolicy_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["admin"],
        };
        var policy = AuthenticationPolicy.AdminOnly();

        // Act
        var result = _service.AuthorizeUser(user, policy);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Authorize_WhenUserDoesNotSatisfyPolicy_ReturnsFalse()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["user"],
        };
        var policy = AuthenticationPolicy.AdminOnly();

        // Act
        var result = _service.AuthorizeUser(user, policy);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AuthorizeByRoles_WhenUserHasRole_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["user"],
        };

        // Act
        var result = _service.AuthorizeByRoles(user, "admin", "user");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AuthorizeByRoles_WhenUserLacksRole_ReturnsFalse()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["guest"],
        };

        // Act
        var result = _service.AuthorizeByRoles(user, "admin", "user");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AuthorizeByRoles_WithNoRequiredRoles_ReturnsTrue()
    {
        // Arrange
        var user = new AuthenticatedUser
        {
            Id = "user-id",
            Email = "user@example.com",
            Roles = ["guest"],
        };

        // Act
        var result = _service.AuthorizeByRoles(user);

        // Assert
        Assert.True(result);
    }
}
