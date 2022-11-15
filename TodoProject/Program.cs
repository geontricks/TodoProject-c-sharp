using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDB>(
    opt => opt.UseInMemoryDatabase("Todos"));
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.InferSecuritySchemes();
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "My Auth",
                Description = "Add Swagger Security",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme,Id = JwtBearerDefaults.AuthenticationScheme }
            },
            new string[] {}
        }
    });
});
builder.Services.Configure<SwaggerGeneratorOptions>(options =>
{
    options.InferSecuritySchemes = true;
});
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", (TodoDB db) =>db.Todos.ToListAsync());

var todos = app.MapGroup("/todo").RequireAuthorization();

todos.MapPost("/", async (Todo todo, TodoDB TodoDb) =>
{
    TodoDb.Todos.Add(todo);
    await TodoDb.SaveChangesAsync();
    return TypedResults.Created($"/todos/{todo.Id}", todo);
});

app.Run();


class Todo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsCompleted { get; set; }
}

class TodoDB : DbContext
{
    public TodoDB(DbContextOptions<TodoDB> options):base(options)
    {
        //
    }
    public DbSet<Todo> Todos => Set<Todo>();
}