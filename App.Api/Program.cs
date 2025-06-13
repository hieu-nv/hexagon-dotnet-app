using App.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateSlimBuilder(args);

builder.UseData();

var app = builder.Build();
app.UseData();

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", async ([FromServices] AppDbContext db) => await db.Todos.ToListAsync());
todosApi.MapGet(
    "/{id}",
    async (int id, [FromServices] AppDbContext db) =>
    {
        var todo = await db.Todos.FindAsync(id);
        return todo != null ? Results.Ok(todo) : Results.NotFound();
    }
);

app.Run();
