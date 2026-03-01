using App.Core.Entities;
using App.Data;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Tests;

public class AppDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldUpdateUpdatedAtTimestampOnModifiedEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TimestampUpdateTest")
            .Options;

        using var context = new AppDbContext(options);
        var todo = new TodoEntity { Title = "Initial Title" };
        context.Todos.Add(todo);
        await context.SaveChangesAsync();
        
        var initialUpdatedAt = todo.UpdatedAt;
        
        // Add a small delay to ensure UtcNow is different if the test runs too fast
        await Task.Delay(10);

        // Act
        todo.Title = "Updated Title";
        context.Entry(todo).State = EntityState.Modified;
        await context.SaveChangesAsync();

        // Assert
        Assert.NotNull(todo.UpdatedAt);
        if (initialUpdatedAt.HasValue)
        {
            Assert.True(todo.UpdatedAt > initialUpdatedAt);
        }
    }

    [Fact]
    public void Constructor_WhenOptionsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AppDbContext(null!));
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotUpdateUpdatedAt_WhenEntityIsUnchanged()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "UnchangedEntityTest")
            .Options;

        using var context = new AppDbContext(options);
        var todo = new TodoEntity { Title = "Stable Todo" };
        context.Todos.Add(todo);
        await context.SaveChangesAsync();

        var updatedAtAfterCreate = todo.UpdatedAt;

        // Act — save without modifying the entity
        await context.SaveChangesAsync();

        // Assert — UpdatedAt should remain unchanged (only modified entities get UpdatedAt bumped)
        Assert.Equal(updatedAtAfterCreate, todo.UpdatedAt);
    }
}
