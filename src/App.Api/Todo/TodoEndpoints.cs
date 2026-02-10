using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using App.Core.Entities;
using App.Core.Todo;

namespace App.Api.Todo;

/// <summary>
/// Endpoints for managing to-do items.
/// </summary>
/// <param name="todoService">Service for managing to-do items.</param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class TodoEndpoints(TodoService todoService)
{
    /// <summary>
    /// Service for managing to-do items.
    /// </summary>
    private readonly TodoService _todoService = todoService;

    /// <summary>
    /// Finds all to-do items.
    /// </summary>
    public async Task<IResult> FindAllTodosAsync()
    {
        try
        {
            var todos = await _todoService.FindAllAsync().ConfigureAwait(false);
            return Results.Ok(todos);
        }
        catch (Exception)
        {
            return Results.Problem(
                detail: "An error occurred while retrieving todos",
                statusCode: 500,
                title: "Internal Server Error"
            );
        }
    }

    /// <summary>
    /// Finds a to-do item by its ID.
    /// </summary>
    /// <param name="id">The ID of the to-do item to retrieve.</param>
    public async Task<IResult> FindTodoByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return Results.BadRequest("Invalid ID. ID must be greater than zero.");
            }

            var todo = await _todoService.FindByIdAsync(id).ConfigureAwait(false);
            return todo is not null ? Results.Ok(todo) : Results.NotFound();
        }
        catch (ArgumentOutOfRangeException)
        {
            return Results.BadRequest("Invalid ID. ID must be greater than zero.");
        }
        catch (Exception)
        {
            return Results.Problem(
                detail: "An error occurred while retrieving the todo",
                statusCode: 500,
                title: "Internal Server Error"
            );
        }
    }

    /// <summary>
    /// Finds all completed to-do items.
    /// </summary>
    public async Task<IResult> FindCompletedTodosAsync()
    {
        try
        {
            var todos = await _todoService.FindCompletedAsync().ConfigureAwait(false);
            return Results.Ok(todos);
        }
        catch (Exception)
        {
            return Results.Problem(
                detail: "An error occurred while retrieving completed todos",
                statusCode: 500,
                title: "Internal Server Error"
            );
        }
    }

    /// <summary>
    /// Finds all incomplete to-do items.
    /// </summary>
    public async Task<IResult> FindIncompleteTodosAsync()
    {
        try
        {
            var todos = await _todoService.FindIncompleteAsync().ConfigureAwait(false);
            return Results.Ok(todos);
        }
        catch (Exception)
        {
            return Results.Problem(
                detail: "An error occurred while retrieving incomplete todos",
                statusCode: 500,
                title: "Internal Server Error"
            );
        }
    }

    /// <summary>
    /// Creates a new to-do item.
    /// </summary>
    /// <param name="entity">The to-do item to create.</param>
    public async Task<IResult> CreateTodoAsync(TodoEntity entity)
    {
        try
        {
            if (entity == null)
            {
                return Results.BadRequest("Todo item is required");
            }

            var created = await _todoService.CreateAsync(entity).ConfigureAwait(false);
            return Results.Created($"/todos/{created.Id}", created);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return Results.Problem(
                detail: "An error occurred while creating the todo",
                statusCode: 500,
                title: "Internal Server Error"
            );
        }
    }

    /// <summary>
    /// Updates an existing to-do item.
    /// </summary>
    /// <param name="id">The ID of the to-do item to update.</param>
    /// <param name="entity">The updated to-do item data.</param>
    public async Task<IResult> UpdateTodoAsync(int id, TodoEntity entity)
    {
        try
        {
            if (id <= 0)
            {
                return Results.BadRequest("Invalid ID. ID must be greater than zero.");
            }

            if (entity == null)
            {
                return Results.BadRequest("Todo item is required");
            }

            var updated = await _todoService.UpdateAsync(id, entity).ConfigureAwait(false);
            return updated is not null ? Results.Ok(updated) : Results.NotFound();
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Results.BadRequest("Invalid ID. ID must be greater than zero.");
        }
        catch (Exception)
        {
            return Results.Problem(
                detail: "An error occurred while updating the todo",
                statusCode: 500,
                title: "Internal Server Error"
            );
        }
    }

    /// <summary>
    /// Deletes a to-do item by its ID.
    /// </summary>
    /// <param name="id">The ID of the to-do item to delete.</param>
    public async Task<IResult> DeleteTodoAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return Results.BadRequest("Invalid ID. ID must be greater than zero.");
            }

            var deleted = await _todoService.DeleteAsync(id).ConfigureAwait(false);
            return deleted ? Results.NoContent() : Results.NotFound();
        }
        catch (ArgumentOutOfRangeException)
        {
            return Results.BadRequest("Invalid ID. ID must be greater than zero.");
        }
        catch (Exception)
        {
            return Results.Problem(
                detail: "An error occurred while deleting the todo",
                statusCode: 500,
                title: "Internal Server Error"
            );
        }
    }

    private string GetDebuggerDisplay()
    {
        return ToString() ?? string.Empty;
    }
}
