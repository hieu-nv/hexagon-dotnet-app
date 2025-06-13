using App.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Todo> Todos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add sample todos
        modelBuilder
            .Entity<Todo>()
            .HasData(
                new Todo
                {
                    Id = 1,
                    Title = "Walk the dog",
                    IsComplete = false,
                },
                new Todo
                {
                    Id = 2,
                    Title = "Do the dishes",
                    DueBy = DateOnly.FromDateTime(DateTime.Now),
                    IsComplete = false,
                },
                new Todo
                {
                    Id = 3,
                    Title = "Do the laundry",
                    DueBy = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                    IsComplete = false,
                },
                new Todo
                {
                    Id = 4,
                    Title = "Clean the bathroom",
                    IsComplete = false,
                },
                new Todo
                {
                    Id = 5,
                    Title = "Clean the car",
                    DueBy = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                    IsComplete = false,
                }
            );
    }
}
