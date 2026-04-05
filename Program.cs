using System.Text;
using CinePass_be.Authorization;
using CinePass_be.Data;
using CinePass_be.Repositories;
using CinePass_be.Services;
using CinePass_be.Clients.Tmdb;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(option =>
  option.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Auth
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.TokenValidationParameters = new TokenValidationParameters
    {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
      ValidateIssuer = true,
      ValidIssuer = jwtIssuer,
      ValidateAudience = true,
      ValidAudience = jwtAudience,
      ValidateLifetime = true
    };
  });

builder.Services.AddAuthorization(options =>
{
  options.AddPolicy("SelfOnly", policy => policy.Requirements.Add(new SelfRequirement()));
});

// DI
// DI - Auth
builder.Services.AddSingleton<IAuthorizationHandler, SelfHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

// DI - Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();

// DI - Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// DI - HttpClient and Tmdb
builder.Services.AddHttpClient<ITmdbClient, TmdbClient>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  // Define JWT Bearer security scheme
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT"
  });

  // Add security requirement to all endpoints
  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
      },
      new string[] { }
    }
  });
});

// App
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapControllers();

app.UseHttpsRedirection();

app.Run();