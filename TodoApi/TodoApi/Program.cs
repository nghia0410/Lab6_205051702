using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
var app = builder.Build();
app.MapGet("/todoitems", async (TodoDb db) =>
 await db.Todos.Select(x => new TodoItemDTO(x)).ToListAsync());
app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
 await db.Todos.FindAsync(id)
 is Todo todo
 ? Results.Ok(new TodoItemDTO(todo))
 : Results.NotFound());
app.MapPost("/todoitems", async (TodoItemDTO todoItemDTO, TodoDb db) =>
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };
    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todoItem.Id}", new TodoItemDTO(todoItem));
});
app.MapPut("/todoitems/{id}", async (int id, TodoItemDTO todoItemDTO, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(new TodoItemDTO(todo));
    }
    return Results.NotFound();
});
app.Run();