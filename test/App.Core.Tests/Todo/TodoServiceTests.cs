using App.Core.Entities;
using App.Core.Todo;

using Moq;

using Xunit;

namespace App.Core.Tests.Todo;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly ITodoRepository _todoRepository;
    private readonly TodoService _todoService;

    public TodoServiceTests()
    {
        // Arrange - Setup mock repository
        _todoRepositoryMock = new Mock<ITodoRepository>();
        _todoRepository = _todoRepositoryMock.Object;
        _todoService = new TodoService(_todoRepository);
    }

    [Fact]
    public async Task FindAllAsync_ShouldReturnAllTodos()
    {
        // Arrange
        var expectedTodos = new List<TodoEntity>
        {
            new() { Id = 1, Title = "Test Todo 1", IsCompleted = false, },
            new() { Id = 2, Title = "Test Todo 2", IsCompleted = true, },
        };

        _todoRepositoryMock.Setup(x => x.FindAllAsync()).ReturnsAsync(expectedTodos);

        // Act
        var result = await _todoService.FindAllAsync();

        // Assert
        Assert.Equal(expectedTodos, result);
        _todoRepositoryMock.Verify(x => x.FindAllAsync(), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_WithValidId_ShouldReturnTodo()
    {
        // Arrange
        var todoId = 1;
        var expectedTodo = new TodoEntity { Id = todoId, Title = "Test Todo", IsCompleted = false, };

        _todoRepositoryMock.Setup(x => x.FindByIdAsync(todoId)).ReturnsAsync(expectedTodo);

        // Act
        var result = await _todoService.FindByIdAsync(todoId);

        // Assert
        Assert.Equal(expectedTodo, result);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(todoId), Times.Once);
    }

    [Fact]
    public async Task FindByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var todoId = 999;
        _todoRepositoryMock.Setup(x => x.FindByIdAsync(todoId)).ReturnsAsync((TodoEntity?)null);

        // Act
        var result = await _todoService.FindByIdAsync(todoId);

        // Assert
        Assert.Null(result);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(todoId), Times.Once);
    }

    [Fact]
    public async Task FindCompletedAsync_ShouldReturnCompletedTodos()
    {
        // Arrange
        var completedTodos = new List<TodoEntity>
        {
            new() { Id = 1, Title = "Completed Todo 1", IsCompleted = true, },
            new() { Id = 2, Title = "Completed Todo 2", IsCompleted = true, },
        };

        _todoRepositoryMock.Setup(x => x.FindCompletedTodosAsync()).ReturnsAsync(completedTodos);

        // Act
        var result = await _todoService.FindCompletedAsync();

        // Assert
        Assert.Equal(completedTodos, result);
        Assert.All(result, todo => Assert.True(todo.IsCompleted));
        _todoRepositoryMock.Verify(x => x.FindCompletedTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task FindIncompleteAsync_ShouldReturnIncompleteTodos()
    {
        // Arrange
        var incompleteTodos = new List<TodoEntity>
        {
            new() { Id = 1, Title = "Incomplete Todo 1", IsCompleted = false, },
            new() { Id = 2, Title = "Incomplete Todo 2", IsCompleted = false, },
        };

        _todoRepositoryMock.Setup(x => x.FindIncompleteTodosAsync()).ReturnsAsync(incompleteTodos);

        // Act
        var result = await _todoService.FindIncompleteAsync();

        // Assert
        Assert.Equal(incompleteTodos, result);
        Assert.All(result, todo => Assert.False(todo.IsCompleted));
        _todoRepositoryMock.Verify(x => x.FindIncompleteTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidEntity_ShouldCreateTodo()
    {
        // Arrange
        var newTodo = new TodoEntity { Title = "New Todo", IsCompleted = false };

        var createdTodo = new TodoEntity { Id = 1, Title = "New Todo", IsCompleted = false, };

        _todoRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<TodoEntity>()))
            .ReturnsAsync(createdTodo);

        // Act
        var result = await _todoService.CreateAsync(newTodo);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdTodo.Id, result.Id);
        Assert.Equal(createdTodo.Title, result.Title);
        _todoRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TodoEntity>()), Times.Once);
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
        _todoRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TodoEntity>()), Times.Never);
    }


    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateTodo()
    {
        // Arrange
        var todoId = 1;
        var existingTodo = new TodoEntity { Id = todoId, Title = "Original Title", IsCompleted = false, };

        var updatedData = new TodoEntity
        {
            Title = "Updated Title", DueBy = new DateOnly(2026, 3, 15), IsCompleted = true,
        };

        _todoRepositoryMock.Setup(x => x.FindByIdAsync(todoId)).ReturnsAsync(existingTodo);
        _todoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<TodoEntity>()))
            .ReturnsAsync((TodoEntity entity) => entity);

        // Act
        var result = await _todoService.UpdateAsync(todoId, updatedData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(todoId, result!.Id);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal(new DateOnly(2026, 3, 15), result.DueBy);
        Assert.True(result.IsCompleted);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(todoId), Times.Once);
        _todoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TodoEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentTodo_ShouldReturnNull()
    {
        // Arrange
        var todoId = 999;
        var updatedData = new TodoEntity { Title = "Updated Title", IsCompleted = true };

        _todoRepositoryMock.Setup(x => x.FindByIdAsync(todoId)).ReturnsAsync((TodoEntity?)null);

        // Act
        var result = await _todoService.UpdateAsync(todoId, updatedData);

        // Assert
        Assert.Null(result);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(todoId), Times.Once);
        _todoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TodoEntity>()), Times.Never);
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
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
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
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
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
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
    }


    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteTodo()
    {
        // Arrange
        var todoId = 1;
        _todoRepositoryMock.Setup(x => x.DeleteAsync(todoId)).ReturnsAsync(true);

        // Act
        var result = await _todoService.DeleteAsync(todoId);

        // Assert
        Assert.True(result);
        _todoRepositoryMock.Verify(x => x.DeleteAsync(todoId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var todoId = 999;
        _todoRepositoryMock.Setup(x => x.DeleteAsync(todoId)).ReturnsAsync(false);

        // Act
        var result = await _todoService.DeleteAsync(todoId);

        // Assert
        Assert.False(result);
        _todoRepositoryMock.Verify(x => x.DeleteAsync(todoId), Times.Once);
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
        _todoRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
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
        _todoRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
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
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
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
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
    }
}
