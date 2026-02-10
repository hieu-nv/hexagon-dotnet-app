using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Core.Entities;

/// <summary>
/// Represents a to-do item entity.
/// </summary>
public class TodoEntity : Entity<int>
{
    /// <summary>
    /// Gets or sets the title of the to-do item.
    /// </summary>
    [Required]
    [StringLength(
        200,
        MinimumLength = 1,
        ErrorMessage = "Title must be between 1 and 200 characters"
    )]
    [Column("TITLE")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the due date for the to-do item.
    /// </summary>
    [Column("DUE_BY")]
    public DateOnly? DueBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the to-do item is completed.
    /// </summary>
    [Column("IS_COMPLETED")]
    public bool IsCompleted { get; set; }
}
