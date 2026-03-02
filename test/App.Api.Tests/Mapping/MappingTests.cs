using App.Api.Poke;
using App.Api.Todo;
using App.Core.Entities;
using Xunit;

namespace App.Api.Tests.Mapping;

public class MappingTests
{
    [Fact]
    public void ToResponse_TodoEntity_ShouldMapCorrectly()
    {
        // Arrange
        var entity = new TodoEntity
        {
            Id = 1,
            Title = "Test Todo",
            IsCompleted = true,
            DueBy = DateOnly.FromDateTime(DateTime.Now),
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.Equal(entity.Id, response.Id);
        Assert.Equal(entity.Title, response.Title);
        Assert.Equal(entity.IsCompleted, response.IsCompleted);
        Assert.Equal(entity.DueBy, response.DueBy);
        Assert.Equal(entity.CreatedAt, response.CreatedAt);
        Assert.Equal(entity.UpdatedAt, response.UpdatedAt);
    }

    [Fact]
    public void ToResponse_Pokemon_ShouldMapCorrectly()
    {
        // Arrange
        var pokemon = new App.Core.Poke.Pokemon
        {
            Name = "pikachu",
            Url = "https://pokeapi.co/api/v2/pokemon/25/"
        };

        // Act
        var response = pokemon.ToResponse();

        // Assert
        Assert.Equal(pokemon.Name, response.Name);
        Assert.Equal(pokemon.Url, response.Url);
    }
}
