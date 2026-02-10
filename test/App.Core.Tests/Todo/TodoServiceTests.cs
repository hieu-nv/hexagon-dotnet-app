using System.ComponentModel.DataAnnotations;
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

    [Fact]
    public async Task CreateAsync_WithValidEntity_ShouldCreateTodo()
    {
        // Arrange
        var newTodo = new TodoEntity { Title = "New Todo", IsCompleted = false };

        var createdTodo = new TodoEntity
        {
            Id = 1,
            Title = "New Todo",
            IsCompleted = false,
        };

        _todoRepository.CreateAsync(Arg.Any<TodoEntity>()).Returns(createdTodo);

        // Act
        var result = await _todoService.CreateAsync(newTodo);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdTodo.Id, result.Id);
        Assert.Equal(createdTodo.Title, result.Title);
        await _todoRepository.Received(1).CreateAsync(Arg.Any<TodoEntity>());
    }

    [Fact]
    public async Task CreateAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        TodoEntity? nullEntity = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _todoService.CreateAsync(nullEntity!)
        );
        await _todoRepository.DidNotReceive().CreateAsync(Arg.Any<TodoEntity>());
    }

    [Fact]
    public async Task CreateAsync_WithEmptyTitle_ShouldThrowValidationException()
    {
        // Arrange
        var invalidTodo = new TodoEntity { Title = "", IsCompleted = false };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _todoService.CreateAsync(invalidTodo));
        await _todoRepository.DidNotReceive().CreateAsync(Arg.Any<TodoEntity>());
    }

    [Fact]
    public async Task CreateAsync_WithWhitespaceTitle_ShouldThrowValidationException()
    {
        // Arrange
        var invalidTodo = new TodoEntity { Title = "   ", IsCompleted = false };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _todoService.CreateAsync(invalidTodo));
        await _todoRepository.DidNotReceive().CreateAsync(Arg.Any<TodoEntity>());
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateTodo()
    {
        // Arrange
        var todoId = 1;
        var existingTodo = new TodoEntity
        {
            Id = todoId,
            Title = "Original Title",
            IsCompleted = false,
        };

        var updatedData = new TodoEntity
        {
            Title = "Updated Title",
            DueBy = new DateOnly(2026, 3, 15),
            IsCompleted = true,
        };

        _todoRepository.FindByIdAsync(todoId).Returns(existingTodo);
        _todoRepository
            .UpdateAsync(Arg.Any<TodoEntity>())
            .Returns(callInfo => callInfo.Arg<TodoEntity>());

        // Act
        var result = await _todoService.UpdateAsync(todoId, updatedData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(todoId, result!.Id);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal(new DateOnly(2026, 3, 15), result.DueBy);
        Assert.True(result.IsCompleted);
        await _todoRepository.Received(1).FindByIdAsync(todoId);
        await _todoRepository.Received(1).UpdateAsync(Arg.Any<TodoEntity>());
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentTodo_ShouldReturnNull()
    {
        // Arrange
        var todoId = 999;
        var updatedData = new TodoEntity { Title = "Updated Title", IsCompleted = true };

        _todoRepository.FindByIdAsync(todoId).Returns((TodoEntity?)null);

        // Act
        var result = await _todoService.UpdateAsync(todoId, updatedData);

        // Assert
        Assert.Null(result);
        await _todoRepository.Received(1).FindByIdAsync(todoId);
        await _todoRepository.DidNotReceive().UpdateAsync(Arg.Any<TodoEntity>());
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var invalidId = 0;
        var updatedData = new TodoEntity { Title = "Updated Title", IsCompleted = true };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _todoService.UpdateAsync(invalidId, updatedData)
        );
        await _todoRepository.DidNotReceive().FindByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task UpdateAsync_WithNegativeId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var invalidId = -1;
        var updatedData = new TodoEntity { Title = "Updated Title", IsCompleted = true };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _todoService.UpdateAsync(invalidId, updatedData)
        );
        await _todoRepository.DidNotReceive().FindByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task UpdateAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        var todoId = 1;
        TodoEntity? nullEntity = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _todoService.UpdateAsync(todoId, nullEntity!)
        );
        await _todoRepository.DidNotReceive().FindByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyTitle_ShouldThrowValidationException()
    {
        // Arrange
        var todoId = 1;
        var invalidData = new TodoEntity { Title = "", IsCompleted = true };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _todoService.UpdateAsync(todoId, invalidData)
        );
        await _todoRepository.DidNotReceive().FindByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteTodo()
    {
        // Arrange
        var todoId = 1;
        _todoRepository.DeleteAsync(todoId).Returns(true);

        // Act
        var result = await _todoService.DeleteAsync(todoId);

        // Assert
        Assert.True(result);
        await _todoRepository.Received(1).DeleteAsync(todoId);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var todoId = 999;
        _todoRepository.DeleteAsync(todoId).Returns(false);

        // Act
        var result = await _todoService.DeleteAsync(todoId);

        // Assert
        Assert.False(result);
        await _todoRepository.Received(1).DeleteAsync(todoId);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _todoService.DeleteAsync(invalidId)
        );
        await _todoRepository.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task DeleteAsync_WithNegativeId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var invalidId = -5;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _todoService.DeleteAsync(invalidId)
        );
        await _todoRepository.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task FindByIdAsync_WithZeroId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _todoService.FindByIdAsync(invalidId)
        );
        await _todoRepository.DidNotReceive().FindByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task FindByIdAsync_WithNegativeId_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var invalidId = -10;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _todoService.FindByIdAsync(invalidId)
        );
        await _todoRepository.DidNotReceive().FindByIdAsync(Arg.Any<int>());
    }
}
