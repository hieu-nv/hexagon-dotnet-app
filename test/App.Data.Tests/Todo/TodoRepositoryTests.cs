using App.Core.Entities;
using App.Data;
using App.Data.Todo;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Tests.Todo;

public class TodoRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly TodoRepository _repository;

    public TodoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new TodoRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_WithValidEntity_ShouldAddTodo()
    {
        // Arrange
        var todo = new TodoEntity { Title = "Test Todo", IsCompleted = false };

        // Act
        var result = await _repository.CreateAsync(todo);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("Test Todo", result.Title);
        
        var dbTodo = await _context.Todos.FindAsync(result.Id);
        Assert.NotNull(dbTodo);
        Assert.Equal("Test Todo", dbTodo.Title);
    }

    [Fact]
    public async Task FindByIdAsync_WhenTodoExists_ShouldReturnTodo()
    {
        // Arrange
        var todo = new TodoEntity { Title = "Test Todo" };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByIdAsync(todo.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(todo.Id, result.Id);
        Assert.Equal("Test Todo", result.Title);
    }

    [Fact]
    public async Task FindByIdAsync_WhenTodoDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.FindByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FindAllAsync_ShouldReturnAllTodos()
    {
        // Arrange
        _context.Todos.AddRange(
            new TodoEntity { Title = "Todo 1" },
            new TodoEntity { Title = "Todo 2" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteAsync_WhenTodoExists_ShouldReturnTrue()
    {
        // Arrange
        var todo = new TodoEntity { Title = "Test Todo" };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(todo.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await _context.Todos.FindAsync(todo.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenTodoDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task FindCompletedTodosAsync_ShouldReturnOnlyCompleted()
    {
        // Arrange
        _context.Todos.AddRange(
            new TodoEntity { Title = "Completed", IsCompleted = true },
            new TodoEntity { Title = "Incomplete", IsCompleted = false }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindCompletedTodosAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsCompleted);
    }

    [Fact]
    public async Task FindIncompleteTodosAsync_ShouldReturnOnlyIncomplete()
    {
        // Arrange
        _context.Todos.AddRange(
            new TodoEntity { Title = "Completed", IsCompleted = true },
            new TodoEntity { Title = "Incomplete", IsCompleted = false }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindIncompleteTodosAsync();

        // Assert
        Assert.Single(result);
        Assert.False(result.First().IsCompleted);
    }

    [Fact]
    public async Task UpdateAsync_WithValidEntity_ShouldUpdateTodo()
    {
        // Arrange
        var todo = new TodoEntity { Title = "Original Title" };
        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        todo.Title = "Updated Title";
        var result = await _repository.UpdateAsync(todo);

        // Assert
        Assert.Equal("Updated Title", result.Title);
        
        var dbTodo = await _context.Todos.FindAsync(todo.Id);
        Assert.Equal("Updated Title", dbTodo!.Title);
    }
}
