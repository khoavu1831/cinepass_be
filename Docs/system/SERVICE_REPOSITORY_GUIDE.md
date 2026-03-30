# CinePass Services & Repository Pattern Guide

## Architecture Overview

CinePass follows the **Repository + Service** pattern:

```
API Request
    ↓
Controller
    ↓
Service (Business Logic)
    ↓
Repository (Data Access)
    ↓
Database
```

**Folder Structure:**
```
Services/
├── Auth/
│   ├── IAuthService.cs
│   └── AuthService.cs
├── User/
│   ├── IUserService.cs
│   └── UserService.cs
├── Movie/
│   ├── IMovieService.cs
│   └── MovieService.cs
├── Review/
│   ├── IReviewService.cs
│   └── ReviewService.cs
├── Comment/
│   ├── ICommentService.cs
│   └── CommentService.cs
└── Follow/
    ├── IFollowService.cs
    └── FollowService.cs

Repositories/
├── Base/
│   ├── IRepository.cs
│   └── Repository.cs
├── User/
│   ├── IUserRepository.cs
│   └── UserRepository.cs
├── Movie/
│   ├── IMovieRepository.cs
│   └── MovieRepository.cs
├── Review/
│   ├── IReviewRepository.cs
│   └── ReviewRepository.cs
└── ... (other entities)
```

---

## 📋 Base Repository Pattern

### IRepository<T> (Generic Interface)
```csharp
public interface IRepository<T> where T : class
{
    // Read Operations
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    
    // Pagination
    Task<(List<T> Items, int Total)> GetPagedAsync(
        int page, 
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

    // Write Operations
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteAsync(int id);
    Task<int> SaveChangesAsync();

    // Bulk Operations
    Task<List<T>> CreateManyAsync(List<T> entities);
    Task DeleteManyAsync(List<T> entities);
}
```

---

### Repository<T> (Generic Implementation)
```csharp
public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public virtual async Task<List<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public virtual async Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<T> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await SaveChangesAsync();
        return entity;
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) await DeleteAsync(entity);
    }

    public virtual async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
```

---

## 🔐 Auth Service

### IAuthService
```csharp
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task LogoutAsync(int userId);
    Task<bool> ValidateTokenAsync(string token);
}
```

### AuthService Implementation
```csharp
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Validate unique email/username
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already registered");

        // Create user with hashed password
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(null, request.Password),
            CreatedAt = DateTime.UtcNow
        };

        user = await _userRepository.CreateAsync(user);

        // Generate tokens
        var response = await _tokenService.GenerateTokensAsync(user);
        return response;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials");

        var response = await _tokenService.GenerateTokensAsync(user);
        return response;
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var refreshToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken);
        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiryDate < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");

        // Revoke old token
        await _userRepository.RevokeRefreshTokenAsync(request.RefreshToken);

        // Generate new tokens
        var response = await _tokenService.GenerateTokensAsync(user);
        return response;
    }

    public async Task LogoutAsync(int userId)
    {
        // Revoke all refresh tokens for this user
        await _userRepository.RevokeAllUserTokensAsync(userId);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var result = _tokenService.ValidateToken(token);
            return result;
        }
        catch
        {
            return false;
        }
    }
}
```

---

## 👤 User Service & Repository

### IUserRepository
```csharp
public interface IUserRepository : IRepository<User>
{
    // User-specific queries
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<List<User>> GetFollowersAsync(int userId, int page, int pageSize);
    Task<List<User>> GetFollowingAsync(int userId, int page, int pageSize);
    
    // Refresh Token management
    Task AddRefreshTokenAsync(RefreshToken token);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task RevokeAllUserTokensAsync(int userId);
    Task<bool> IsRefreshTokenValidAsync(string token);

    // Stats
    Task<int> GetFollowerCountAsync(int userId);
    Task<int> GetFollowingCountAsync(int userId);
    Task<int> GetReviewCountAsync(int userId);
    Task UpdateFollowerCountAsync(int userId);
    Task UpdateFollowingCountAsync(int userId);
    Task UpdateReviewCountAsync(int userId);
}
```

### IUserService
```csharp
public interface IUserService
{
    Task<UserDto> GetByIdAsync(int id);
    Task<UserProfileDto> GetProfileAsync(int userId, int? currentUserId = null);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequestDto request);
    Task DeleteAsync(int id);
    Task<PaginatedResponseDto<UserFollowStatsDto>> GetFollowersAsync(int userId, int page, int pageSize);
    Task<PaginatedResponseDto<UserFollowStatsDto>> GetFollowingAsync(int userId, int page, int pageSize);
}
```

