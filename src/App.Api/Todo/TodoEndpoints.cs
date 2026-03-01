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
        var todos = (await _todoService.FindAllAsync().ConfigureAwait(false)).ToList();
        var response = todos.Select(t => t.ToResponse());
        _logger.LogInformation("Successfully retrieved {TodoCount} todos", todos.Count);
        return Results.Ok(response);
    }

    /// <summary>
    /// Finds a to-do item by its ID.
    /// </summary>
    /// <param name="id">The ID of the to-do item to retrieve.</param>
    public async Task<IResult> FindTodoByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving todo with ID: {TodoId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid todo ID provided: {TodoId}", id);
            return Results.BadRequest("Invalid ID. ID must be greater than zero.");
        }

        var todo = await _todoService.FindByIdAsync(id).ConfigureAwait(false);
        if (todo is not null)
        {
            _logger.LogInformation("Successfully retrieved todo with ID: {TodoId}", id);
            return Results.Ok(todo.ToResponse());
        }
        else
        {
            _logger.LogInformation("Todo with ID {TodoId} not found", id);
            return Results.NotFound();
        }
    }

    /// <summary>
    /// Finds all completed to-do items.
    /// </summary>
    public async Task<IResult> FindCompletedTodosAsync()
    {
        _logger.LogInformation("Retrieving completed todos");
        var todos = (await _todoService.FindCompletedAsync().ConfigureAwait(false)).ToList();
        var response = todos.Select(t => t.ToResponse());
        _logger.LogInformation(
            "Successfully retrieved {CompletedCount} completed todos",
            todos.Count
        );
        return Results.Ok(response);
    }

    /// <summary>
    /// Finds all incomplete to-do items.
    /// </summary>
    public async Task<IResult> FindIncompleteTodosAsync()
    {
        _logger.LogInformation("Retrieving incomplete todos");
        var todos = (await _todoService.FindIncompleteAsync().ConfigureAwait(false)).ToList();
        var response = todos.Select(t => t.ToResponse());
        _logger.LogInformation(
            "Successfully retrieved {IncompleteCount} incomplete todos",
            todos.Count
        );
        return Results.Ok(response);
    }

    /// <summary>
    /// Creates a new to-do item.
    /// </summary>
    /// <param name="request">The to-do item creation request.</param>
    public async Task<IResult> CreateTodoAsync(CreateTodoRequest request)
    {
        _logger.LogInformation("Creating new todo with title: {Title}", request.Title);

        var entity = new TodoEntity
        {
            Title = request.Title,
            IsCompleted = request.IsCompleted,
            DueBy = request.DueBy,
        };

        var created = await _todoService.CreateAsync(entity).ConfigureAwait(false);
        _logger.LogInformation(
            "Successfully created todo with ID: {TodoId}, Title: {Title}",
            created.Id,
            created.Title
        );
        return Results.Created($"/api/v1/todos/{created.Id}", created.ToResponse());
    }

    /// <summary>
    /// Updates an existing to-do item.
    /// </summary>
    /// <param name="id">The ID of the to-do item to update.</param>
    /// <param name="request">The to-do item update request.</param>
    public async Task<IResult> UpdateTodoAsync(int id, UpdateTodoRequest request)
    {
        _logger.LogInformation("Updating todo with ID: {TodoId}", id);
        try
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid todo ID provided for update: {TodoId}", id);
                return Results.BadRequest("Invalid ID. ID must be greater than zero.");
            }

            var entity = new TodoEntity
            {
                Id = id,
                Title = request.Title,
                IsCompleted = request.IsCompleted,
                DueBy = request.DueBy,
            };

            var updated = await _todoService.UpdateAsync(id, entity).ConfigureAwait(false);
            if (updated is not null)
            {
                _logger.LogInformation(
                    "Successfully updated todo with ID: {TodoId}, Title: {Title}",
                    id,
                    updated.Title
                );
                return Results.Ok(updated.ToResponse());
            }
            else
            {
                _logger.LogInformation("Todo with ID {TodoId} not found for update", id);
                return Results.NotFound();
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Validation failed: invalid argument provided");
            return Results.BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a to-do item by its ID.
    /// </summary>
    /// <param name="id">The ID of the to-do item to delete.</param>
    public async Task<IResult> DeleteTodoAsync(int id)
    {
        _logger.LogInformation("Deleting todo with ID: {TodoId}", id);

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

    private string GetDebuggerDisplay()
    {
        return ToString() ?? string.Empty;
    }
}
