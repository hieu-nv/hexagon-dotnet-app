using App.Core.Entities;
using Xunit;

namespace App.Core.Tests.Entities;

public class EntityTests
{
    private class TestEntity : Entity<int> { }

    [Fact]
    public void Entity_ShouldInitializeWithCreatedAt()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        Assert.NotEqual(default, entity.CreatedAt);
        Assert.True(entity.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Entity_ShouldAllowSettingProperties()
    {
        // Arrange
        var entity = new TestEntity();
        var id = 42;
        var updatedAt = DateTime.UtcNow;

        // Act
        entity.Id = id;
        entity.UpdatedAt = updatedAt;

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.Equal(updatedAt, entity.UpdatedAt);
    }

    [Fact]
    public void TodoEntity_ShouldInitializeCorrectly()
    {
        // Act
        var todo = new TodoEntity
        {
            Title = "Test",
            IsCompleted = false
        };

        // Assert
        Assert.Equal("Test", todo.Title);
        Assert.False(todo.IsCompleted);
        Assert.NotEqual(default, todo.CreatedAt);
    }
}
