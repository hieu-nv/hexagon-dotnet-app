using App.Core.Entities;
using App.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Repositories;

/// <summary>
/// Repository for TodoEntity with custom methods specific to todos.
/// </summary>
public class TodoRepository : Repository<TodoEntity, int>, ITodoRepository
{
    public TodoRepository(AppDbContext dbContext)
        : base(dbContext) { }

    public async Task<IEnumerable<TodoEntity>> FindCompletedTodosAsync()
    {
        return await _dbContext.Todos.Where(t => t.IsCompleted).ToListAsync();
    }

    public async Task<IEnumerable<TodoEntity>> FindIncompleteTodosAsync()
    {
        return await _dbContext.Todos.Where(t => !t.IsCompleted).ToListAsync();
    }
}
