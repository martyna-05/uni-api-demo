using Microsoft.EntityFrameworkCore;
using RfidApi.Models;
using System.Linq; // ✅ REQUIRED for Any()

var builder = WebApplication.CreateBuilder(args);

// SQLite connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=rfid.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        db.Users.Add(new Users
        {
            Username = "admin",
            Password = "admin"
        });

        db.SaveChanges();
    }
}

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
    {
        return Results.Json(
            new { success = false },
            statusCode: StatusCodes.Status401Unauthorized
        );
    }

    return Results.Ok(new { success = true });
});


app.Run();
