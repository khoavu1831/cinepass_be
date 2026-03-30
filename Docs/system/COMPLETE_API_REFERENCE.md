# CinePass API Reference - Complete Documentation

## Base URL
```
https://localhost:5001/api
```

---

## 🔐 Authentication Endpoints

### POST /auth/register
**Register new user**
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_cinema",
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```
**Response: 201 Created**
```json
{
  "userId": 1,
  "username": "john_cinema",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "accessTokenExpiry": "2026-03-30T11:15:00Z"
}
```
**Status Codes:**
- 201 Created - User registered successfully
- 400 Bad Request - Invalid input
- 409 Conflict - Username or email already exists

---

### POST /auth/login
**Login existing user**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```
**Response: 200 OK**
```json
{
  "userId": 1,
  "username": "john_cinema",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "accessTokenExpiry": "2026-03-30T11:15:00Z"
}
```
**Status Codes:**
- 200 OK - Login successful
- 401 Unauthorized - Invalid credentials
- 404 Not Found - User not found

---

### POST /auth/refresh
**Refresh access token**
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```
**Response: 200 OK**
```json
{
  "userId": 1,
  "username": "john_cinema",
  "accessToken": "new_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "accessTokenExpiry": "2026-03-30T11:15:00Z"
}
```
**Status Codes:**
- 200 OK - Token refreshed
- 401 Unauthorized - Invalid or expired refresh token
- 400 Bad Request - Refresh token revoked

---

### POST /auth/logout
**Logout user** *(Auth Required)*
```http
POST /api/auth/logout
Authorization: Bearer <access_token>
```
**Response: 200 OK**
```json
{
  "message": "Logout successful"
}
```
**Status Codes:**
- 200 OK - Logout successful
- 401 Unauthorized - Invalid token

---

## 👤 User Endpoints

### GET /users/{id}
**Get user profile**
```http
GET /api/users/1
```
**Response: 200 OK**
```json
{
  "id": 1,
  "username": "john_cinema",
  "email": "john@example.com",
  "bio": "Movie enthusiast & critic",
  "avatarUrl": "https://cdn.example.com/avatar/1.jpg",
  "role": "USER",
  "isActive": true,
  "followerCount": 42,
  "followingCount": 30,
  "reviewCount": 15,
  "createdAt": "2026-01-15T10:30:00Z",
  "updatedAt": "2026-03-29T11:00:00Z"
}
```
**Status Codes:**
- 200 OK - Success
- 404 Not Found - User not found

---

### PUT /users/{id}
**Update user profile** *(Auth Required)*
```http
PUT /api/users/1
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "bio": "Updated bio - Movie critic",
  "avatarUrl": "https://cdn.example.com/avatar/new.jpg"
}
```
**Response: 200 OK**
```json
{
  "id": 1,
  "username": "john_cinema",
  "bio": "Updated bio - Movie critic",
  "avatarUrl": "https://cdn.example.com/avatar/new.jpg",
  "updatedAt": "2026-03-30T14:45:00Z"
}
```
**Status Codes:**
- 200 OK - Updated successfully
- 401 Unauthorized - Not authenticated
- 403 Forbidden - Cannot update other user's profile
- 404 Not Found - User not found

---

### DELETE /users/{id}
**Delete user account** *(Auth Required, Admin)*
```http
DELETE /api/users/1
Authorization: Bearer <access_token>
```
**Response: 204 No Content**

**Status Codes:**
- 204 No Content - Deleted successfully
- 401 Unauthorized - Not authenticated
- 403 Forbidden - Insufficient permissions
- 404 Not Found - User not found

---

## 🎬 Movie Endpoints

### GET /movies
**List all movies with pagination**
```http
GET /api/movies?page=1&pageSize=20&sortBy=releaseDate
```
**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "tmdbId": 550,
      "title": "Fight Club",
      "localTitle": "Câu Lạc Bộ Chiến Đấu",
      "description": "An insomniac office worker and a devil-may-care soapmaker form an underground fight club...",
      "posterUrl": "https://image.tmdb.org/t/p/w500/pB8BM7pdSp6B6Ric7caatvIVfjV.jpg",
      "backdropUrl": "https://image.tmdb.org/t/p/w1280/...",
      "duration": 139,
      "releaseDate": "1999-10-15",
      "director": "David Fincher",
      "genres": ["Action", "Drama"],
      "ratingAvg": 8.5,
      "reviewCount": 124,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ],
  "total": 150,
  "page": 1,
  "pageSize": 20
}
```
**Query Parameters:**
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 20) - Items per page
- `sortBy` (string, default: "releaseDate") - Sort field
- `order` (string, default: "desc") - asc | desc
- `search` (string, optional) - Search by title

