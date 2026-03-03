using System.ComponentModel.DataAnnotations;

namespace App.Core.Entities;

/// <summary>
/// Base class for all entities in the application
/// </summary>
public abstract class Entity<T> : IEntity<T>
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity
    /// </summary>
    [Key]
    public T? Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
