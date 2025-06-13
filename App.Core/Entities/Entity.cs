using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    [Column("ID")]
    public T? Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created
    /// </summary>
    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated
    /// </summary>
    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }
}