### UserService Implementation
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id) 
            ?? throw new KeyNotFoundException($"User {id} not found");
        
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId, int? currentUserId = null)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found");

        var dto = _mapper.Map<UserProfileDto>(user);
        
        if (currentUserId.HasValue)
        {
            // Check if current user follows this user
            var isFollowing = await _userRepository.IsFollowingAsync(currentUserId.Value, userId);
            dto.IsFollowing = isFollowing;
        }

        return dto;
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found");

        user.Bio = request.Bio ?? user.Bio;
        user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        user = await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteAsync(int id)
    {
        await _userRepository.DeleteAsync(id);
    }

    public async Task<PaginatedResponseDto<UserFollowStatsDto>> GetFollowersAsync(int userId, int page, int pageSize)
    {
        var (items, total) = await _userRepository.GetPagedAsync(
            page, pageSize,
            predicate: u => u.FollowersCollection.Any(f => f.FollowingId == userId)
        );

        return new PaginatedResponseDto<UserFollowStatsDto>
        {
            Data = _mapper.Map<List<UserFollowStatsDto>>(items),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
```

---

## ⭐ Review Service & Repository

### IReviewRepository
```csharp
public interface IReviewRepository : IRepository<Review>
{
    Task<Review?> GetByUserMovieAsync(int userId, int movieId);
    Task<List<Review>> GetByUserAsync(int userId, int page, int pageSize);
    Task<List<Review>> GetByMovieAsync(int movieId, int page, int pageSize);
    Task<List<Review>> GetPopularAsync(int page, int pageSize); // By likes
    Task<List<Review>> GetRecentAsync(int page, int pageSize);  // By date
    Task<Review> GetWithRelationsAsync(int id);
    
    // Stats
    Task<decimal> GetAverageRatingForMovieAsync(int movieId);
    Task<int> GetReviewCountForMovieAsync(int movieId);
    Task IncrementLikeCountAsync(int reviewId);
    Task DecrementLikeCountAsync(int reviewId);
    Task IncrementCommentCountAsync(int reviewId);
    Task DecrementCommentCountAsync(int reviewId);
}
```

### IReviewService
```csharp
public interface IReviewService
{
    Task<ReviewDetailDto> CreateAsync(int userId, CreateReviewRequestDto request);
    Task<ReviewDetailDto> GetAsync(int id, int? currentUserId = null);
    Task<ReviewDetailDto> UpdateAsync(int id, int userId, UpdateReviewRequestDto request);
    Task DeleteAsync(int id, int userId);
    
    Task<PaginatedResponseDto<ReviewListItemDto>> GetByMovieAsync(int movieId, int page, int pageSize, int? currentUserId = null);
    Task<PaginatedResponseDto<ReviewListItemDto>> GetByUserAsync(int userId, int page, int pageSize);
    Task<PaginatedResponseDto<ReviewListItemDto>> GetPopularAsync(int page, int pageSize, int? currentUserId = null);
    Task<PaginatedResponseDto<ReviewListItemDto>> GetRecentAsync(int page, int pageSize, int? currentUserId = null);
}
```

### ReviewService Implementation (Excerpt)
```csharp
public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public async Task<ReviewDetailDto> CreateAsync(int userId, CreateReviewRequestDto request)
    {
        // Validate movie exists
        var movie = await _movieRepository.GetByIdAsync(request.MovieId)
            ?? throw new KeyNotFoundException($"Movie {request.MovieId} not found");

        // Check user hasn't already reviewed
        var existing = await _reviewRepository.GetByUserMovieAsync(userId, request.MovieId);
        if (existing != null)
            throw new InvalidOperationException("User already reviewed this movie");

        var review = new Review
        {
            UserId = userId,
            MovieId = request.MovieId,
            Title = request.Title,
            Content = request.Content,
            Rating = request.Rating,
            HasSpoiler = request.HasSpoiler,
            CreatedAt = DateTime.UtcNow
        };

        review = await _reviewRepository.CreateAsync(review);

        // Update movie stats
        await _movieRepository.UpdateAverageRatingAsync(request.MovieId);
        
        // Update user review count
        await _movieRepository.IncrementMovieReviewCountAsync(request.MovieId);

        return await GetAsync(review.Id, userId);
    }

    public async Task<ReviewDetailDto> GetAsync(int id, int? currentUserId = null)
    {
        var review = await _reviewRepository.GetWithRelationsAsync(id)
            ?? throw new KeyNotFoundException($"Review {id} not found");

        var dto = _mapper.Map<ReviewDetailDto>(review);

        if (currentUserId.HasValue)
        {
            var userLiked = await _likeRepository.IsLikedAsync(currentUserId.Value, id);
            dto.CurrentUserLiked = userLiked;
        }

        return dto;
    }
}
```

---

## 💬 Comment Service & Repository

### ICommentRepository
```csharp
public interface ICommentRepository : IRepository<Comment>
{
    Task<List<Comment>> GetByReviewAsync(int reviewId, int page, int pageSize);
    Task<Comment> GetWithAuthorAsync(int id);
    Task<int> GetCountByReviewAsync(int reviewId);
}
```

### ICommentService
```csharp
public interface ICommentService
{
    Task<CommentDto> CreateAsync(int reviewId, int userId, CreateCommentRequestDto request);
    Task<CommentDto> GetAsync(int id);
    Task<CommentDto> UpdateAsync(int id, int userId, UpdateCommentRequestDto request);
    Task DeleteAsync(int id, int userId);
    Task<PaginatedResponseDto<CommentDto>> GetByReviewAsync(int reviewId, int page, int pageSize);
}
```

---

## 👥 Follow Service & Repository

### IFollowRepository
```csharp
public interface IFollowRepository : IRepository<Follow>
{
    Task<Follow?> GetFollowAsync(int followerId, int followingId);
    Task<bool> IsFollowingAsync(int followerId, int followingId);
    Task<List<Follow>> GetFollowersAsync(int userId, int page, int pageSize);
    Task<List<Follow>> GetFollowingAsync(int userId, int page, int pageSize);
    Task<int> GetFollowerCountAsync(int userId);
    Task<int> GetFollowingCountAsync(int userId);
}
```

### IFollowService
```csharp
public interface IFollowService
{
    Task<bool> FollowAsync(int followerId, int followingId);
    Task<bool> UnfollowAsync(int followerId, int followingId);
    Task<UserFollowStatsDto> GetStatsAsync(int userId, int? currentUserId = null);
}
```

---

## 🎬 Movie Service & Repository

### IMovieRepository
```csharp
public interface IMovieRepository : IRepository<Movie>
{
    Task<Movie?> GetByTmdbIdAsync(int tmdbId);
    Task<(List<Movie> Items, int Total)> SearchAsync(string query, int page, int pageSize);
    Task<(List<Movie> Items, int Total)> GetTopRatedAsync(int page, int pageSize);
    Task<(List<Movie> Items, int Total)> GetRecentAsync(int page, int pageSize);
    
    Task UpdateAverageRatingAsync(int movieId);
    Task IncrementMovieReviewCountAsync(int movieId);
    Task DecrementMovieReviewCountAsync(int movieId);
}
```

### IMovieService
```csharp
public interface IMovieService
{
    Task<MovieDto> GetAsync(int id);
    Task<PaginatedResponseDto<MovieListItemDto>> GetAllAsync(int page, int pageSize);
    Task<PaginatedResponseDto<MovieListItemDto>> SearchAsync(string query, int page, int pageSize);
    Task<PaginatedResponseDto<MovieListItemDto>> GetTopRatedAsync(int page, int pageSize);
    Task<MovieDto> CreateFromTmdbAsync(int tmdbId);
}
```

---

## 🔄 Like Service & Repository

### ILikeRepository
```csharp
public interface ILikeRepository : IRepository<Like>
{
    Task<Like?> GetUserReviewLikeAsync(int userId, int reviewId);
    Task<bool> IsLikedAsync(int userId, int reviewId);
    Task<int> GetLikeCountAsync(int reviewId);
    Task<List<Review>> GetLikedReviewsAsync(int userId, int page, int pageSize);
}
```

### ILikeService
```csharp
public interface ILikeService
{
    Task<bool> LikeReviewAsync(int userId, int reviewId);
    Task<bool> UnlikeReviewAsync(int userId, int reviewId);
    Task<bool> IsLikedAsync(int userId, int reviewId);
    Task<PaginatedResponseDto<ReviewListItemDto>> GetLikedAsync(int userId, int page, int pageSize);
}
```

---

## 🔑 Token Service

### ITokenService
```csharp
public interface ITokenService
{
    Task<AuthResponseDto> GenerateTokensAsync(User user);
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    Task<bool> IsRefreshTokenValidAsync(string refreshToken);
}
```

### TokenService Implementation
```csharp
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;

    public async Task<AuthResponseDto> GenerateTokensAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Store refresh token in database
        var expiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiry"] ?? "7");
        var rtEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddRefreshTokenAsync(rtEntity);

        var expirySeconds = int.Parse(_configuration["Jwt:AccessTokenExpiry"] ?? "900");
        return new AuthResponseDto
        {
            UserId = user.Id,
            Username = user.Username,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddSeconds(expirySeconds)
        };
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var expirySeconds = int.Parse(_configuration["Jwt:AccessTokenExpiry"] ?? "900");
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(expirySeconds),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
```

---

## 📋 Dependency Injection (Program.cs)

```csharp
// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<ILikeService, LikeService>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();

// Password hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
```

---

## 🎯 Best Practices

1. **Repository:**
   - Single responsibility: Data access only
   - Use async/await throughout
   - Return DTOs from services, not repositories
   - Include eager loading (Include/ThenInclude) for relations

2. **Service:**
   - Contains business logic
   - Validates inputs
   - Orchestrates multiple repositories
   - Throws meaningful exceptions
   - Maps entities to DTOs via AutoMapper

3. **Exceptions:**
   - Use custom exceptions for business logic
   - Catch and handle appropriately in controllers
   - Return meaningful error responses

4. **Async Patterns:**
   - Make all I/O operations async
   - Use ConfigureAwait(false) in libraries
   - Never use .Result or .Wait()

5. **Testing:**
   - Mock interfaces for unit tests
   - Test service logic independently
   - Use repositories for integration tests
