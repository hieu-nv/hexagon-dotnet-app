using System.Text.Json.Serialization;
using App.Data;
using App.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateSlimBuilder(args);

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"
    )
);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppApiJsonSerializerContext.Default);
});

var app = builder.Build();

// Create database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // This will create the database and schema if they don't exist
    context.Database.EnsureCreated();
}

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", async ([FromServices] AppDbContext db) => await db.Todos.ToListAsync());
todosApi.MapGet(
    "/{id}",
    async (int id, [FromServices] AppDbContext db) =>
    {
        var todo = await db.Todos.FindAsync(id);
        return todo != null ? Results.Ok(todo) : Results.NotFound();
    }
);

app.Run();

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(Todo))]
internal partial class AppApiJsonSerializerContext : JsonSerializerContext { }
