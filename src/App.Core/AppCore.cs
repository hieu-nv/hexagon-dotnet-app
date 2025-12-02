using App.Core.Todo;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides methods to configure the application core, including services for managing to-do items.
/// </summary>
public static class AppCore
{
    /// <summary>
    /// Configures the application to use the core services.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>The web application builder with core services added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the builder is null.</exception>
    public static WebApplicationBuilder UseAppCore(this WebApplicationBuilder builder)
    {
        builder?.Services.AddScoped<TodoService>();
        return builder ?? throw new ArgumentNullException(nameof(builder));
    }
}
