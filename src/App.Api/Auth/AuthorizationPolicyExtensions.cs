using App.Api.Auth;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for configuring role-based authorization policies
/// in the ASP.NET Core authorization middleware.
/// </summary>
internal static class AuthorizationPolicyExtensions
{
    /// <summary>
    /// Registers the standard application authorization policies (AdminOnly, UserAccess, TodoAccess).
    /// </summary>
    public static AuthorizationOptions AddAppPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
            policy.RequireAssertion(ctx => EvaluateRoles(ctx, App.Api.ProgramConstants.AdminRoles)));

        options.AddPolicy(AuthorizationPolicies.UserAccess, policy =>
            policy.RequireAssertion(ctx => EvaluateRoles(ctx, App.Api.ProgramConstants.UserAdminRoles)));

        options.AddPolicy(AuthorizationPolicies.TodoAccess, policy =>
            policy.RequireAssertion(ctx => EvaluateRoles(ctx, App.Api.ProgramConstants.UserAdminRoles)));

        return options;
    }

    private static bool EvaluateRoles(
        AuthorizationHandlerContext ctx,
        string[] requiredRoles)
    {
        if (ctx.Resource is not HttpContext httpContext) return false;

        var extractor = httpContext.RequestServices.GetRequiredService<IClaimsExtractor>();
        var user = extractor.ExtractFromPrincipal(ctx.User);
        return user is not null && AuthService.AuthorizeByRoles(user, requiredRoles);
    }
}
