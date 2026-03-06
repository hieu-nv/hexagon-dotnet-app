using App.Api.Auth;
using App.Api.Filters;
using App.Api.Todo;
using App.Core.Entities;

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
    public static IEndpointRouteBuilder UseTodo(
        this IEndpointRouteBuilder endpointRouteBuilder,
        Asp.Versioning.Builder.ApiVersionSet apiVersionSet
    )
    {
        var group = endpointRouteBuilder
            .MapGroup("/api/v{version:apiVersion}/todos")
            .WithApiVersionSet(apiVersionSet)
            .WithTags("Todos");

        // GET endpoints
        group
            .MapGet("/", (TodoEndpoints handler) => handler.FindAllTodosAsync())
            .WithName("GetAllTodos")
            .WithSummary("Get all to-do items")
            .AllowAnonymous()
            .Produces<IEnumerable<TodoResponse>>(200);

        group
            .MapGet("/{id:int}", (TodoEndpoints handler, int id) => handler.FindTodoByIdAsync(id))
            .WithName("GetTodoById")
            .WithSummary("Get a to-do item by ID")
            .AllowAnonymous()
            .Produces<TodoResponse>(200)
            .Produces(404);

        group
            .MapGet("/completed", (TodoEndpoints handler) => handler.FindCompletedTodosAsync())
            .WithName("GetCompletedTodos")
            .WithSummary("Get all completed to-do items")
            .AllowAnonymous()
            .Produces<IEnumerable<TodoResponse>>(200);

        group
            .MapGet("/incomplete", (TodoEndpoints handler) => handler.FindIncompleteTodosAsync())
            .WithName("GetIncompleteTodos")
            .WithSummary("Get all incomplete to-do items")
            .AllowAnonymous()
            .Produces<IEnumerable<TodoResponse>>(200);

        // POST endpoint
        group
            .MapPost(
                "/",
                (TodoEndpoints handler, CreateTodoRequest request) =>
                    handler.CreateTodoAsync(request)
            )
            .WithName("CreateTodo")
            .WithSummary("Create a new to-do item")
            .RequireAuthorization(AuthorizationPolicies.TodoAccess)
            .AddEndpointFilter<ValidationFilter<CreateTodoRequest>>()
            .Produces<TodoResponse>(201)
            .Produces(400)
            .Produces(401);

        // PUT endpoint
        group
            .MapPut(
                "/{id:int}",
                (TodoEndpoints handler, int id, UpdateTodoRequest request) =>
                    handler.UpdateTodoAsync(id, request)
            )
            .WithName("UpdateTodo")
            .WithSummary("Update an existing to-do item")
            .RequireAuthorization(AuthorizationPolicies.TodoAccess)
            .AddEndpointFilter<ValidationFilter<UpdateTodoRequest>>()
            .Produces<TodoResponse>(200)
            .Produces(404)
            .Produces(400)
            .Produces(401);

        // DELETE endpoint
        group
            .MapDelete("/{id:int}", (TodoEndpoints handler, int id) => handler.DeleteTodoAsync(id))
            .WithName("DeleteTodo")
            .WithSummary("Delete a to-do item")
            .RequireAuthorization()
            .Produces(204)
            .Produces(404)
            .Produces(400)
            .Produces(401);

        return endpointRouteBuilder;
    }
}
