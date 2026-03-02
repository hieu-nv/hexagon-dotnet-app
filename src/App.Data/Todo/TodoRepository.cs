using App.Core.Entities;
using App.Core.Todo;

using Microsoft.EntityFrameworkCore;

namespace App.Data.Todo;

/// <summary>
/// Repository for managing to-do items.
/// </summary>
/// <param name="dbContext"></param>
public sealed class TodoRepository(AppDbContext dbContext) : ITodoRepository
{
    private AppDbContext DbContext { get; } = dbContext;

    /// <summary>
    /// Creates a new to-do item in the repository.
    /// </summary>
    /// <param name="entity">The to-do entity to create.</param>
    /// <returns>The created to-do entity.</returns>
    public async Task<TodoEntity> CreateAsync(TodoEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DbContext.Set<TodoEntity>().Add(entity);
        await DbContext.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }

    /// <summary>
    /// Deletes a to-do item by its ID.
    /// </summary>
    /// <param name="id">The ID of the to-do item to delete.</param>
    /// <returns>True if the item was deleted, false if not found.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await DbContext.Todos.FindAsync(id).ConfigureAwait(false);
        if (entity == null)
        {
            return false;
        }

        DbContext.Todos.Remove(entity);
        await DbContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Retrieves all to-do items.
    /// </summary>
    /// <returns>A collection of all to-do items.</returns>
    public async Task<IEnumerable<TodoEntity>> FindAllAsync()
    {
        return await DbContext
            .Todos.AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Finds a to-do item by its ID.
    /// </summary>
    /// <param name="id">The ID of the to-do item to find.</param>
    /// <returns>The to-do item if found, null otherwise.</returns>
    public async Task<TodoEntity?> FindByIdAsync(int id)
    {
        return await DbContext
            .Todos.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all completed to-dos.
    /// </summary>
    /// <returns>A collection of completed to-do items.</returns>
    public async Task<IEnumerable<TodoEntity>> FindCompletedTodosAsync()
    {
        return await DbContext
            .Todos.AsNoTracking()
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all incomplete to-dos.
    /// </summary>
    /// <returns>A collection of incomplete to-do items.</returns>
    public async Task<IEnumerable<TodoEntity>> FindIncompleteTodosAsync()
    {
        return await DbContext
            .Todos.AsNoTracking()
            .Where(t => !t.IsCompleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an existing to-do item in the repository.
    /// </summary>
    /// <param name="entity">The to-do entity to update.</param>
    /// <returns>The updated to-do entity.</returns>
    public async Task<TodoEntity> UpdateAsync(TodoEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DbContext.Entry(entity).State = EntityState.Modified;
        await DbContext.SaveChangesAsync().ConfigureAwait(false);
        return entity;
    }
}
