using App.Core.Repositories;
using Microsoft.AspNetCore.Builder;

namespace App.Api.Endpoints;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Class is instantiated by the DI container."
)]
sealed class TodoEndpoints(ITodoRepository todoRepository)
{
    private readonly ITodoRepository _todoRepository = todoRepository;

    public async Task<IResult> GetAllTodos()
    {
        var todos = await _todoRepository.FindAllAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }

    public async Task<IResult> GetTodoById(int id)
    {
        var todo = await _todoRepository.FindByIdAsync(id).ConfigureAwait(false);
        return todo is not null ? Results.Ok(todo) : Results.NotFound();
    }

    public async Task<IResult> GetCompletedTodos()
    {
        var todos = await _todoRepository.FindCompletedTodosAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }

    public async Task<IResult> GetIncompleteTodos()
    {
        var todos = await _todoRepository.FindIncompleteTodosAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }
}

internal static class TodoEndpointsExtensions
{
    public static WebApplicationBuilder UseTodo(this WebApplicationBuilder app)
    {
        app.Services.AddScoped<TodoEndpoints>();
        return app;
    }

    public static IEndpointRouteBuilder UseTodo(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("/todos");

        group.MapGet("/", (TodoEndpoints handler) => handler.GetAllTodos());
        group.MapGet("/{id:int}", (TodoEndpoints handler, int id) => handler.GetTodoById(id));
        group.MapGet("/completed", (TodoEndpoints handler) => handler.GetCompletedTodos());
        group.MapGet("/incomplete", (TodoEndpoints handler) => handler.GetIncompleteTodos());

        return endpointRouteBuilder;
    }
}
