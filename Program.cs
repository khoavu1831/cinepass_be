using System.Text;
using CinePass_be.Authorization;
using CinePass_be.Data;
using CinePass_be.Repositories;
using CinePass_be.Services;
using CinePass_be.Clients.Tmdb;
using CinePass_be.Middleware;
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

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
  options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN", "SUPERADMIN"));
  options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SUPERADMIN"));
});

// DI
// DI - Auth
builder.Services.AddSingleton<IAuthorizationHandler, SelfHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

// DI - Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();

// DI - Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ICollectionService, CollectionService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // context.Database.Migrate();

    if (!context.Users.Any(u => u.Role == CinePass_be.Models.UserRole.SUPERADMIN))
    {
        context.Users.Add(new CinePass_be.Models.User
        {
            Username = "admin",
            Email = "admin@cinepass.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            Role = CinePass_be.Models.UserRole.SUPERADMIN,
            IsActive = true
        });
        context.SaveChanges();
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowFrontend");

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