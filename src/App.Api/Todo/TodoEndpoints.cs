using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using App.Core.Entities;
using App.Core.Todo;

using Microsoft.Extensions.Logging;

namespace App.Api.Todo;

/// <summary>
/// Endpoints for managing to-do items.
/// </summary>
/// <param name="todoService">Service for managing to-do items.</param>
/// <param name="logger">Logger for tracking operations.</param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class TodoEndpoints(TodoService todoService, ILogger<TodoEndpoints> logger)
{
    /// <summary>
    /// Service for managing to-do items.
    /// </summary>
    private readonly TodoService _todoService = todoService;

    /// <summary>
    /// Logger for tracking operations.
    /// </summary>
    private readonly ILogger<TodoEndpoints> _logger = logger;

    /// <summary>
    /// Finds all to-do items.
    /// </summary>
    public async Task<IResult> FindAllTodosAsync()
    {
        _logger.LogInformation("Retrieving all todos");
        try
        {
            var todos = await _todoService.FindAllAsync().ConfigureAwait(false);
            _logger.LogInformation("Successfully retrieved {TodoCount} todos", todos.Count());
            return Results.Ok(todos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all todos");
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
        _logger.LogInformation("Retrieving todo with ID: {TodoId}", id);
        try
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid todo ID provided: {TodoId}", id);
                return Results.BadRequest("Invalid ID. ID must be greater than zero.");
            }

            var todo = await _todoService.FindByIdAsync(id).ConfigureAwait(false);
            if (todo is not null)
            {
                _logger.LogInformation("Successfully retrieved todo with ID: {TodoId}", id);
                return Results.Ok(todo);
            }
            else
            {
                _logger.LogInformation("Todo with ID {TodoId} not found", id);
                return Results.NotFound();
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "ArgumentOutOfRangeException for todo ID: {TodoId}", id);
            return Results.BadRequest("Invalid ID. ID must be greater than zero.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving todo with ID: {TodoId}", id);
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
        _logger.LogInformation("Retrieving completed todos");
        try
        {
            var todos = await _todoService.FindCompletedAsync().ConfigureAwait(false);
            _logger.LogInformation("Successfully retrieved {CompletedCount} completed todos", todos.Count());
            return Results.Ok(todos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving completed todos");
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
        _logger.LogInformation("Retrieving incomplete todos");
        try
        {
            var todos = await _todoService.FindIncompleteAsync().ConfigureAwait(false);
            _logger.LogInformation("Successfully retrieved {IncompleteCount} incomplete todos", todos.Count());
            return Results.Ok(todos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving incomplete todos");
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
        _logger.LogInformation("Creating new todo with title: {Title}", entity?.Title);
        try
        {
            if (entity == null)
            {
                _logger.LogWarning("Create todo failed: entity is null");
                return Results.BadRequest("Todo item is required");
            }

            var created = await _todoService.CreateAsync(entity).ConfigureAwait(false);
            _logger.LogInformation("Successfully created todo with ID: {TodoId}, Title: {Title}", created.Id, created.Title);
            return Results.Created($"/todos/{created.Id}", created);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating todo: {Message}", ex.Message);
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating todo");
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
        _logger.LogInformation("Updating todo with ID: {TodoId}", id);
        try
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid todo ID provided for update: {TodoId}", id);
                return Results.BadRequest("Invalid ID. ID must be greater than zero.");
            }

            if (entity == null)
            {
                _logger.LogWarning("Update todo failed: entity is null for ID: {TodoId}", id);
                return Results.BadRequest("Todo item is required");
            }

            var updated = await _todoService.UpdateAsync(id, entity).ConfigureAwait(false);
            if (updated is not null)
            {
                _logger.LogInformation("Successfully updated todo with ID: {TodoId}, Title: {Title}", id, updated.Title);
                return Results.Ok(updated);
            }
            else
            {
                _logger.LogInformation("Todo with ID {TodoId} not found for update", id);
                return Results.NotFound();
            }
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed while updating todo ID {TodoId}: {Message}", id, ex.Message);
            return Results.BadRequest(ex.Message);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "ArgumentOutOfRangeException for todo ID: {TodoId}", id);
            return Results.BadRequest("Invalid ID. ID must be greater than zero.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating todo with ID: {TodoId}", id);
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
        _logger.LogInformation("Deleting todo with ID: {TodoId}", id);
        try
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid todo ID provided for delete: {TodoId}", id);
                return Results.BadRequest("Invalid ID. ID must be greater than zero.");
            }

            var deleted = await _todoService.DeleteAsync(id).ConfigureAwait(false);
            if (deleted)
            {
                _logger.LogInformation("Successfully deleted todo with ID: {TodoId}", id);
                return Results.NoContent();
            }
            else
            {
                _logger.LogInformation("Todo with ID {TodoId} not found for deletion", id);
                return Results.NotFound();
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "ArgumentOutOfRangeException for todo ID: {TodoId}", id);
            return Results.BadRequest("Invalid ID. ID must be greater than zero.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting todo with ID: {TodoId}", id);
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
