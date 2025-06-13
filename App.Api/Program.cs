using App.Core.Entities;
using App.Core.Repositories;
using App.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);

builder.UseData();

var app = builder.Build();
app.UseData();

var todosApi = app.MapGroup("/todos");
todosApi.MapGet(
    "/",
    async ([FromServices] ITodoRepository repository) => await repository.FindAllAsync()
);

todosApi.MapGet(
    "/{id}",
    async (int id, [FromServices] ITodoRepository repository) =>
    {
        var todoEntity = await repository.FindByIdAsync(id);
        return todoEntity != null ? Results.Ok(todoEntity) : Results.NotFound();
    }
);

// Example using the specialized repository
todosApi.MapGet(
    "/completed",
    async ([FromServices] ITodoRepository repository) => await repository.FindCompletedTodosAsync()
);

todosApi.MapGet(
    "/incomplete",
    async ([FromServices] ITodoRepository repository) => await repository.FindIncompleteTodosAsync()
);

todosApi.MapPost(
    "/",
    async (TodoEntity todo, [FromServices] ITodoRepository repository) =>
    {
        var result = await repository.CreateAsync(todo);
        return Results.Created($"/todos/{result.Id}", result);
    }
);

app.Run();
