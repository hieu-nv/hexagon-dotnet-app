using App.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options ?? throw new ArgumentNullException(nameof(options)))
    {
        // FIX: SA1500
    }

    public DbSet<TodoEntity> Todos { get; set; } = null!;

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
