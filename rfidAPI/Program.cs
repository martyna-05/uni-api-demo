using Microsoft.EntityFrameworkCore;
using RfidApi.Models;

var builder = WebApplication.CreateBuilder(args);

// SQLite connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=rfid.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// LOGIN ENDPOINT
app.MapPost("/login", async (LoginRequest request, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u =>
        u.Username == request.Username &&
        u.Password == request.Password);

    if (user == null)
        return Results.Unauthorized();

    return Results.Ok("Login successful");
});

app.Run();
