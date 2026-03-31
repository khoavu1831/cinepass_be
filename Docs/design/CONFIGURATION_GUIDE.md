# CinePass Configuration Guide

## Project Setup & Configuration

---

## 📋 appsettings.json

Main configuration file for production/staging environments.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=cinepass_db;User=root;Password=YourPassword;"
  },

  "Jwt": {
    "Key": "your-super-secret-key-must-be-at-least-32-characters-long!",
    "Issuer": "cinepass-api",
    "Audience": "cinepass-users",
    "AccessTokenExpiry": 900,      // 15 minutes (seconds)
    "RefreshTokenExpiry": 604800   // 7 days (seconds)
  },

  "Database": {
    "MaxConnectionAttempts": 5,
    "InitialConnectionDelay": 1000  // milliseconds
  },

  "ExternalApis": {
    "Tmdb": {
      "BaseUrl": "https://api.themoviedb.org/3",
      "ApiKey": "your-tmdb-api-key",
      "ImageBaseUrl": "https://image.tmdb.org/t/p"
    },
    "OpenAi": {
      "ApiKey": "your-openai-api-key",
      "EmbeddingModel": "text-embedding-3-small",
      "Dimensions": 1536
    }
  },

  "Features": {
    "EnableSemanticSearch": true,
    "EnableUserFollows": true,
    "EnableComments": true,
    "MaxReviewLength": 5000,
    "MaxCommentLength": 500
  },

  "Security": {
    "EnableCors": true,
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:3001"
    ],
    "RequireHttpsMetadata": false,
    "TokenValidationParameters": {
      "ValidateIssuerSigningKey": true,
      "ValidateIssuer": true,
      "ValidateAudience": true,
      "ValidateLifetime": true,
      "ClockSkew": 0
    }
  },

  "RateLimiting": {
    "Enabled": true,
    "PermitLimit": 100,
    "WindowSize": 900  // 15 minutes (seconds)
  }
}
```

---

## 📋 appsettings.Development.json

Development-specific overrides.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Debug",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  },

  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=cinepass_dev;User=root;Password=root;"
  },

  "Jwt": {
    "Key": "dev-secret-key-this-must-be-at-least-32-chars-long!",
    "AccessTokenExpiry": 3600,      // 1 hour for development
    "RefreshTokenExpiry": 2592000   // 30 days
  },

  "ExternalApis": {
    "Tmdb": {
      "ApiKey": "dev-tmdb-key"
    },
    "OpenAi": {
      "ApiKey": "dev-openai-key"
    }
  },

  "Security": {
    "RequireHttpsMetadata": false,
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:3001",
      "http://localhost:5173"
    ]
  },

  "RateLimiting": {
    "Enabled": false  // Disable in development for testing
  }
}
```

---

## 🔧 Program.cs - Complete Configuration

```csharp
using CinePass_be.Data;
using CinePass_be.Services.Auth;
using CinePass_be.Services.User;
using CinePass_be.Repositories.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============ Configuration ============
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

// ============ DbContext ============
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure()
    )
);

// ============ Controllers & API ============
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============ Authentication ============
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
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Handle token expiration gracefully
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

// ============ Authorization ============
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("ADMIN")
    );
    options.AddPolicy("ModeratorOrAdmin", policy =>
        policy.RequireRole("MODERATOR", "ADMIN")
    );
});

// ============ Password Hasher ============
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// ============ Services ============
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
// Add other services here

// ============ Repositories ============
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Add other repositories here

// ============ CORS ============
var allowedOrigins = builder.Configuration.GetSection("Security:AllowedOrigins").Get<string[]>()?
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ============ AutoMapper ============
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ============ Rate Limiting ============
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        policyName: "default",
        configure: opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromSeconds(900);
            opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        }
    );
});

// ============ Health Checks ============
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// ============ Build App ============
var app = builder.Build();

// ============ Middleware ============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Exception handling middleware
app.UseExceptionHandler("/error");

// HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS
app.UseCors("AllowFrontend");

// Rate limiting
app.UseRateLimiter();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health checks endpoint
app.MapHealthChecks("/health");

// API routes
app.MapControllers();

// 404 handler
app.MapFallback(() => Results.NotFound(new { error = "Endpoint not found" }));

// ============ Database Initialization ============
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    try
    {
        // Run migrations
        await db.Database.MigrateAsync();
        Console.WriteLine("✅ Database migrated successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database migration failed: {ex.Message}");
        throw;
    }
}

app.Run();
```

---

## 🗄️ Database Configuration

### MySQL Connection String Format
```
Server=<host>;Port=<port>;Database=<database>;User=<user>;Password=<password>;
```

