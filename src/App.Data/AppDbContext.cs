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
    /// Saves all changes made in this context to the database.
    /// Automatically updates the UpdatedAt timestamp for modified entities.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries<Entity<int>>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

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
                    DueBy = new DateOnly(2026, 2, 15),
                    IsCompleted = false,
                },
                new TodoEntity
                {
                    Id = 3,
                    Title = "Do the laundry",
                    DueBy = new DateOnly(2026, 2, 16),
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
                    DueBy = new DateOnly(2026, 2, 17),
                    IsCompleted = false,
                }
            );
    }
}
