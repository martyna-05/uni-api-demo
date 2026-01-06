using Microsoft.Data.SqlClient;
using RfidApi.Models;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Kestrel to listen on all IPs
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});


var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Endpoints
app.MapGet("/api/dbtest", async (IConfiguration config) =>
{
    var connStr = config.GetConnectionString("DefaultConnection");
    using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();
    return Results.Ok("Connected to SQL Server!");
});

app.MapPost("/api/login", async (LoginRequest request, IConfiguration config) =>
{
    var connStr = config.GetConnectionString("DefaultConnection");
    using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();

    var sql = @"
        SELECT COUNT(*)
        FROM dbo.Login_TB
        WHERE Username = @username
          AND Password = @password
          AND IsActive = 1
    ";

    using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.Add("@username", System.Data.SqlDbType.VarChar, 50).Value = request.Username.Trim();
    cmd.Parameters.Add("@password", System.Data.SqlDbType.VarChar, 50).Value = request.Password.Trim();

    int count = (int)await cmd.ExecuteScalarAsync();
    return Results.Ok(count > 0);
});

app.MapPost("/api/login-debug", async (LoginRequest request, IConfiguration config) =>
{
    Console.WriteLine($"Received Username: '{request.Username}'");
    Console.WriteLine($"Received Password: '{request.Password}'");

    var connStr = config.GetConnectionString("DefaultConnection");
    using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();

    var sql = """
        SELECT COUNT(*)
        FROM dbo.Login_TB
        WHERE Username = @username
          AND Password = @password
          AND IsActive = 1
    """;

    using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = request.Username.Trim();
    cmd.Parameters.Add("@password", SqlDbType.VarChar, 50).Value = request.Password.Trim();

    int count = (int)await cmd.ExecuteScalarAsync();
    Console.WriteLine($"SQL returned: {count}");

    return Results.Ok(count > 0);
});

app.UseAuthorization();
app.MapControllers();
app.Run();
