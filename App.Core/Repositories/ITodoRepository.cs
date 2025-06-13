using App.Core.Entities;

namespace App.Core.Repositories;

/// <summary>
/// Repository interface for managing to-do items.
/// </summary>
public interface ITodoRepository : IRepository<TodoEntity, int>
{
    /// <summary>
    /// Retrieves all completed to-dos.
    /// </summary>
    Task<IEnumerable<TodoEntity>> FindCompletedTodosAsync();

    /// <summary>
    /// Retrieves all incomplete to-dos.
    /// </summary>
    Task<IEnumerable<TodoEntity>> FindIncompleteTodosAsync();
}
