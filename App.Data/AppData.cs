using System.Text.Json.Serialization;
using App.Core.Entities;
using App.Core.Repositories;
using App.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Data;

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

[JsonSerializable(typeof(TodoEntity[]))]
[JsonSerializable(typeof(TodoEntity))]
internal sealed partial class AppDataJsonSerializerContext : JsonSerializerContext { }
