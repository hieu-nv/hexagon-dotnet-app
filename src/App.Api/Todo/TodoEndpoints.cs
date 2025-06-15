using App.Core.Todo;

namespace App.Api.Todo;

/// <summary>
///
/// </summary>
/// <param name="todoService"></param>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Performance",
    "CA1812:Avoid uninstantiated internal classes",
    Justification = "Class is instantiated by the DI container."
)]
sealed class TodoEndpoints(TodoService todoService)
{
    /// <summary>
    /// Service for managing to-do items.
    /// </summary>
    private readonly TodoService _todoService = todoService;

    public async Task<IResult> FindAllTodos()
    {
        var todos = await _todoService.FindAllAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }

    /// <summary>
    /// Finds a to-do item by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IResult> FindTodoById(int id)
    {
        var todo = await _todoService.FindByIdAsync(id).ConfigureAwait(false);
        return todo is not null ? Results.Ok(todo) : Results.NotFound();
    }

    /// <summary>
    /// Finds all completed to-do items.
    /// </summary>
    /// <returns></returns>
    public async Task<IResult> FindCompletedTodos()
    {
        var todos = await _todoService.FindCompletedAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }

    /// <summary>
    /// Finds all incomplete to-do items.
    /// </summary>
    /// <returns></returns>
    public async Task<IResult> FindIncompleteTodos()
    {
        var todos = await _todoService.FindIncompleteAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }
}

/// <summary>
/// Extension methods for configuring the Todo endpoints in the application.
/// </summary>
internal static class TodoEndpointsExtensions
{
    /// <summary>
    /// Adds the Todo endpoints to the application builder.
    /// </summary>
    public static WebApplicationBuilder UseTodo(this WebApplicationBuilder app)
    {
        app.Services.AddScoped<TodoEndpoints>();
        return app;
    }

    /// <summary>
    /// Maps the Todo endpoints to the specified route builder.
    /// </summary>
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
