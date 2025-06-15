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
}
