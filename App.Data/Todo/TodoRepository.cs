using App.Core.Entities;
using App.Core.Todo;
using App.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Todo;

/// <summary>
/// Repository for TodoEntity with custom methods specific to todos.
/// </summary>
public class TodoRepository(AppDbContext dbContext)
    : Repository<TodoEntity, int>(dbContext),
        ITodoRepository
{
    public async Task<IEnumerable<TodoEntity>> FindCompletedTodosAsync()
    {
        return await DbContext.Todos.Where(t => t.IsCompleted).ToListAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<TodoEntity>> FindIncompleteTodosAsync()
    {
        return await DbContext.Todos.Where(t => !t.IsCompleted).ToListAsync().ConfigureAwait(false);
    }
}
