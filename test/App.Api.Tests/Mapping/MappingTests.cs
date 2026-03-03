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
            Url = new Uri("https://pokeapi.co/api/v2/pokemon/25/", UriKind.RelativeOrAbsolute)
        };

        // Act
        var response = pokemon.ToResponse();

        // Assert
        Assert.Equal(pokemon.Name, response.Name);
        Assert.Equal(pokemon.Url, response.Url);
    }

    [Fact]
    public void ToResponse_TodoEntity_WithNullOptionalFields_ShouldMapCorrectly()
    {
        // Arrange — DueBy and UpdatedAt are intentionally null
        var entity = new TodoEntity
        {
            Id = 5,
            Title = "Minimal Todo",
            IsCompleted = false
        };

        // Act
        var response = entity.ToResponse();

        // Assert
        Assert.Equal(entity.Id, response.Id);
        Assert.Equal(entity.Title, response.Title);
        Assert.False(response.IsCompleted);
        Assert.Null(response.DueBy);
        Assert.Null(response.UpdatedAt);
        Assert.NotEqual(default, response.CreatedAt);
    }

    [Fact]
    public void ToResponse_Pokemon_WithNullUrl_ShouldFallBackToAboutBlank()
    {
        // Arrange — Pokemon with a null Url (defensive fallback in PokemonMappingExtensions)
        var pokemon = new App.Core.Poke.Pokemon
        {
            Name = "missingno",
            Url = null
        };

        // Act
        var response = pokemon.ToResponse();

        // Assert
        Assert.Equal("missingno", response.Name);
        Assert.Equal(new Uri("about:blank"), response.Url);
    }
}
