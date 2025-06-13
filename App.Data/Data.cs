using System.Text.Json.Serialization;
using App.Core.Entities;
using App.Core.Repositories;
using App.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Data;

public static class Data
{
    /// <summary>
    /// Configures the application to use the data layer with SQLite.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    public static WebApplicationBuilder UseData(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db")
        );

        builder.Services.AddScoped<ITodoRepository, TodoRepository>();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, DataJsonSerializerContext.Default);
        });

        return builder;
    }

    /// <summary>
    /// Ensures the database is created and seeded with initial data.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application.</returns>
    public static WebApplication UseData(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }
        return app;
    }
}

[JsonSerializable(typeof(TodoEntity[]))]
[JsonSerializable(typeof(TodoEntity))]
internal partial class DataJsonSerializerContext : JsonSerializerContext { }
