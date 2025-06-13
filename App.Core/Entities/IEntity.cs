using System;

namespace App.Core.Entities;

/// <summary>
/// Represents a generic entity with an identifier.
/// </summary>
/// <typeparam name="T">The type of the identifier.</typeparam>
public interface IEntity<T>
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    T? Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
