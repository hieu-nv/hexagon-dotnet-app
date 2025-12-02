using App.Api.Todo;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for configuring the TodoEndpoints in the application.
/// </summary>
internal static class TodoEndpointsExtensions
{
    /// <summary>
    /// Adds the TodoEndpoints to the application builder.
    /// </summary>
    public static WebApplicationBuilder UseTodo(this WebApplicationBuilder app)
    {
        app.Services.AddScoped<TodoEndpoints>();
        return app;
    }

    /// <summary>
    /// Maps the TodoEndpoints to the specified route builder.
    /// </summary>
    public static IEndpointRouteBuilder UseTodo(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("/todos");

        group.MapGet("/", (TodoEndpoints handler) => handler.FindAllTodosAsync());
        group.MapGet("/{id:int}", (TodoEndpoints handler, int id) => handler.FindTodoByIdAsync(id));
        group.MapGet("/completed", (TodoEndpoints handler) => handler.FindCompletedTodosAsync());
        group.MapGet("/incomplete", (TodoEndpoints handler) => handler.FindIncompleteTodosAsync());

        return endpointRouteBuilder;
    }
}
