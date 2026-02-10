using System.ComponentModel.DataAnnotations;
using App.Core.Entities;

namespace App.Core.Todo;

/// <summary>
/// Service for managing to-do items.
/// </summary>
/// <param name="todoRepository"></param>
public class TodoService(ITodoRepository todoRepository)
{
    /// <summary>
    /// Repository for managing to-do items.
    /// </summary>
    private readonly ITodoRepository _todoRepository = todoRepository;

    /// <summary>
    /// Finds all to-do items.
    /// </summary>
    public async Task<IEnumerable<TodoEntity>> FindAllAsync()
    {
        return await _todoRepository.FindAllAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Finds a to-do item by its ID.
    /// </summary>
    public async Task<TodoEntity?> FindByIdAsync(int id)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 1);
        return await _todoRepository.FindByIdAsync(id).ConfigureAwait(false);
    }

    /// <summary>
    /// Finds all completed to-do items.
    /// </summary>
    public async Task<IEnumerable<TodoEntity>> FindCompletedAsync()
    {
        return await _todoRepository.FindCompletedTodosAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Finds all incomplete to-do items.
    /// </summary>
    public async Task<IEnumerable<TodoEntity>> FindIncompleteAsync()
    {
        return await _todoRepository.FindIncompleteTodosAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new to-do item.
    /// </summary>
    public async Task<TodoEntity> CreateAsync(TodoEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (string.IsNullOrWhiteSpace(entity.Title))
        {
            throw new ValidationException("Title is required");
        }

        return await _todoRepository.CreateAsync(entity).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an existing to-do item.
    /// </summary>
    public async Task<TodoEntity?> UpdateAsync(int id, TodoEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 1);

        if (string.IsNullOrWhiteSpace(entity.Title))
        {
            throw new ValidationException("Title is required");
        }

        var existing = await _todoRepository.FindByIdAsync(id).ConfigureAwait(false);
        if (existing == null)
        {
            return null;
        }

        existing.Title = entity.Title;
        existing.DueBy = entity.DueBy;
        existing.IsCompleted = entity.IsCompleted;

        return await _todoRepository.UpdateAsync(existing).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a to-do item by its ID.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 1);
        return await _todoRepository.DeleteAsync(id).ConfigureAwait(false);
    }
}
