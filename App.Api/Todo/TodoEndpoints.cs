using App.Core.Todo;

namespace App.Api.Todo;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Class is instantiated by the DI container."
)]
sealed class TodoEndpoints(TodoService todoService)
{
    private readonly TodoService _todoService = todoService;

    public async Task<IResult> FindAllTodos()
    {
        var todos = await _todoService.FindAllAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }

    public async Task<IResult> FindTodoById(int id)
    {
        var todo = await _todoService.FindByIdAsync(id).ConfigureAwait(false);
        return todo is not null ? Results.Ok(todo) : Results.NotFound();
    }

    public async Task<IResult> FindCompletedTodos()
    {
        var todos = await _todoService.FindCompletedAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }

    public async Task<IResult> FindIncompleteTodos()
    {
        var todos = await _todoService.FindIncompleteAsync().ConfigureAwait(false);
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

        group.MapGet("/", (TodoEndpoints handler) => handler.FindAllTodos());
        group.MapGet("/{id:int}", (TodoEndpoints handler, int id) => handler.FindTodoById(id));
        group.MapGet("/completed", (TodoEndpoints handler) => handler.FindCompletedTodos());
        group.MapGet("/incomplete", (TodoEndpoints handler) => handler.FindIncompleteTodos());

        return endpointRouteBuilder;
    }
}
