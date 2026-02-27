using App.Core.Auth;
using Microsoft.AspNetCore.Authorization;

namespace App.Api.Auth;

/// <summary>
/// Extension methods for configuring SAML2-based authorization policies.
/// </summary>
public static class AuthorizeExtensions
{
    /// <summary>
    /// Adds SAML2-based authorization policies to the application.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddSaml2AuthorizationPolicies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddAuthorizationBuilder()
            // Policy for authenticated users only (any role allowed)
            .AddPolicy(
                "Authenticated",
                builder =>
                {
                    builder.RequireAuthenticatedUser();
                }
            )
            // Policy for admin users only
            .AddPolicy(
                "AdminOnly",
                builder =>
                {
                    builder.RequireAuthenticatedUser().RequireRole("admin");
                }
            )
            // Policy for todo service endpoints
            .AddPolicy(
                "TodoAccess",
                builder =>
                {
                    builder.RequireAuthenticatedUser().RequireRole("admin", "user");
                }
            );

        return services;
    }
}
