using App.Core.Entities;

namespace App.Core.Todo;

public class TodoService(ITodoRepository todoRepository)
{
    private readonly ITodoRepository _todoRepository = todoRepository;

    public async Task<IEnumerable<TodoEntity>> FindAllAsync()
    {
        return await _todoRepository.FindAllAsync().ConfigureAwait(false);
    }

    public async Task<TodoEntity?> FindByIdAsync(int id)
    {
        return await _todoRepository.FindByIdAsync(id).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TodoEntity>> FindCompletedAsync()
    {
        return await _todoRepository.FindCompletedTodosAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<TodoEntity>> FindIncompleteAsync()
    {
        return await _todoRepository.FindIncompleteTodosAsync().ConfigureAwait(false);
    }
}
