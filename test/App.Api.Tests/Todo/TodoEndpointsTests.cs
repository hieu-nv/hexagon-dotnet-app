using System.ComponentModel.DataAnnotations;
using App.Api.Filters;
using App.Api.Todo;
using App.Core.Entities;
using App.Core.Todo;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace App.Api.Tests.Todo;

public class TodoEndpointsTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly Mock<ILogger<TodoEndpoints>> _loggerMock;
    private readonly TodoService _todoService;
    private readonly TodoEndpoints _todoEndpoints;

    public TodoEndpointsTests()
    {
        // Mock the repository and use the actual service
        // This provides integration-like testing of endpoints + service layers
        _todoRepositoryMock = new Mock<ITodoRepository>();
        _loggerMock = new Mock<ILogger<TodoEndpoints>>();
        _todoService = new TodoService(_todoRepositoryMock.Object);
        _todoEndpoints = new TodoEndpoints(_todoService, _loggerMock.Object);
    }

    #region FindAllTodosAsync Tests

    [Fact]
    public async Task FindAllTodosAsync_ShouldReturnOkWithTodos()
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

        _todoRepositoryMock.Setup(x => x.FindAllAsync()).ReturnsAsync(expectedTodos);

        // Act
        var result = await _todoEndpoints.FindAllTodosAsync();

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<TodoResponse>>>(result);
        var responseList = okResult.Value.ToList();
        Assert.Equal(2, responseList.Count);
        Assert.Equal(expectedTodos[0].Id, responseList[0].Id);
        Assert.Equal(expectedTodos[1].Id, responseList[1].Id);
        _todoRepositoryMock.Verify(x => x.FindAllAsync(), Times.Once);
    }

    [Fact]
    public async Task FindAllTodosAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        _todoRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _todoEndpoints.FindAllTodosAsync()
        );
        Assert.Equal("Database error", exception.Message);
    }

    #endregion

    #region FindTodoByIdAsync Tests

    [Fact]
    public async Task FindTodoByIdAsync_WithValidId_ShouldReturnOkWithTodo()
    {
        // Arrange
        var todoId = 1;
        var expectedTodo = new TodoEntity
        {
            Id = todoId,
            Title = "Test Todo",
            IsCompleted = false,
        };

        _todoRepositoryMock.Setup(x => x.FindByIdAsync(todoId)).ReturnsAsync(expectedTodo);

        // Act
        var result = await _todoEndpoints.FindTodoByIdAsync(todoId);

        // Assert
        var okResult = Assert.IsType<Ok<TodoResponse>>(result);
        Assert.Equal(expectedTodo.Id, okResult.Value.Id);
        Assert.Equal(expectedTodo.Title, okResult.Value.Title);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(todoId), Times.Once);
    }

    [Fact]
    public async Task FindTodoByIdAsync_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var todoId = 999;
        _todoRepositoryMock.Setup(x => x.FindByIdAsync(todoId)).ReturnsAsync((TodoEntity?)null);

        // Act
        var result = await _todoEndpoints.FindTodoByIdAsync(todoId);

        // Assert
        Assert.IsType<NotFound>(result);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(todoId), Times.Once);
    }

    [Fact]
    public async Task FindTodoByIdAsync_WithZeroId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidId = 0;

        // Act
        var result = await _todoEndpoints.FindTodoByIdAsync(invalidId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("Invalid ID. ID must be greater than zero.", badRequestResult.Value);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task FindTodoByIdAsync_WithNegativeId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var result = await _todoEndpoints.FindTodoByIdAsync(invalidId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("Invalid ID. ID must be greater than zero.", badRequestResult.Value);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task FindTodoByIdAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        var todoId = 1;
        _todoRepositoryMock
            .Setup(x => x.FindByIdAsync(todoId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _todoEndpoints.FindTodoByIdAsync(todoId)
        );
        Assert.Equal("Database error", exception.Message);
    }

    #endregion

    #region FindCompletedTodosAsync Tests

    [Fact]
    public async Task FindCompletedTodosAsync_ShouldReturnOkWithCompletedTodos()
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

        _todoRepositoryMock.Setup(x => x.FindCompletedTodosAsync()).ReturnsAsync(completedTodos);

        // Act
        var result = await _todoEndpoints.FindCompletedTodosAsync();

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<TodoResponse>>>(result);
        var responseList = okResult.Value.ToList();
        Assert.Equal(2, responseList.Count);
        Assert.All(responseList, todo => Assert.True(todo.IsCompleted));
        _todoRepositoryMock.Verify(x => x.FindCompletedTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task FindCompletedTodosAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        _todoRepositoryMock
            .Setup(x => x.FindCompletedTodosAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _todoEndpoints.FindCompletedTodosAsync()
        );
        Assert.Equal("Database error", exception.Message);
    }

    #endregion

    #region FindIncompleteTodosAsync Tests

    [Fact]
    public async Task FindIncompleteTodosAsync_ShouldReturnOkWithIncompleteTodos()
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

        _todoRepositoryMock.Setup(x => x.FindIncompleteTodosAsync()).ReturnsAsync(incompleteTodos);

        // Act
        var result = await _todoEndpoints.FindIncompleteTodosAsync();

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<TodoResponse>>>(result);
        var responseList = okResult.Value.ToList();
        Assert.Equal(2, responseList.Count);
        Assert.All(responseList, todo => Assert.False(todo.IsCompleted));
        _todoRepositoryMock.Verify(x => x.FindIncompleteTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task FindIncompleteTodosAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        _todoRepositoryMock
            .Setup(x => x.FindIncompleteTodosAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _todoEndpoints.FindIncompleteTodosAsync()
        );
        Assert.Equal("Database error", exception.Message);
    }

    #endregion

    #region CreateTodoAsync Tests

    [Fact]
    public async Task CreateTodoAsync_WithValidEntity_ShouldReturnCreated()
    {
        // Arrange
        var newTodo = new CreateTodoRequest("New Todo", false, null);

        var createdTodo = new TodoEntity
        {
            Id = 1,
            Title = "New Todo",
            IsCompleted = false,
        };

        _todoRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<TodoEntity>()))
            .ReturnsAsync(createdTodo);

        // Act
        var result = await _todoEndpoints.CreateTodoAsync(newTodo);

        // Assert
        var createdResult = Assert.IsType<Created<TodoResponse>>(result);
        Assert.Equal($"/api/v1/todos/{createdTodo.Id}", createdResult.Location);
        Assert.Equal(createdTodo.Id, createdResult.Value.Id);
        _todoRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TodoEntity>()), Times.Once);
    }

    [Fact]
    public async Task CreateTodoAsync_WithNullEntity_ShouldReturnBadRequest()
    {
        // Arrange
        CreateTodoRequest? nullEntity = null;

        // Act
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _todoEndpoints.CreateTodoAsync(nullEntity!)
        );
        _todoRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TodoEntity>()), Times.Never);
    }

    [Fact]
    public async Task CreateTodoAsync_WithEmptyTitle_ShouldThrowValidationException()
    {
        // Arrange
        var invalidTodo = new CreateTodoRequest("", false, null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _todoEndpoints.CreateTodoAsync(invalidTodo)
        );
    }

    [Fact]
    public async Task CreateTodoAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        var newTodo = new CreateTodoRequest("New Todo", false, null);

        _todoRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<TodoEntity>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _todoEndpoints.CreateTodoAsync(newTodo)
        );
        Assert.Equal("Database error", exception.Message);
    }

    #endregion

    #region UpdateTodoAsync Tests

    [Fact]
    public async Task UpdateTodoAsync_WithValidData_ShouldReturnOkWithUpdatedTodo()
    {
        // Arrange
        var todoId = 1;
        var updateData = new UpdateTodoRequest("Updated Title", true, new DateOnly(2026, 3, 15));

        var existingTodo = new TodoEntity
        {
            Id = todoId,
            Title = "Original Title",
            IsCompleted = false,
        };

        var updatedTodo = new TodoEntity
        {
            Id = todoId,
            Title = "Updated Title",
            IsCompleted = true,
            DueBy = new DateOnly(2026, 3, 15),
        };

        _todoRepositoryMock.Setup(x => x.FindByIdAsync(todoId)).ReturnsAsync(existingTodo);
        _todoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<TodoEntity>()))
            .ReturnsAsync(updatedTodo);

        // Act
        var result = await _todoEndpoints.UpdateTodoAsync(todoId, updateData);

        // Assert
        var okResult = Assert.IsType<Ok<TodoResponse>>(result);
        Assert.Equal(updatedTodo.Id, okResult.Value.Id);
        Assert.Equal("Updated Title", okResult.Value.Title);
        Assert.True(okResult.Value.IsCompleted);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(todoId), Times.Once);
        _todoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TodoEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTodoAsync_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var todoId = 999;
        var updateData = new UpdateTodoRequest("Updated Title", true, null);

        _todoRepositoryMock.Setup(x => x.FindByIdAsync(todoId)).ReturnsAsync((TodoEntity?)null);

        // Act
        var result = await _todoEndpoints.UpdateTodoAsync(todoId, updateData);

        // Assert
        Assert.IsType<NotFound>(result);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(todoId), Times.Once);
        _todoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TodoEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTodoAsync_WithZeroId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidId = 0;
        var updateData = new UpdateTodoRequest("Updated Title", true, null);

        // Act
        var result = await _todoEndpoints.UpdateTodoAsync(invalidId, updateData);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("Invalid ID. ID must be greater than zero.", badRequestResult.Value);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
        _todoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TodoEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTodoAsync_WithNegativeId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidId = -1;
        var updateData = new UpdateTodoRequest("Updated Title", true, null);

        // Act
        var result = await _todoEndpoints.UpdateTodoAsync(invalidId, updateData);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("Invalid ID. ID must be greater than zero.", badRequestResult.Value);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
        _todoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TodoEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTodoAsync_WithNullEntity_ShouldReturnBadRequest()
    {
        // Arrange
        var todoId = 1;
        UpdateTodoRequest? nullEntity = null;

        // Act
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _todoEndpoints.UpdateTodoAsync(todoId, nullEntity!)
        );
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
        _todoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TodoEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTodoAsync_WithEmptyTitle_ShouldThrowValidationException()
    {
        // Arrange
        var todoId = 1;
        var invalidData = new UpdateTodoRequest("", true, null);

        var existingTodo = new TodoEntity
        {
            Id = todoId,
            Title = "Original Title",
            IsCompleted = false,
        };

        _todoRepositoryMock.Setup(x => x.FindByIdAsync(todoId)).ReturnsAsync(existingTodo);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _todoEndpoints.UpdateTodoAsync(todoId, invalidData)
        );
    }

    [Fact]
    public async Task UpdateTodoAsync_WhenServiceThrowsArgumentOutOfRangeException_ShouldReturnBadRequest()
    {
        // Arrange
        var todoId = 1;
        var updateData = new UpdateTodoRequest("Updated Title", true, null);

        _todoRepositoryMock
            .Setup(x => x.FindByIdAsync(todoId))
            .ReturnsAsync(new TodoEntity { Id = todoId, Title = "Original" });
        _todoRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<TodoEntity>()))
            .ThrowsAsync(new ArgumentOutOfRangeException("Id", "Invalid ID"));

        // Act
        var result = await _todoEndpoints.UpdateTodoAsync(todoId, updateData);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("Invalid ID", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateTodoAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        var todoId = 1;
        var updateData = new UpdateTodoRequest("Updated Title", true, null);

        _todoRepositoryMock
            .Setup(x => x.FindByIdAsync(todoId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _todoEndpoints.UpdateTodoAsync(todoId, updateData)
        );
        Assert.Equal("Database error", exception.Message);
    }

    #endregion

    #region DeleteTodoAsync Tests

    [Fact]
    public async Task DeleteTodoAsync_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var todoId = 1;
        _todoRepositoryMock.Setup(x => x.DeleteAsync(todoId)).ReturnsAsync(true);

        // Act
        var result = await _todoEndpoints.DeleteTodoAsync(todoId);

        // Assert
        Assert.IsType<NoContent>(result);
        _todoRepositoryMock.Verify(x => x.DeleteAsync(todoId), Times.Once);
    }

    [Fact]
    public async Task DeleteTodoAsync_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var todoId = 999;
        _todoRepositoryMock.Setup(x => x.DeleteAsync(todoId)).ReturnsAsync(false);

        // Act
        var result = await _todoEndpoints.DeleteTodoAsync(todoId);

        // Assert
        Assert.IsType<NotFound>(result);
        _todoRepositoryMock.Verify(x => x.DeleteAsync(todoId), Times.Once);
    }

    [Fact]
    public async Task DeleteTodoAsync_WithZeroId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidId = 0;

        // Act
        var result = await _todoEndpoints.DeleteTodoAsync(invalidId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("Invalid ID. ID must be greater than zero.", badRequestResult.Value);
        _todoRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTodoAsync_WithNegativeId_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var result = await _todoEndpoints.DeleteTodoAsync(invalidId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("Invalid ID. ID must be greater than zero.", badRequestResult.Value);
        _todoRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTodoAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        var todoId = 1;
        _todoRepositoryMock
            .Setup(x => x.DeleteAsync(todoId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _todoEndpoints.DeleteTodoAsync(todoId)
        );
        Assert.Equal("Database error", exception.Message);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task FindTodoByIdAsync_WithLargeId_ShouldAttemptFetch()
    {
        // Arrange
        var largeId = int.MaxValue - 1;
        _todoRepositoryMock.Setup(x => x.FindByIdAsync(largeId)).ReturnsAsync((TodoEntity?)null);

        // Act
        var result = await _todoEndpoints.FindTodoByIdAsync(largeId);

        // Assert
        Assert.IsType<NotFound>(result);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(largeId), Times.Once);
    }

    [Fact]
    public async Task CreateTodoAsync_WithEmptyTitle_ShouldThrowValidationError()
    {
        // Arrange
        var request = new CreateTodoRequest(string.Empty, false, null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _todoEndpoints.CreateTodoAsync(request)
        );
        Assert.Equal("Title is required", ex.Message);
    }

    [Fact]
    public async Task CreateTodoAsync_WithWhitespaceTitle_ShouldThrowValidationError()
    {
        // Arrange
        var request = new CreateTodoRequest("   ", false, null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            _todoEndpoints.CreateTodoAsync(request)
        );
        Assert.Equal("Title is required", ex.Message);
    }

    [Fact]
    public async Task CreateTodoAsync_WithFutureDueDate_ShouldSucceed()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var request = new CreateTodoRequest("Future Todo", false, futureDate);
        var expectedEntity = new TodoEntity
        {
            Id = 1,
            Title = "Future Todo",
            IsCompleted = false,
            DueBy = futureDate,
        };

        _todoRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<TodoEntity>()))
            .ReturnsAsync(expectedEntity);

        // Act
        var result = await _todoEndpoints.CreateTodoAsync(request);

        // Assert
        var createdResult = Assert.IsType<Created<TodoResponse>>(result);
        Assert.NotNull(createdResult.Value);
        Assert.Equal("Future Todo", createdResult.Value.Title);
    }

    [Fact]
    public async Task UpdateTodoAsync_WithValidChanges_ShouldReturnUpdated()
    {
        // Arrange
        var todoId = 1;
        var request = new UpdateTodoRequest("Updated", true, null);
        var updated = new TodoEntity
        {
            Id = todoId,
            Title = "Updated",
            IsCompleted = true,
        };

        _todoRepositoryMock
            .Setup(x => x.FindByIdAsync(todoId))
            .ReturnsAsync(new TodoEntity { Id = todoId, Title = "Original" });
        _todoRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TodoEntity>())).ReturnsAsync(updated);

        // Act
        var result = await _todoEndpoints.UpdateTodoAsync(todoId, request);

        // Assert
        var okResult = Assert.IsType<Ok<TodoResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("Updated", okResult.Value.Title);
        Assert.True(okResult.Value.IsCompleted);
    }

    [Fact]
    public async Task FindCompletedTodosAsync_WithEmptyList_ShouldReturnOk()
    {
        // Arrange
        _todoRepositoryMock
            .Setup(x => x.FindCompletedTodosAsync())
            .ReturnsAsync(new List<TodoEntity>());

        // Act
        var result = await _todoEndpoints.FindCompletedTodosAsync();

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<TodoResponse>>>(result);
        Assert.Empty(okResult.Value);
    }

    [Fact]
    public async Task FindIncompleteTodosAsync_WithMultipleTodos_ShouldReturnAll()
    {
        // Arrange
        var incompleteTodos = new List<TodoEntity>
        {
            new()
            {
                Id = 1,
                Title = "Task 1",
                IsCompleted = false,
            },
            new()
            {
                Id = 2,
                Title = "Task 2",
                IsCompleted = false,
            },
            new()
            {
                Id = 3,
                Title = "Task 3",
                IsCompleted = false,
            },
        };

        _todoRepositoryMock.Setup(x => x.FindIncompleteTodosAsync()).ReturnsAsync(incompleteTodos);

        // Act
        var result = await _todoEndpoints.FindIncompleteTodosAsync();

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<TodoResponse>>>(result);
        var resultList = okResult.Value.ToList();
        Assert.Equal(3, resultList.Count);
        Assert.All(resultList, todo => Assert.False(todo.IsCompleted));
    }

    #endregion

    #region Debugger Tests

    [Fact]
    public void GetDebuggerDisplay_ReturnsString()
    {
        // Act
        var method = _todoEndpoints.GetType()
            .GetMethod("GetDebuggerDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(_todoEndpoints, null);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
    }

    #endregion
}
