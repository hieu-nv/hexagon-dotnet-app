using System.Security.Claims;

namespace App.Api.Auth;

/// <summary>
/// Extension methods for resolving <see cref="AuthService"/> from the DI container
/// and using it to extract the current authenticated user.
/// </summary>
internal static class AuthServiceExtensions
{
    /// <summary>
    /// Resolves <see cref="AuthService"/> from the <paramref name="serviceProvider"/>
    /// and extracts an <see cref="AuthenticatedUser"/> from the given <paramref name="principal"/>.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider.</param>
    /// <param name="principal">The claims principal from the current HTTP context.</param>
    /// <returns>An <see cref="AuthenticatedUser"/> if the principal is valid; otherwise null.</returns>
    public static AuthenticatedUser? GetAuthenticatedUser(
        this IServiceProvider serviceProvider,
        ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(principal);

        var authService = serviceProvider.GetRequiredService<AuthService>();
        return authService.GetAuthenticatedUser(principal);
    }
}
