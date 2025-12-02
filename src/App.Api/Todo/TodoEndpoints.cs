using System.Diagnostics;
using App.Core.Todo;

namespace App.Api.Todo;

/// <summary>
///
/// </summary>
/// <param name="todoService"></param>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class TodoEndpoints(TodoService todoService)
{
    /// <summary>
    /// Service for managing to-do items.
    /// </summary>
    private readonly TodoService _todoService = todoService;

    public async Task<IResult> FindAllTodosAsync()
    {
        var todos = await _todoService.FindAllAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }

    /// <summary>
    /// Finds a to-do item by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IResult> FindTodoByIdAsync(int id)
    {
        var todo = await _todoService.FindByIdAsync(id).ConfigureAwait(false);
        return todo is not null ? Results.Ok(todo) : Results.NotFound();
    }

    /// <summary>
    /// Finds all completed to-do items.
    /// </summary>
    /// <returns></returns>
    public async Task<IResult> FindCompletedTodosAsync()
    {
        var todos = await _todoService.FindCompletedAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }

    /// <summary>
    /// Finds all incomplete to-do items.
    /// </summary>
    /// <returns></returns>
    public async Task<IResult> FindIncompleteTodosAsync()
    {
        var todos = await _todoService.FindIncompleteAsync().ConfigureAwait(false);
        return Results.Ok(todos);
    }

    private string GetDebuggerDisplay()
    {
        return ToString() ?? string.Empty;
    }
}
