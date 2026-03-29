using CinePass_be.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(option =>
  option.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Auth
var jwtKey = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication().AddJwtBearer();


// App
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}

app.MapGet("api/", () => "Hello cinepass");

app.UseHttpsRedirection();

app.Run();