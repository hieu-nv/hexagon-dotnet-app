using App.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Endpoints;

internal static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var todosApi = endpoints.MapGroup("/todos");

        todosApi.MapGet("/", FindAllAsync);

        todosApi.MapGet("/{id:int}", FindByIdAsync);

        todosApi.MapGet("/completed", FindCompletedTodosAsync);

        todosApi.MapGet("/incomplete", FindIncompleteTodosAsync);
    }

    public static async Task<IResult> FindAllAsync([FromServices] ITodoRepository repository)
    {
        return Results.Ok(await repository.FindAllAsync().ConfigureAwait(false));
    }

    public static async Task<IResult> FindByIdAsync([FromServices] ITodoRepository repository, int id)
    {
        var todoEntity = await repository.FindByIdAsync(id).ConfigureAwait(false);
        return todoEntity != null ? Results.Ok(todoEntity) : Results.NotFound();
    }

    public static async Task<IResult> FindCompletedTodosAsync([FromServices] ITodoRepository repository)
    {
        return Results.Ok(await repository.FindCompletedTodosAsync().ConfigureAwait(false));
    }

    public static async Task<IResult> FindIncompleteTodosAsync([FromServices] ITodoRepository repository)
    {
        return Results.Ok(await repository.FindIncompleteTodosAsync().ConfigureAwait(false));
    }
}
