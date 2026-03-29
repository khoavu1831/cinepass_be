# CinePass API Reference - Quick Guide

## 🔑 Quick Links
- [Authentication](#authentication)
- [Users](#users)
- [Movies](#movies)
- [Reviews](#reviews)
- [Comments](#comments)
- [Likes](#likes)
- [Follows](#follows)
- [Search (Future)](#search--discovery-future)
- [Error Codes](#error-codes)

---

## Authentication

### POST /api/auth/register
**Register a new user**
```
Request:
{
  "username": "john_cinema",
  "email": "john@example.com",
  "password": "SecurePass123!"
}

Response: 201 Created
{
  "id": 1,
  "username": "john_cinema",
  "email": "john@example.com",
  "role": "USER",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "createdAt": "2026-03-29T10:30:00Z"
}

Status: 200 OK | 400 Bad Request | 409 Conflict
```

### POST /api/auth/login
**Login existing user**
```
Request:
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}

Response: 200 OK
{
  "id": 1,
  "username": "john_cinema",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 86400
}

Status: 200 OK | 401 Unauthorized
```

---

## Users

### GET /api/users/{id}
**Get user profile**
```
Response: 200 OK
{
  "id": 1,
  "username": "john_cinema",
  "email": "john@example.com",
  "bio": "Movie enthusiast",
  "avatarUrl": "https://...",
  "followerCount": 42,
  "followingCount": 30,
  "reviewCount": 15,
  "createdAt": "2026-01-15T10:30:00Z"
}

Status: 200 OK | 404 Not Found
```

### PUT /api/users/{id}
**Update user profile** (Auth Required)
```
Request:
{
  "bio": "Updated bio",
  "avatarUrl": "https://new-avatar-url"
}

Response: 200 OK
{
  "id": 1,
  "username": "john_cinema",
  "bio": "Updated bio",
  "updatedAt": "2026-03-29T11:00:00Z"
}

Status: 200 OK | 401 Unauthorized | 403 Forbidden | 404 Not Found
```

### GET /api/users/{id}/reviews
**Get user's reviews**
```
Query Parameters:
- limit: int (default: 10, max: 100)
- offset: int (default: 0)
- sort: string (default: createdAt:desc)

Response: 200 OK
{
  "total": 15,
  "count": 10,
  "data": [
    {
      "id": 1,
      "movieId": 550,
      "title": "Amazing!",
      "rating": 9.5,
      "createdAt": "2026-03-28T10:30:00Z"
    }
  ]
}

Status: 200 OK | 404 Not Found
```

### GET /api/users/{id}/followers
**Get user's followers**
```
Query Parameters:
- limit: int (default: 20, max: 100)
- offset: int (default: 0)

Response: 200 OK
{
  "total": 43,
  "count": 20,
  "data": [
    {
      "id": 2,
      "username": "jane_movie",
      "avatarUrl": "https://..."
    }
  ]
}

Status: 200 OK | 404 Not Found
```

### GET /api/users/{id}/following
**Get users this user is following**
```
Query Parameters:
- limit: int (default: 20, max: 100)
- offset: int (default: 0)

Response: 200 OK
{
  "total": 30,
  "count": 20,
  "data": [...]
}

Status: 200 OK | 404 Not Found
```

---

## Movies

### GET /api/movies
**Get movies list**
```
Query Parameters:
- limit: int (default: 20, max: 100)
- offset: int (default: 0)
- sort: string (default: createdAt:desc)
  Options: ratingAvg:desc, reviewCount:desc, releaseDate:desc
- genre: string (filter)
- year: int (filter)
- language: string (filter)
- keyword: string (search by title/director)

Response: 200 OK
{
  "total": 1250,
  "count": 20,
  "data": [
    {
      "id": 550,
      "title": "The Shawshank Redemption",
      "posterUrl": "https://...",
      "ratingAvg": 9.3,
      "reviewCount": 456,
      "releaseDate": "1994-10-14",
      "duration": 142,
      "language": "en"
    }
  ]
}

Status: 200 OK
```

### GET /api/movies/{id}
**Get movie details**
```
Response: 200 OK
{
  "id": 550,
  "tmdbId": 278,
  "title": "The Shawshank Redemption",
  "localTitle": "Nhà Tù Shawshank",
  "description": "Two imprisoned men...",
  "posterUrl": "https://...",
  "backdropUrl": "https://...",
  "trailerUrl": "https://...",
  "duration": 142,
  "releaseDate": "1994-10-14",
  "language": "en",
  "director": "Frank Darabont",
  "cast": "Tim Robbins, Morgan Freeman",
  "genresJson": "[\"Drama\", \"Crime\"]",
  "ratingAvg": 9.3,
  "reviewCount": 456,
  "createdAt": "2026-01-20T10:30:00Z"
}

Status: 200 OK | 404 Not Found
```

### GET /api/movies/search
**Search movies**
```
Query Parameters:
- keyword: string (required)
- limit: int (default: 10, max: 50)

Response: 200 OK
{
  "data": [
    {
      "id": 27205,
      "title": "Inception",
      "posterUrl": "https://...",
      "ratingAvg": 8.8,
      "reviewCount": 892
    }
  ]
}

Status: 200 OK
```

### GET /api/movies/{movieId}/reviews
**Get reviews for a movie**
```
Query Parameters:
- limit: int (default: 10, max: 100)
- offset: int (default: 0)
- sort: string (default: createdAt:desc)

Response: 200 OK
{
  "total": 456,
  "count": 10,
  "data": [
    {
      "id": 1,
      "userId": 1,
      "title": "Amazing!",
      "rating": 9.5,
      "likeCount": 12,
      "commentCount": 3,
      "createdAt": "2026-03-29T11:00:00Z",
      "user": {
        "id": 1,
        "username": "john_cinema",
        "avatarUrl": "https://..."
      }
    }
  ]
}

Status: 200 OK | 404 Not Found
```

---

## Reviews

### POST /api/reviews
**Create review** (Auth Required)
```
Request:
{
  "movieId": 550,
  "title": "Amazing masterpiece!",
  "content": "This movie is incredible. The cinematography...",
  "rating": 9.5,
  "hasSpoiler": false
}

Response: 201 Created
{
  "id": 1,
  "userId": 1,
  "movieId": 550,
  "title": "Amazing masterpiece!",
  "rating": 9.5,
  "likeCount": 0,
  "commentCount": 0,
  "createdAt": "2026-03-29T11:00:00Z",
  "user": {
    "id": 1,
    "username": "john_cinema"
  }
}

Status: 201 Created | 400 Bad Request | 401 Unauthorized
```

### PUT /api/reviews/{id}
**Update review** (Auth Required)
```
Request:
{
  "title": "Updated title",
  "content": "Updated content",
  "rating": 9.0,
  "hasSpoiler": true
}

Response: 200 OK
{
  "id": 1,
  "userId": 1,
  "movieId": 550,
  "isEdited": true,
  "editedAt": "2026-03-29T12:00:00Z",
  ...
}

Status: 200 OK | 401 Unauthorized | 403 Forbidden | 404 Not Found
```

### DELETE /api/reviews/{id}
**Delete review** (Auth Required)
```
Response: 204 No Content

Status: 204 No Content | 401 Unauthorized | 403 Forbidden | 404 Not Found
```

### GET /api/reviews/{id}
**Get review details**
```
Response: 200 OK
{
  "id": 1,
  "userId": 1,
  "movieId": 550,
  "title": "Amazing!",
  "content": "This movie is incredible...",
  "rating": 9.5,
  "hasSpoiler": false,
  "isEdited": false,
  "likeCount": 12,
  "commentCount": 3,
  "createdAt": "2026-03-29T11:00:00Z",
  "user": {
    "id": 1,
    "username": "john_cinema",
    "avatarUrl": "https://..."
  },
  "movie": {
    "id": 550,
    "title": "The Shawshank Redemption"
  }
}

Status: 200 OK | 404 Not Found
```

---

## Comments

### POST /api/reviews/{reviewId}/comments
**Create comment** (Auth Required)
```
Request:
{
  "content": "I totally agree with this review!"
}

Response: 201 Created
{
  "id": 1,
  "userId": 1,
  "reviewId": 1,
  "content": "I totally agree with this review!",
  "createdAt": "2026-03-29T11:10:00Z",
  "user": {
    "id": 1,
    "username": "john_cinema",
    "avatarUrl": "https://..."
  }
}

Status: 201 Created | 400 Bad Request | 401 Unauthorized | 404 Not Found
```

### DELETE /api/reviews/{reviewId}/comments/{commentId}
**Delete comment** (Auth Required)
```
Response: 204 No Content

Status: 204 No Content | 401 Unauthorized | 403 Forbidden | 404 Not Found
```

### GET /api/reviews/{reviewId}/comments
**Get comments for a review**
```
Query Parameters:
- limit: int (default: 10, max: 100)
- offset: int (default: 0)
- sort: string (default: createdAt:desc)

Response: 200 OK
{
  "total": 3,
  "count": 3,
  "data": [
    {
      "id": 1,
      "userId": 1,
      "content": "I totally agree!",
      "createdAt": "2026-03-29T11:10:00Z",
      "user": {
        "id": 1,
        "username": "john_cinema"
      }
    }
  ]
}

Status: 200 OK | 404 Not Found
```

---

## Likes

### POST /api/reviews/{reviewId}/like
**Toggle like/unlike** (Auth Required)
```
Request: {} (empty body)

Response: 200 OK
{
  "liked": true,
  "likeCount": 13
}

Status: 200 OK | 401 Unauthorized | 404 Not Found
```

### GET /api/reviews/{reviewId}/likes
**Get likes for a review**
```
Query Parameters:
- limit: int (default: 20, max: 100)
- offset: int (default: 0)

Response: 200 OK
{
  "total": 13,
  "count": 20,
  "data": [
    {
      "id": 1,
      "userId": 1,
      "username": "john_cinema",
      "avatarUrl": "https://...",
      "createdAt": "2026-03-29T11:05:00Z"
    }
  ]
}

Status: 200 OK | 404 Not Found
```

---

## Follows

### POST /api/users/{userId}/follow
**Toggle follow/unfollow** (Auth Required)
```
Request: {} (empty body)

Response: 200 OK
{
  "followed": true,
  "followerCount": 43
}

Status: 200 OK | 401 Unauthorized | 404 Not Found
```

### GET /api/users/{userId}/followers
**Get followers of a user**
```
(See Users section)
```

### GET /api/users/{userId}/following
**Get users this user is following**
```
(See Users section)
```

---

## Search & Discovery (Future)

### POST /api/search/semantic
**Semantic search using AI** (Auth Required)
```
Request:
{
  "query": "Phim về tình yêu buồn",
  "limit": 10,
  "filters": {
    "minRating": 7.0,
    "language": "vi"
  }
}

Response: 200 OK
{
  "data": [
    {
      "id": 550,
      "title": "The Shawshank Redemption",
      "similarity": 0.92,
      "topReviews": [...]
    }
  ]
}

Status: 200 OK | 401 Unauthorized
```

### GET /api/movies/trending
**Get trending movies** (Future)
```
Query Parameters:
- timeRange: string (default: week) [day, week, month, year]
- limit: int (default: 10, max: 50)

Response: 200 OK
{
  "data": [
    {
      "id": 550,
      "title": "The Shawshank Redemption",
      "ratingAvg": 9.3,
      "reviewCount": 456,
      "trend": "↑ 45 new reviews this week"
    }
  ]
}

Status: 200 OK
```

---

## Error Codes

### Common HTTP Status Codes

| Code | Meaning | Example |
|------|---------|---------|
| **200** | OK | Successful GET/PUT/DELETE |
| **201** | Created | New resource created (POST) |
| **204** | No Content | Successful DELETE |
| **400** | Bad Request | Validation failed |
| **401** | Unauthorized | Missing/invalid JWT token |
| **403** | Forbidden | No permission to resource |
| **404** | Not Found | Resource doesn't exist |
| **409** | Conflict | Duplicate (email, username, review) |
| **413** | Payload Too Large | Request body too big |
| **500** | Server Error | Unexpected server error |

### Error Response Format

```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable message",
    "details": [
      {
        "field": "fieldName",
        "message": "Field specific error"
      }
    ]
  },
  "timestamp": "2026-03-29T11:00:00Z"
}
```

### Common Error Codes

| Code | Meaning |
|------|---------|
| **AUTH_REQUIRED** | Missing or invalid JWT token |
| **INVALID_CREDENTIALS** | Wrong email/password |
| **USER_EXISTS** | Email or username already registered |
| **USER_NOT_FOUND** | User doesn't exist |
| **REVIEW_NOT_FOUND** | Review doesn't exist |
| **REVIEW_ALREADY_EXISTS** | User already reviewed this movie |
| **MOVIE_NOT_FOUND** | Movie doesn't exist |
| **UNAUTHORIZED_ACTION** | User doesn't have permission |
| **VALIDATION_ERROR** | Invalid input data |
| **EXTERNAL_API_ERROR** | TMDB/OpenAI API call failed |
| **RATE_LIMIT_EXCEEDED** | Too many requests |

---

## Authentication & Headers

### All Authenticated Endpoints Require

```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

### JWT Token Example
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6ImpvaG5fY2luZW1hIiwiaWF0IjoxNjc0OTk2MzAwfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

Token includes:
- User ID
- Username
- Expiration (24 hours default)

---

## Pagination Details

All list endpoints follow this pattern:

```json
{
  "total": 100,           // Total count in database
  "count": 10,            // Count in current response
  "limit": 10,            // Items per page
  "offset": 0,            // Starting position
  "hasMore": true,        // Whether more items exist
  "data": [...]           // Array of items
}
```

---

## Sorting

Default sort: `createdAt:desc`

Format: `field:direction`

**Direction**: `asc` or `desc`

**Common fields**:
- `createdAt`
- `rating`
- `likeCount`
- `ratingAvg` (movies)
- `reviewCount` (movies)

Example: `GET /api/reviews?sort=rating:desc&limit=10`

---

## Rate Limiting (Future)

Future implementation will include:
- 10 requests per second per user
- 1000 requests per day per user
- 100 reviews per 24 hours per user

Headers in response:
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1706302800
```

---

**API Reference v1.0**  
Last Updated: 2026-03-29