**Status Codes:**
- 200 OK - Success

---

### GET /movies/{id}
**Get movie details**
```http
GET /api/movies/1
```
**Response: 200 OK** (Same structure as list)

**Status Codes:**
- 200 OK - Success
- 404 Not Found - Movie not found

---

### GET /movies/{id}/reviews
**Get all reviews for a movie**
```http
GET /api/movies/1/reviews?page=1&pageSize=10
```
**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "userId": 2,
      "movieId": 1,
      "title": "Masterpiece!",
      "content": "One of the best films ever made...",
      "rating": 9.5,
      "hasSpoiler": false,
      "likeCount": 45,
      "commentCount": 8,
      "author": {
        "id": 2,
        "username": "critic_jane",
        "avatarUrl": "https://..."
      },
      "createdAt": "2026-03-20T15:30:00Z"
    }
  ],
  "total": 50,
  "page": 1,
  "pageSize": 10
}
```

---

## ⭐ Review Endpoints

### POST /reviews
**Create review** *(Auth Required)*
```http
POST /api/reviews
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "movieId": 1,
  "title": "Amazing film",
  "content": "This movie is absolutely incredible...",
  "rating": 9.0,
  "hasSpoiler": false
}
```
**Response: 201 Created**
```json
{
  "id": 1,
  "userId": 2,
  "movieId": 1,
  "title": "Amazing film",
  "content": "This movie is absolutely incredible...",
  "rating": 9.0,
  "hasSpoiler": false,
  "likeCount": 0,
  "commentCount": 0,
  "createdAt": "2026-03-30T14:00:00Z"
}
```
**Status Codes:**
- 201 Created - Review created
- 400 Bad Request - Invalid input
- 401 Unauthorized - Not authenticated
- 409 Conflict - User already reviewed this movie

---

### GET /reviews/{id}
**Get review details**
```http
GET /api/reviews/1
```
**Response: 200 OK** (Same structure as create)

**Status Codes:**
- 200 OK - Success
- 404 Not Found - Review not found

---

### PUT /reviews/{id}
**Update review** *(Auth Required, Owner Only)*
```http
PUT /api/reviews/1
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "title": "Updated title",
  "content": "Updated content...",
  "rating": 8.5
}
```
**Response: 200 OK** (Updated review)

**Status Codes:**
- 200 OK - Updated
- 401 Unauthorized - Not authenticated
- 403 Forbidden - Cannot edit other user's review
- 404 Not Found - Review not found

---

### DELETE /reviews/{id}
**Delete review** *(Auth Required, Owner/Admin)*
```http
DELETE /api/reviews/1
Authorization: Bearer <access_token>
```
**Response: 204 No Content**

---

### POST /reviews/{id}/like
**Like a review** *(Auth Required)*
```http
POST /api/reviews/1/like
Authorization: Bearer <access_token>
```
**Response: 200 OK**
```json
{
  "message": "Review liked",
  "likeCount": 46
}
```
**Status Codes:**
- 200 OK - Liked
- 400 Bad Request - Already liked
- 401 Unauthorized - Not authenticated

---

### DELETE /reviews/{id}/like
**Unlike a review** *(Auth Required)*
```http
DELETE /api/reviews/1/like
Authorization: Bearer <access_token>
```
**Response: 200 OK**
```json
{
  "message": "Like removed",
  "likeCount": 45
}
```

---

## 💬 Comment Endpoints

### POST /reviews/{reviewId}/comments
**Add comment** *(Auth Required)*
```http
POST /api/reviews/1/comments
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "content": "Great review! I totally agree."
}
```
**Response: 201 Created**
```json
{
  "id": 1,
  "reviewId": 1,
  "userId": 3,
  "content": "Great review! I totally agree.",
  "author": {
    "id": 3,
    "username": "user_bob",
    "avatarUrl": "https://..."
  },
  "createdAt": "2026-03-30T15:20:00Z"
}
```

---

### GET /reviews/{reviewId}/comments
**Get all comments for review**
```http
GET /api/reviews/1/comments?page=1&pageSize=20
```
**Response: 200 OK** (List of comments)

---

### PUT /comments/{id}
**Update comment** *(Auth Required, Owner Only)*
```http
PUT /api/comments/1
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "content": "Updated comment..."
}
```
**Response: 200 OK** (Updated comment)

---

### DELETE /comments/{id}
**Delete comment** *(Auth Required, Owner/Admin)*
```http
DELETE /api/comments/1
Authorization: Bearer <access_token>
```
**Response: 204 No Content**

---

## 👥 Follow Endpoints

### POST /users/{userId}/follow
**Follow user** *(Auth Required)*
```http
POST /api/users/1/follow
Authorization: Bearer <access_token>
```
**Response: 200 OK**
```json
{
  "message": "User followed",
  "followingCount": 31
}
```
**Status Codes:**
- 200 OK - Followed
- 400 Bad Request - Already following or cannot follow self
- 401 Unauthorized - Not authenticated

---

### DELETE /users/{userId}/follow
**Unfollow user** *(Auth Required)*
```http
DELETE /api/users/1/follow
Authorization: Bearer <access_token>
```
**Response: 200 OK**
```json
{
  "message": "User unfollowed",
  "followingCount": 30
}
```

---

### GET /users/{id}/followers
**Get user followers**
```http
GET /api/users/1/followers?page=1&pageSize=20
```
**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 2,
      "username": "follower_1",
      "avatarUrl": "https://..."
    }
  ],
  "total": 42,
  "page": 1,
  "pageSize": 20
}
```

