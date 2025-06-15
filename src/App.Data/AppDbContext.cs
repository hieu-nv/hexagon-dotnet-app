using App.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Data;

/// <summary>
/// Database context for the application, managing the TodoEntity.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AppDbContext"/> class with the specified options.
/// </remarks>
/// <param name="options"></param>
/// <exception cref="ArgumentNullException"></exception>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options ?? throw new ArgumentNullException(nameof(options)))
{
    /// <summary>
    /// Represents the collection of to-do items in the database.
    /// </summary>
    public DbSet<TodoEntity> Todos { get; set; } = null!;

    /// <summary>
    /// Configures the model for the application database context.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the model.</param>
    /// <exception cref="ArgumentNullException">Thrown when the modelBuilder is null.</exception>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<TodoEntity>()
            .HasData(
                new TodoEntity
                {
                    Id = 1,
                    Title = "Walk the dog",
                    IsCompleted = false,
                },
                new TodoEntity
                {
                    Id = 2,
                    Title = "Do the dishes",
                    DueBy = DateOnly.FromDateTime(DateTime.Now),
                    IsCompleted = false,
                },
                new TodoEntity
                {
                    Id = 3,
                    Title = "Do the laundry",
                    DueBy = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                    IsCompleted = false,
                },
                new TodoEntity
                {
                    Id = 4,
                    Title = "Clean the bathroom",
                    IsCompleted = false,
                },
                new TodoEntity
                {
                    Id = 5,
                    Title = "Clean the car",
                    DueBy = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                    IsCompleted = false,
                }
            );
    }
}
