using System.ComponentModel.DataAnnotations;

namespace App.Data.Models;

public class Todo
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string? Title { get; set; }

    public DateOnly? DueBy { get; set; }

    public bool IsComplete { get; set; } = false;
}