**Examples:**
```
// Local development
Server=localhost;Port=3306;Database=cinepass_dev;User=root;Password=root;

// Remote production
Server=prod-db.example.com;Port=3306;Database=cinepass;User=prod_user;Password=SecurePassword123!;

// With SSL
Server=prod-db.example.com;Port=3306;Database=cinepass;User=prod_user;Password=pass;SslMode=Required;
```

### Entity Framework Configuration
```csharp
// In appsettings.json - Retry on failure for transient errors
options.UseMySql(
    connectionString,
    ServerVersion.AutoDetect(connectionString),
    mysqlOptions => mysqlOptions
        .EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelaySeconds: 10,
            errorNumbersToAdd: null
        )
);
```

---

## 🔐 JWT & Security Configuration

### Generate Strong JWT Key
```bash
# PowerShell
[Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes([guid]::NewGuid().ToString() + [guid]::NewGuid().ToString()))

# Linux/Mac
openssl rand -base64 32

# Python
python3 -c "import secrets; print(secrets.token_urlsafe(32))"
```

### Token Expiry Guide
```
AccessToken:  900s  (15 min)  - Short-lived, requires refresh frequent
            or 3600s (1 hour) - Development
            or 1800s (30 min) - Balance

RefreshToken: 604800s (7 days)   - Standard
            or 2592000s (30 days) - Long sessions
            or 86400s (1 day)     - High security
```

---

## 🚀 Environment Variables

### Development
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5001
```

### Production
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443
ConnectionStrings__DefaultConnection=<production-db-string>
Jwt__Key=<production-jwt-key>
Jwt__Issuer=cinepass-api
Jwt__Audience=cinepass-users
ExternalApis__Tmdb__ApiKey=<tmdb-key>
ExternalApis__OpenAi__ApiKey=<openai-key>
```

---

## 📝 User Secrets (Development)

Store sensitive data locally without committing to git:

```bash
# Initialize user secrets
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "Jwt:Key" "your-secret-key"
dotnet user-secrets set "ExternalApis:Tmdb:ApiKey" "tmdb-key"
dotnet user-secrets set "ExternalApis:OpenAi:ApiKey" "openai-key"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "connection-string"

# List all secrets
dotnet user-secrets list

# Remove secret
dotnet user-secrets remove "Jwt:Key"

# Clear all secrets
dotnet user-secrets clear
```

---

## 🗂️ File Structure for Secrets

**Development (local):**
- `appsettings.json` - Default/public config
- `appsettings.Development.json` - Dev overrides
- User Secrets - Sensitive keys (not in git)
- `.gitignore` - Excludes sensitive files

**Production (server):**
- `appsettings.json` - Default config
- Environment Variables - Sensitive keys
- Or Docker Secrets/K8s Secrets

---

## ✅ Configuration Checklist

- [ ] JWT Key set (32+ characters)
- [ ] Database connection string configured
- [ ] CORS origins set correctly
- [ ] External API keys stored (user-secrets)
- [ ] Environment set (Development/Production)
- [ ] Rate limiting configured
- [ ] Logging levels set appropriately
- [ ] Security headers configured
- [ ] HTTPS enabled in production
- [ ] Token expiry times appropriate for use case

---

## 🐛 Common Configuration Issues

### Issue: "No JWT key configured"
**Solution:** Ensure `Jwt:Key` is set in appsettings.json or user-secrets

### Issue: "Database connection failed"
**Solution:** Check connection string format and MySQL is running
```bash
mysql -u root -p  # Test connection
```

### Issue: "CORS errors from frontend"
**Solution:** Add frontend URL to `Security:AllowedOrigins` in appsettings

### Issue: "Token validation failed"
**Solution:** Ensure token generated with same key/issuer as validation

### Issue: "Refresh token expiration too short"
**Solution:** Increase `Jwt:RefreshTokenExpiry` in appsettings.json

---

## 📊 Logging Configuration

### Log Levels
```
Trace = 0       // Very detailed - only use for debugging
Debug = 1       // Detailed info for debugging
Information = 2 // General info about app flow
Warning = 3     // Warning messages
Error = 4       // Error messages
Critical = 5    // Critical failures
None = 6        // No logging
```

### Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "CinePass_be": "Debug"
    }
  }
}
```

---

## 🚀 Deployment Configuration

### For Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=builder /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80
ENTRYPOINT ["dotnet", "CinePass_be.dll"]
```

### Environment Variables in Docker
```bash
docker run -e "Jwt__Key=value" \
           -e "ConnectionStrings__DefaultConnection=server=db;..." \
           -e "ASPNETCORE_ENVIRONMENT=Production" \
           cinepass-api:latest
```

---

## 💾 Database Migrations

### Create Migration
```bash
dotnet ef migrations add InitialCreate
```

### Apply Migration
```bash
dotnet ef database update
```

### Rollback Migration
```bash
dotnet ef database update PreviousMigrationName
```

### Remove Migration
```bash
dotnet ef migrations remove
```
