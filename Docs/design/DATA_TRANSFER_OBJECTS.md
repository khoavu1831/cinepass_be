# CinePass Data Transfer Objects (DTOs)

## Overview
DTOs are lightweight objects used for API communication. They:
- Transfer data between client and server
- Hide internal model structure
- Provide validation contracts
- Are immutable and serializable

**Folder Structure:**
```
DTOs/
├── Auth/
│   ├── LoginRequestDto.cs
│   ├── RegisterRequestDto.cs
│   ├── RefreshTokenRequestDto.cs
│   └── AuthResponseDto.cs
├── User/
│   ├── UserDto.cs
│   ├── UserProfileDto.cs
│   └── UpdateUserRequestDto.cs
├── Movie/
│   ├── MovieDto.cs
│   ├── MovieListItemDto.cs
│   └── CreateMovieRequestDto.cs
├── Review/
│   ├── ReviewDto.cs
│   ├── ReviewListItemDto.cs
│   ├── CreateReviewRequestDto.cs
│   ├── UpdateReviewRequestDto.cs
│   └── ReviewDetailDto.cs
├── Comment/
│   ├── CommentDto.cs
│   ├── CreateCommentRequestDto.cs
│   └── UpdateCommentRequestDto.cs
└── Common/
    ├── PaginationDto.cs
    ├── PaginatedResponseDto.cs
    └── ErrorResponseDto.cs
```

---

## 🔐 Auth DTOs

### LoginRequestDto
```csharp
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
```
**Validation:**
- Email: Required, valid email format
- Password: Required, minimum 6 characters

---

### RegisterRequestDto
```csharp
public class RegisterRequestDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])", 
        ErrorMessage = "Password must contain uppercase, lowercase, digit, and special character")]
    public string Password { get; set; } = string.Empty;
}
```
**Validation:**
- Username: 3-50 characters, unique
- Email: Valid format, unique
- Password: 8+ chars, uppercase, lowercase, digit, special char

---

### RefreshTokenRequestDto
```csharp
public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
```

---

### AuthResponseDto
```csharp
public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
}
```

---

## 👤 User DTOs

### UserDto
```csharp
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

### UserProfileDto
```csharp
public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFollowing { get; set; }  // Current user following?
    public DateTime CreatedAt { get; set; }
}
```
**Note:** Email and Role not included for privacy

---

### UpdateUserRequestDto
```csharp
public class UpdateUserRequestDto
{
    [MaxLength(500)]
    public string? Bio { get; set; }

    [Url]
    public string? AvatarUrl { get; set; }
}
```

---

## 🎬 Movie DTOs

### MovieDto
```csharp
public class MovieDto
{
    public int Id { get; set; }
    public int? TmdbId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? LocalTitle { get; set; }
    public string? Description { get; set; }
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public int? Duration { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Director { get; set; }
    public string? Cast { get; set; }
    public List<string> Genres { get; set; } = [];
    public decimal RatingAvg { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

### MovieListItemDto
```csharp
public class MovieListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? LocalTitle { get; set; }
    public string? PosterUrl { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public decimal RatingAvg { get; set; }
    public int ReviewCount { get; set; }
    public List<string> Genres { get; set; } = [];
}
```
**Use:** For list endpoints, lighter payload

---

### CreateMovieRequestDto
```csharp
public class CreateMovieRequestDto
{
    [Required]
    public int TmdbId { get; set; }
    
    // Other fields auto-fetched from TMDB
}
```
**Note:** Most fields populated from TMDB API

---

## ⭐ Review DTOs

### ReviewDto
```csharp
public class ReviewDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public bool HasSpoiler { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

### ReviewListItemDto
```csharp
public class ReviewListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;  // Truncated
    public decimal Rating { get; set; }
    public bool HasSpoiler { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public UserProfileDto Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
```

---

### CreateReviewRequestDto
```csharp
public class CreateReviewRequestDto
{
    [Required]
    public int MovieId { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Range(1, 10)]
    public decimal Rating { get; set; }

    public bool HasSpoiler { get; set; } = false;
}
```
**Validation:**
- MovieId: Required, valid movie
- Title: 3-200 characters
- Content: 10-5000 characters
- Rating: 1-10
- HasSpoiler: Optional

---

### UpdateReviewRequestDto
```csharp
public class UpdateReviewRequestDto
{
    [MinLength(3)]
    [MaxLength(200)]
    public string? Title { get; set; }

    [MinLength(10)]
    [MaxLength(5000)]
    public string? Content { get; set; }

    [Range(1, 10)]
    public decimal? Rating { get; set; }

    public bool? HasSpoiler { get; set; }
}
```

---

### ReviewDetailDto
```csharp
public class ReviewDetailDto
{
    public int Id { get; set; }
    public UserProfileDto Author { get; set; } = null!;
    public MovieListItemDto Movie { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public bool HasSpoiler { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool CurrentUserLiked { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 💬 Comment DTOs

### CommentDto
```csharp
public class CommentDto
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public UserProfileDto Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

### CreateCommentRequestDto
```csharp
public class CreateCommentRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;
}
```

---

### UpdateCommentRequestDto
```csharp
public class UpdateCommentRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;
}
```

---

## 📦 Common DTOs

### PaginationDto
```csharp
public class PaginationDto
{
    public int Page { get; set; } = 1;
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public string? SortBy { get; set; } = "CreatedAt";
    
    public string Order { get; set; } = "desc";
}
```

---

### PaginatedResponseDto<T>
```csharp
public class PaginatedResponseDto<T>
{
    public List<T> Data { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (Total + PageSize - 1) / PageSize;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

---

### ErrorResponseDto
```csharp
public class ErrorResponseDto
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<ErrorDetailDto>? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ErrorDetailDto
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
```

---

## 🔄 Follow DTOs

### FollowResponseDto
```csharp
public class FollowResponseDto
{
    public int FollowerId { get; set; }
    public int FollowingId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

### UserFollowStatsDto
```csharp
public class UserFollowStatsDto
{
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public bool IsFollowing { get; set; }
}
```

---

## 🔍 Search DTOs

### SemanticSearchRequestDto
```csharp
public class SemanticSearchRequestDto : PaginationDto
{
    [Required]
    [MinLength(3)]
    public string Query { get; set; } = string.Empty;

    public decimal? MinSimilarity { get; set; } = 0.7m;
}
```

---

### SemanticSearchResultDto
```csharp
public class SemanticSearchResultDto
{
    public int ReviewId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string ReviewTitle { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public decimal Similarity { get; set; }  // 0.0 - 1.0
    public UserProfileDto Author { get; set; } = null!;
}
```

---

## 📋 DTO Best Practices

1. **Request DTOs:**
   - Include only writable fields
   - Add [Required] for mandatory fields
   - Add validation attributes

2. **Response DTOs:**
   - Include full object graph for single GET
   - Use lightweight versions for lists
   - Hide sensitive data (PasswordHash, etc.)

3. **Naming Convention:**
   - Request: `Create{Entity}RequestDto`, `Update{Entity}RequestDto`
   - Response: `{Entity}Dto`, `{Entity}ListItemDto`, `{Entity}DetailDto`
   - Custom: `{Entity}ResponseDto`, `SemanticSearchRequestDto`

4. **Mapping:**
   - Use AutoMapper for DTO ↔ Model conversion
   - Avoid complex logic in DTOs
   - Keep DTOs focused on API contracts

5. **Versioning:**
   - Prefix DTOs with version if needed: `UserDtoV2`
   - Maintain backward compatibility
   - Deprecate gradually
