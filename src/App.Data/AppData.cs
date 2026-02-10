using System.Text.Json.Serialization;
using App.Core.Entities;
using App.Core.Todo;
using App.Data;
using App.Data.Todo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides methods to configure the application data layer, including database context and repositories.
/// </summary>
public static class AppData
{
    /// <summary>
    /// Configures the application to use the data layer with SQLite.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    public static WebApplicationBuilder UseAppData(this WebApplicationBuilder builder)
    {
        builder?.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=app.db"
            )
        );

        builder?.Services.AddScoped<ITodoRepository, TodoRepository>();

        builder?.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

        builder?.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(
                0,
                AppDataJsonSerializerContext.Default
            );
        });

        return builder ?? throw new ArgumentNullException(nameof(builder));
    }

    /// <summary>
    /// Ensures the database is created and seeded with initial data.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application.</returns>
    public static WebApplication UseAppData(this WebApplication app)
    {
        using (var scope = app?.Services.CreateScope())
        {
            var context = scope?.ServiceProvider.GetRequiredService<AppDbContext>();
            context?.Database.EnsureCreated();
        }
        return app ?? throw new ArgumentNullException(nameof(app));
    }
}

/// <summary>
/// Provides a JSON serializer context for the application data layer.
/// This context is used for serializing and deserializing entities.
/// </summary>
[JsonSerializable(typeof(TodoEntity[]))]
[JsonSerializable(typeof(TodoEntity))]
internal sealed partial class AppDataJsonSerializerContext : JsonSerializerContext { }
