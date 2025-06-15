using App.Core.Entities;
using App.Core.Todo;
using App.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Todo;

/// <summary>
/// Repository for managing to-do items.
/// </summary>
/// <param name="dbContext"></param>
public class TodoRepository(AppDbContext dbContext)
    : Repository<TodoEntity, int>(dbContext),
        ITodoRepository
{
    /// <summary>
    /// Retrieves all completed to-dos.
    /// </summary>
    /// <returns>A collection of completed to-do items.</returns>
    public async Task<IEnumerable<TodoEntity>> FindCompletedTodosAsync()
    {
        return await DbContext.Todos.Where(t => t.IsCompleted).ToListAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all incomplete to-dos.
    /// </summary>
    /// <returns>A collection of incomplete to-do items.</returns>
    public async Task<IEnumerable<TodoEntity>> FindIncompleteTodosAsync()
    {
        return await DbContext.Todos.Where(t => !t.IsCompleted).ToListAsync().ConfigureAwait(false);
    }
}
