using CinePass_be.Data;
using CinePass_be.Repositories;
using CinePass_be.Services;
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

// DI
// DI - Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// DI - Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// App
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapGet("api/", () => "Hello cinepass");
app.MapControllers();

app.UseHttpsRedirection();

app.Run();