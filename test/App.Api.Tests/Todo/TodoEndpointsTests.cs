using System.ComponentModel.DataAnnotations;
using App.Api.Todo;
using App.Core.Entities;
using App.Core.Todo;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace App.Api.Tests.Todo;

public class TodoEndpointsTests
{
    private readonly Mock<ITodoRepository> _todoRepositoryMock;
    private readonly TodoService _todoService;
    private readonly TodoEndpoints _todoEndpoints;

    public TodoEndpointsTests()
    {
        // Mock the repository and use the actual service
        // This provides integration-like testing of endpoints + service layers
        _todoRepositoryMock = new Mock<ITodoRepository>();
        _todoService = new TodoService(_todoRepositoryMock.Object);
        _todoEndpoints = new TodoEndpoints(_todoService);
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
        var okResult = Assert.IsType<Ok<IEnumerable<TodoEntity>>>(result);
        Assert.Equal(expectedTodos, okResult.Value);
        _todoRepositoryMock.Verify(x => x.FindAllAsync(), Times.Once);
    }

    [Fact]
    public async Task FindAllTodosAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        _todoRepositoryMock
            .Setup(x => x.FindAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _todoEndpoints.FindAllTodosAsync();

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
        Assert.Equal("Internal Server Error", problemResult.ProblemDetails.Title);
        Assert.Equal(
            "An error occurred while retrieving todos",
            problemResult.ProblemDetails.Detail
        );
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
        var okResult = Assert.IsType<Ok<TodoEntity>>(result);
        Assert.Equal(expectedTodo, okResult.Value);
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

        // Act
        var result = await _todoEndpoints.FindTodoByIdAsync(todoId);

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
        Assert.Equal("Internal Server Error", problemResult.ProblemDetails.Title);
        Assert.Equal(
            "An error occurred while retrieving the todo",
            problemResult.ProblemDetails.Detail
        );
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
        var okResult = Assert.IsType<Ok<IEnumerable<TodoEntity>>>(result);
        Assert.Equal(completedTodos, okResult.Value);
        Assert.All(okResult.Value, todo => Assert.True(todo.IsCompleted));
        _todoRepositoryMock.Verify(x => x.FindCompletedTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task FindCompletedTodosAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        _todoRepositoryMock
            .Setup(x => x.FindCompletedTodosAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _todoEndpoints.FindCompletedTodosAsync();

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
        Assert.Equal("Internal Server Error", problemResult.ProblemDetails.Title);
        Assert.Equal(
            "An error occurred while retrieving completed todos",
            problemResult.ProblemDetails.Detail
        );
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
        var okResult = Assert.IsType<Ok<IEnumerable<TodoEntity>>>(result);
        Assert.Equal(incompleteTodos, okResult.Value);
        Assert.All(okResult.Value, todo => Assert.False(todo.IsCompleted));
        _todoRepositoryMock.Verify(x => x.FindIncompleteTodosAsync(), Times.Once);
    }

    [Fact]
    public async Task FindIncompleteTodosAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        _todoRepositoryMock
            .Setup(x => x.FindIncompleteTodosAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _todoEndpoints.FindIncompleteTodosAsync();

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
        Assert.Equal("Internal Server Error", problemResult.ProblemDetails.Title);
        Assert.Equal(
            "An error occurred while retrieving incomplete todos",
            problemResult.ProblemDetails.Detail
        );
    }

    #endregion

    #region CreateTodoAsync Tests

    [Fact]
    public async Task CreateTodoAsync_WithValidEntity_ShouldReturnCreated()
    {
        // Arrange
        var newTodo = new TodoEntity { Title = "New Todo", IsCompleted = false };

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
        var createdResult = Assert.IsType<Created<TodoEntity>>(result);
        Assert.Equal($"/todos/{createdTodo.Id}", createdResult.Location);
        Assert.Equal(createdTodo, createdResult.Value);
        _todoRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TodoEntity>()), Times.Once);
    }

    [Fact]
    public async Task CreateTodoAsync_WithNullEntity_ShouldReturnBadRequest()
    {
        // Arrange
        TodoEntity? nullEntity = null;

        // Act
        var result = await _todoEndpoints.CreateTodoAsync(nullEntity!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("Todo item is required", badRequestResult.Value);
        _todoRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TodoEntity>()), Times.Never);
    }

    [Fact]
    public async Task CreateTodoAsync_WithEmptyTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidTodo = new TodoEntity { Title = "", IsCompleted = false };

        // Act
        var result = await _todoEndpoints.CreateTodoAsync(invalidTodo);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("Title", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateTodoAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        var newTodo = new TodoEntity { Title = "New Todo", IsCompleted = false };

        _todoRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<TodoEntity>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _todoEndpoints.CreateTodoAsync(newTodo);

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
        Assert.Equal("Internal Server Error", problemResult.ProblemDetails.Title);
        Assert.Equal(
            "An error occurred while creating the todo",
            problemResult.ProblemDetails.Detail
        );
    }

    #endregion

    #region UpdateTodoAsync Tests

    [Fact]
    public async Task UpdateTodoAsync_WithValidData_ShouldReturnOkWithUpdatedTodo()
    {
        // Arrange
        var todoId = 1;
        var updateData = new TodoEntity
        {
            Title = "Updated Title",
            IsCompleted = true,
            DueBy = new DateOnly(2026, 3, 15),
        };

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
        var okResult = Assert.IsType<Ok<TodoEntity>>(result);
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
        var updateData = new TodoEntity { Title = "Updated Title", IsCompleted = true };

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
        var updateData = new TodoEntity { Title = "Updated Title", IsCompleted = true };

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
        var updateData = new TodoEntity { Title = "Updated Title", IsCompleted = true };

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
        TodoEntity? nullEntity = null;

        // Act
        var result = await _todoEndpoints.UpdateTodoAsync(todoId, nullEntity!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("Todo item is required", badRequestResult.Value);
        _todoRepositoryMock.Verify(x => x.FindByIdAsync(It.IsAny<int>()), Times.Never);
        _todoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TodoEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTodoAsync_WithEmptyTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var todoId = 1;
        var invalidData = new TodoEntity { Title = "", IsCompleted = true };

        var existingTodo = new TodoEntity
        {
            Id = todoId,
            Title = "Original Title",
            IsCompleted = false,
        };

        _todoRepositoryMock.Setup(x => x.FindByIdAsync(todoId)).ReturnsAsync(existingTodo);

        // Act
        var result = await _todoEndpoints.UpdateTodoAsync(todoId, invalidData);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result);
        Assert.Contains("Title", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateTodoAsync_WhenExceptionThrown_ShouldReturnProblem()
    {
        // Arrange
        var todoId = 1;
        var updateData = new TodoEntity { Title = "Updated Title", IsCompleted = true };

        _todoRepositoryMock
            .Setup(x => x.FindByIdAsync(todoId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _todoEndpoints.UpdateTodoAsync(todoId, updateData);

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
        Assert.Equal("Internal Server Error", problemResult.ProblemDetails.Title);
        Assert.Equal(
            "An error occurred while updating the todo",
            problemResult.ProblemDetails.Detail
        );
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

        // Act
        var result = await _todoEndpoints.DeleteTodoAsync(todoId);

        // Assert
        var problemResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(500, problemResult.StatusCode);
        Assert.Equal("Internal Server Error", problemResult.ProblemDetails.Title);
        Assert.Equal(
            "An error occurred while deleting the todo",
            problemResult.ProblemDetails.Detail
        );
    }

    #endregion
}
