using App.Core.Entities;
using App.Core.Repositories;
using App.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.UseData();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Version = "v1" });
    });
}

var app = builder.Build();
app.UseData();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the root
    });
}

var todosApi = app.MapGroup("/todos");
todosApi.MapGet(
    "/",
    async ([FromServices] ITodoRepository repository) => await repository.FindAllAsync().ConfigureAwait(false)
);

todosApi.MapGet(
    "/{id:int}",
    async (int id, [FromServices] ITodoRepository repository) =>
    {
        var todoEntity = await repository.FindByIdAsync(id).ConfigureAwait(false);
        return todoEntity != null ? Results.Ok(todoEntity) : Results.NotFound();
    }
);

// Example using the specialized repository
todosApi.MapGet(
    "/completed",
    async ([FromServices] ITodoRepository repository) =>
        await repository.FindCompletedTodosAsync().ConfigureAwait(false)
);

todosApi.MapGet(
    "/incomplete",
    async ([FromServices] ITodoRepository repository) =>
        await repository.FindIncompleteTodosAsync().ConfigureAwait(false)
);

todosApi.MapPost(
    "/",
    async (TodoEntity todo, [FromServices] ITodoRepository repository) =>
    {
        var result = await repository.CreateAsync(todo).ConfigureAwait(false);
        return Results.Created($"/todos/{result.Id}", result);
    }
);

app.Run();
