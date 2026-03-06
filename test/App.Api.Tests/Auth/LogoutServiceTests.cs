using App.Api.Auth;
using Xunit;

namespace App.Api.Tests.Auth;

public class LogoutServiceTests
{
    [Fact]
    public void BuildLogoutRedirectUri_WithValidAuthorityAndNoRedirect_ReturnsUri()
    {
        // Arrange
        var authority = "http://localhost:8080/realms/hexagon";

        // Act
        var uri = LogoutService.BuildLogoutRedirectUri(authority, null);

        // Assert
        Assert.Equal("http://localhost:8080/realms/hexagon/protocol/openid-connect/logout", uri.ToString());
    }

    [Fact]
    public void BuildLogoutRedirectUri_WithValidAuthorityAndRedirect_ReturnsUriWithQuery()
    {
        // Arrange
        var authority = "http://localhost:8080/realms/hexagon/";
        var redirectUri = "https://myapp.com/home";

        // Act
        var uri = LogoutService.BuildLogoutRedirectUri(authority, redirectUri);

        // Assert
        Assert.Equal("http://localhost:8080/realms/hexagon/protocol/openid-connect/logout?post_logout_redirect_uri=https%3A%2F%2Fmyapp.com%2Fhome", uri.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void BuildLogoutRedirectUri_WithInvalidAuthority_ThrowsArgumentException(string? authority)
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => LogoutService.BuildLogoutRedirectUri(authority!));
    }
}
