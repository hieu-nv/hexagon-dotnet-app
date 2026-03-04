using App.Api.Auth;
using App.Api.Admin;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for registering and mapping authentication and authorization endpoints.
/// </summary>
internal static class AuthEndpointsExtensions
{
    /// <summary>
    /// Registers the auth endpoint handlers in the DI container.
    /// </summary>
    public static WebApplicationBuilder UseAppAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<AuthStatusEndpoints>();
        builder.Services.AddScoped<AuthInfoEndpoints>();
        builder.Services.AddScoped<AuthLogoutEndpoints>();
        builder.Services.AddScoped<AdminEndpoints>();
        builder.Services.AddScoped<App.Api.Auth.AuthorizationService>();
        builder.Services.AddScoped<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
        return builder;
    }

    /// <summary>
    /// Maps the <c>/auth/*</c> and <c>/admin/*</c> endpoint routes.
    /// </summary>
    public static IEndpointRouteBuilder UseAppAuth(
        this IEndpointRouteBuilder endpoints,
        Asp.Versioning.Builder.ApiVersionSet apiVersionSet)
    {
        // Auth group — open for status/me/logout
        var authGroup = endpoints
            .MapGroup("/auth")
            .WithTags("Auth");

        authGroup
            .MapGet("/status",
                (AuthStatusEndpoints handler, System.Security.Claims.ClaimsPrincipal user)
                    => handler.GetAuthStatus(user))
            .WithName("GetAuthStatus")
            .WithSummary("Returns the current authentication status")
            .Produces<AuthStatusResponse>(200)
            .Produces(401);

        authGroup
            .MapGet("/me",
                (AuthInfoEndpoints handler, System.Security.Claims.ClaimsPrincipal user)
                    => handler.GetCurrentUser(user))
            .WithName("GetCurrentUser")
            .WithSummary("Returns the current authenticated user profile")
            .Produces<AuthMeResponse>(200)
            .Produces(401)
            .RequireAuthorization();

        authGroup
            .MapPost("/logout",
                (AuthLogoutEndpoints handler,
                 System.Security.Claims.ClaimsPrincipal user,
                 HttpContext ctx)
                    => handler.LogoutAsync(user, ctx))
            .WithName("Logout")
            .WithSummary("Logs out the current user and returns the Keycloak logout URL")
            .Produces<AuthLogoutResponse>(200)
            .Produces(401)
            .RequireAuthorization();

        // Admin group — requires AdminOnly policy
        var adminGroup = endpoints
            .MapGroup("/admin")
            .WithTags("Admin")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        adminGroup
            .MapGet("/stats", (AdminEndpoints handler) => handler.GetStats())
            .WithName("GetAdminStats")
            .WithSummary("Returns system statistics (admin only)")
            .Produces<AdminStatsResponse>(200)
            .Produces(401)
            .Produces(403);

        return endpoints;
    }
}
