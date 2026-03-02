namespace App.Api.Middleware;

/// <summary>
/// Middleware that adds security-related HTTP response headers.
/// </summary>
public class SecurityHeadersMiddleware : IMiddleware
{
    /// <summary>
    /// Adds security headers to the response and invokes the next middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";

        await next(context).ConfigureAwait(false);
    }
}
