using App.Core.Entities;
using App.Core.Todo;
using NSubstitute;
using Xunit;

namespace App.Core.Tests.Todo;

public class TodoServiceTests
{
    private readonly ITodoRepository _todoRepository;
    private readonly TodoService _todoService;

    public TodoServiceTests()
    {
        // Arrange - Setup mock repository
        _todoRepository = Substitute.For<ITodoRepository>();
        _todoService = new TodoService(_todoRepository);
    }

    [Fact]
    public async Task FindAllAsync_ShouldReturnAllTodos()
    {
        // Arrange
        var expectedTodos = new List<TodoEntity>
        {
            new()
            {
                Id = 1,
                Title = "Test Todo 1",
                IsCompleted = false,
            },
            new()
            {
                Id = 2,
                Title = "Test Todo 2",
                IsCompleted = true,
            },
        };
        
        _todoRepository.FindAllAsync().Returns(expectedTodos);

        // Act
        var result = await _todoService.FindAllAsync();

        // Assert
        Assert.Equal(expectedTodos, result);
        await _todoRepository.Received(1).FindAllAsync();
    }

    [Fact]
    public async Task FindByIdAsync_WithValidId_ShouldReturnTodo()
    {
        // Arrange
        var todoId = 1;
        var expectedTodo = new TodoEntity
        {
            Id = todoId,
            Title = "Test Todo",
            IsCompleted = false,
        };
        
        _todoRepository.FindByIdAsync(todoId).Returns(expectedTodo);

        // Act
        var result = await _todoService.FindByIdAsync(todoId);

        // Assert
        Assert.Equal(expectedTodo, result);
        await _todoRepository.Received(1).FindByIdAsync(todoId);
    }

    [Fact]
    public async Task FindByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var todoId = 999;
        _todoRepository.FindByIdAsync(todoId).Returns((TodoEntity?)null);

        // Act
        var result = await _todoService.FindByIdAsync(todoId);

        // Assert
        Assert.Null(result);
        await _todoRepository.Received(1).FindByIdAsync(todoId);
    }

    [Fact]
    public async Task FindCompletedAsync_ShouldReturnCompletedTodos()
    {
        // Arrange
        var completedTodos = new List<TodoEntity>
        {
            new()
            {
                Id = 1,
                Title = "Completed Todo 1",
                IsCompleted = true,
            },
            new()
            {
                Id = 2,
                Title = "Completed Todo 2",
                IsCompleted = true,
            },
        };
        
        _todoRepository.FindCompletedTodosAsync().Returns(completedTodos);

        // Act
        var result = await _todoService.FindCompletedAsync();

        // Assert
        Assert.Equal(completedTodos, result);
        Assert.All(result, todo => Assert.True(todo.IsCompleted));
        await _todoRepository.Received(1).FindCompletedTodosAsync();
    }

    [Fact]
    public async Task FindIncompleteAsync_ShouldReturnIncompleteTodos()
    {
        // Arrange
        var incompleteTodos = new List<TodoEntity>
        {
            new()
            {
                Id = 1,
                Title = "Incomplete Todo 1",
                IsCompleted = false,
            },
            new()
            {
                Id = 2,
                Title = "Incomplete Todo 2",
                IsCompleted = false,
            },
        };
        
        _todoRepository.FindIncompleteTodosAsync().Returns(incompleteTodos);

        // Act
        var result = await _todoService.FindIncompleteAsync();

        // Assert
        Assert.Equal(incompleteTodos, result);
        Assert.All(result, todo => Assert.False(todo.IsCompleted));
        await _todoRepository.Received(1).FindIncompleteTodosAsync();
    }
}