---

### GET /users/{id}/following
**Get users being followed**
```http
GET /api/users/1/following?page=1&pageSize=20
```
**Response: 200 OK** (Same structure)

---

## 🔍 Search Endpoints

### POST /search/semantic
**Semantic search reviews** *(Future: AI-powered)*
```http
POST /api/search/semantic
Content-Type: application/json

{
  "query": "best sci-fi movies",
  "page": 1,
  "pageSize": 20
}
```
**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "movieTitle": "Inception",
      "reviewTitle": "Mind-bending masterpiece",
      "similarity": 0.92
    }
  ],
  "total": 15,
  "page": 1
}
```

---

## ⚠️ Error Response Format

All error responses follow this standard format:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input provided",
    "details": [
      {
        "field": "email",
        "message": "Email format is invalid"
      }
    ]
  }
}
```

**Common Error Codes:**
- `VALIDATION_ERROR` - Input validation failed (400)
- `UNAUTHORIZED` - Missing or invalid authentication (401)
- `FORBIDDEN` - Insufficient permissions (403)
- `NOT_FOUND` - Resource not found (404)
- `CONFLICT` - Resource already exists (409)
- `INTERNAL_ERROR` - Server error (500)

---

## 📝 Pagination

All list endpoints support pagination with these parameters:
- `page` (int, default: 1) - Page number (1-indexed)
- `pageSize` (int, default: 20, max: 100) - Items per page

Response structure:
```json
{
  "data": [],
  "total": 150,
  "page": 1,
  "pageSize": 20
}
```

---

## 🔑 Authentication Headers

Add Authorization header to protected endpoints:
```http
Authorization: Bearer <access_token>
```

---

## 📊 Rate Limiting

- **Default:** 100 requests per 15 minutes per IP/user
- **Premium:** 1000 requests per 15 minutes

Response headers:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 85
X-RateLimit-Reset: 1648560000
```
