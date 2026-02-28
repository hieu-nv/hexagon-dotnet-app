using App.Core.Entities;

namespace App.Api.Todo;

/// <summary>
/// Extension methods for mapping Todo-related entities and DTOs.
/// </summary>
public static class TodoMappingExtensions
{
    /// <summary>
    /// Maps a TodoEntity to a TodoResponse.
    /// </summary>
    /// <param name="entity">The entity to map.</param>
    /// <returns>The mapped response.</returns>
    public static TodoResponse ToResponse(this TodoEntity entity)
    {
        return new TodoResponse(
            entity.Id,
            entity.Title,
            entity.IsCompleted,
            entity.DueBy,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}
