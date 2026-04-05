# CinePass API Reference

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
  "email": "john@example.com",
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
  "email": "john@example.com",
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
  "email": "john@example.com",
  "accessToken": "new_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "accessTokenExpiry": "2026-03-30T11:15:00Z"
}
```
**Status Codes:**
- 200 OK - Token refreshed
- 401 Unauthorized - Invalid or expired refresh token

---

### POST /auth/logout
**Logout user** *(Auth Required)*
```http
POST /api/auth/logout
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "refreshToken": "token_here"
}
```
**Response: 200 OK**
```json
{
  "message": "Đăng xuất thành công"
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

## 🎬 Movie Endpoints

### GET /movies
**List all movies with pagination, sorting, and search**
```http
GET /api/movies?page=1&pageSize=20&sortBy=releaseDate&order=desc&search=fight
```
**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "tmdbId": 550,
      "title": "Fight Club",
      "localTitle": "Câu lạc bộ chiến đấu",
      "description": "An insomniac office worker and a devil-may-care soapmaker form an underground fight club...",
      "posterUrl": "https://image.tmdb.org/t/p/w500/pB8BM7pdSp6B6Ric7caatvIVfjV.jpg",
      "backdropUrl": "https://image.tmdb.org/t/p/w1280/...",
      "trailerUrl": "https://youtube.com/...",
      "duration": 139,
      "releaseDate": "1999-10-15",
      "director": "David Fincher",
      "cast": "Brad Pitt, Edward Norton, Helena Bonham Carter",
      "genres": ["Action", "Drama", "Thriller"],
      "ratingAvg": 8.5,
      "reviewCount": 124,
      "createdAt": "2026-01-01T00:00:00Z",
      "updatedAt": "2026-03-15T10:00:00Z"
    }
  ],
  "total": 150,
  "page": 1,
  "pageSize": 20
}
```
**Query Parameters:**
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 20, max: 100) - Items per page
- `sortBy` (string, default: "releaseDate") - Sort field: `releaseDate`, `ratingAvg`, `title`
- `order` (string, default: "desc") - Sort order: `asc` | `desc`
- `search` (string, optional) - Search by title or localTitle

**Status Codes:**
- 200 OK - Success

---

### GET /movies/{id}
**Get movie details**
```http
GET /api/movies/1
```
**Response: 200 OK**
```json
{
  "id": 1,
  "tmdbId": 550,
  "title": "Fight Club",
  "localTitle": "Câu lạc bộ chiến đấu",
  "description": "An insomniac office worker and a devil-may-care soapmaker form an underground fight club...",
  "posterUrl": "https://image.tmdb.org/t/p/w500/pB8BM7pdSp6B6Ric7caatvIVfjV.jpg",
  "backdropUrl": "https://image.tmdb.org/t/p/w1280/...",
  "trailerUrl": "https://youtube.com/...",
  "duration": 139,
  "releaseDate": "1999-10-15",
  "director": "David Fincher",
  "cast": "Brad Pitt, Edward Norton, Helena Bonham Carter",
  "genres": ["Action", "Drama", "Thriller"],
  "ratingAvg": 8.5,
  "reviewCount": 124,
  "createdAt": "2026-01-01T00:00:00Z",
  "updatedAt": "2026-03-15T10:00:00Z"
}
```

**Status Codes:**
- 200 OK - Success
- 404 Not Found - Movie not found

---

### POST /movies/fetch-from-tmdb/{tmdbId}
**Fetch and save movie from TMDB by ID**
```http
POST /api/movies/fetch-from-tmdb/550
```
**Response: 200 OK**
```json
{
  "id": 1,
  "tmdbId": 550,
  "title": "Fight Club",
  "localTitle": null,
  "description": "An insomniac office worker and a devil-may-care soapmaker form an underground fight club...",
  "posterUrl": "https://image.tmdb.org/t/p/w500/pB8BM7pdSp6B6Ric7caatvIVfjV.jpg",
  "backdropUrl": "https://image.tmdb.org/t/p/w1280/...",
  "trailerUrl": "https://www.youtube.com/watch?v=...",
  "duration": 139,
  "releaseDate": "1999-10-15",
  "director": "David Fincher",
  "cast": "Brad Pitt, Edward Norton, Helena Bonham Carter, Jared Leto",
  "genres": ["Drama", "Thriller"],
  "ratingAvg": 0.0,
  "reviewCount": 0,
  "createdAt": "2026-04-05T12:00:00Z",
  "updatedAt": "2026-04-05T12:00:00Z"
}
```

**Status Codes:**
- 200 OK - Movie fetched and saved successfully
- 400 Bad Request - Could not fetch from TMDB or TMDB ID invalid
- 404 Not Found - Movie not found on TMDB

---

### POST /movies/search-tmdb
**Search TMDB and import top results** *(Imports up to 10 movies)*
```http
POST /api/movies/search-tmdb
Content-Type: application/json

{
  "query": "Fight Club",
  "page": 1
}
```
**Response: 200 OK**
```json
{
  "data": [
    {
      "id": 1,
      "tmdbId": 550,
      "title": "Fight Club",
      "localTitle": null,
      "description": "An insomniac office worker and a devil-may-care soapmaker form an underground fight club...",
      "posterUrl": "https://image.tmdb.org/t/p/w500/pB8BM7pdSp6B6Ric7caatvIVfjV.jpg",
      "backdropUrl": "https://image.tmdb.org/t/p/w1280/...",
      "trailerUrl": "https://www.youtube.com/watch?v=...",
      "duration": 139,
      "releaseDate": "1999-10-15",
      "director": "David Fincher",
      "cast": "Brad Pitt, Edward Norton, Helena Bonham Carter, Jared Leto",
      "genres": ["Drama", "Thriller"],
      "ratingAvg": 0.0,
      "reviewCount": 0,
      "createdAt": "2026-04-05T12:00:00Z",
      "updatedAt": "2026-04-05T12:00:00Z"
    }
  ],
  "total": 1
}
```

**Query Parameters:**
- `query` (string, required) - Search term
- `page` (int, default: 1) - Page number for search results

**Status Codes:**
- 200 OK - Search completed successfully
- 400 Bad Request - Query is empty or error occurred

---

## 🔐 Admin Movie Management Endpoints

> **⚠️ Auth Required, Admin Only** - All endpoints require valid JWT token with Admin role

### POST /movies/admin/fetch-list
**Admin: Fetch and save list of movies from TMDB** *(Admin Required)*

Supports importing multiple movies at once based on category:
```http
POST /api/movies/admin/fetch-list
Authorization: Bearer <admin_token>
Content-Type: application/json

{
  "type": "popular",
  "page": 1,
  "pageSize": 20,
  "region": "US"
}
```

**Available Types:**
- `popular` - Popular movies in region
- `top_rated` - Top rated movies globally
- `upcoming` - Upcoming movies in region
- `now_playing` - Currently playing movies in region
- `genre` - Movies by genre (set `genreId` field)

**Request Fields:**
```json
{
  "type": "popular|top_rated|upcoming|now_playing|genre",
  "page": 1,
  "pageSize": 20,
  "region": "US|GB|VN|etc",
  "genreId": 28,          // Only for type="genre"
  "sortBy": "popularity.desc"  // For discover/genre: popularity.desc, rating.desc, etc
}
```

**Response: 200 OK**
```json
{
  "successCount": 20,
  "failureCount": 0,
  "errors": [],
  "savedMovies": [
    {
      "id": 1,
      "tmdbId": 550,
      "title": "Fight Club",
      "description": "...",
      "posterUrl": "https://...",
      "genres": ["Drama", "Thriller"],
      "ratingAvg": 0.0,
      "reviewCount": 0,
      "createdAt": "2026-04-05T12:00:00Z"
    }
  ]
}
```

**Status Codes:**
- 200 OK - Import completed (check successCount/failureCount)
- 400 Bad Request - Invalid type or error occurred
- 401 Unauthorized - Missing or invalid token
- 403 Forbidden - User is not admin

**Examples:**

```bash
# Fetch popular movies
curl -X POST http://localhost:5001/api/movies/admin/fetch-list \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "popular",
    "page": 1,
    "pageSize": 20,
    "region": "US"
  }'

# Fetch top rated movies
curl -X POST http://localhost:5001/api/movies/admin/fetch-list \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "top_rated",
    "page": 1,
    "pageSize": 15
  }'

# Fetch action movies (genre ID 28)
curl -X POST http://localhost:5001/api/movies/admin/fetch-list \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "genre",
    "genreId": 28,
    "page": 1,
    "pageSize": 20,
    "sortBy": "popularity.desc"
  }'

# Fetch upcoming movies in Vietnam
curl -X POST http://localhost:5001/api/movies/admin/fetch-list \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "upcoming",
    "page": 1,
    "pageSize": 20,
    "region": "VN"
  }'
```

---

### PUT /movies/admin/{id}
**Admin: Update movie information** *(Admin Required)*

```http
PUT /api/movies/admin/1
Authorization: Bearer <admin_token>
Content-Type: application/json

{
  "localTitle": "Câu Lạc Bộ Chiến Đấu",
  "description": "Optional updated description",
  "director": "David Fincher",
  "cast": "Brad Pitt, Edward Norton, Helena Bonham Carter"
}
```

**Response: 200 OK**
```json
{
  "id": 1,
  "tmdbId": 550,
  "title": "Fight Club",
  "localTitle": "Câu Lạc Bộ Chiến Đấu",
  "description": "...",
  "director": "David Fincher",
  "cast": "Brad Pitt, Edward Norton, Helena Bonham Carter",
  "genres": ["Drama", "Thriller"],
  "ratingAvg": 8.5,
  "reviewCount": 124,
  "updatedAt": "2026-04-05T15:30:00Z"
}
```

**Status Codes:**
- 200 OK - Updated successfully
- 400 Bad Request - Invalid input
- 401 Unauthorized - Not authenticated or invalid token
- 403 Forbidden - User is not admin
- 404 Not Found - Movie not found

---

### DELETE /movies/admin/{id}
**Admin: Delete movie from database** *(Admin Required)*

```http
DELETE /api/movies/admin/1
Authorization: Bearer <admin_token>
```

**Response: 200 OK**
```json
{
  "message": "Phim đã được xoá thành công"
}
```

**Status Codes:**
- 200 OK - Deleted successfully
- 401 Unauthorized - Not authenticated
- 403 Forbidden - User is not admin
- 404 Not Found - Movie not found

> **⚠️ Note:** Deleting a movie will cascade delete all associated reviews and comments

---

### Supported TMDB Genres

| ID | Genre | ID | Genre |
|----|-------|----|----|
| 28 | Action | 35 | Comedy |
| 12 | Adventure | 80 | Crime |
| 16 | Animation | 99 | Documentary |
| 37 | Western | 14 | Fantasy |
| 27 | Horror | 36 | History |
| 10749 | Romance | 878 | Science Fiction |
| 10770 | TV Movie | 53 | Thriller |
| 9648 | Mystery | 18 | Drama |

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
- 201 Created - Review created successfully
- 400 Bad Request - Invalid input
- 401 Unauthorized - Not authenticated
- 409 Conflict - User already reviewed this movie

---

### GET /reviews/{id}
**Get review details**
```http
GET /api/reviews/1
```
**Response: 200 OK**
```json
{
  "id": 1,
  "userId": 2,
  "movieId": 1,
  "title": "Amazing film",
  "content": "This movie is absolutely incredible...",
  "rating": 9.0,
  "hasSpoiler": false,
  "likeCount": 45,
  "commentCount": 8,
  "author": {
    "id": 2,
    "username": "critic_jane",
    "avatarUrl": "https://..."
  },
  "createdAt": "2026-03-30T14:00:00Z",
  "updatedAt": "2026-03-30T15:00:00Z"
}
```

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
  "rating": 8.5,
  "hasSpoiler": false
}
```
**Response: 200 OK**
```json
{
  "id": 1,
  "userId": 2,
  "movieId": 1,
  "title": "Updated title",
  "content": "Updated content...",
  "rating": 8.5,
  "hasSpoiler": false,
  "updatedAt": "2026-03-30T16:00:00Z"
}
```

**Status Codes:**
- 200 OK - Updated successfully
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

**Status Codes:**
- 204 No Content - Deleted successfully
- 401 Unauthorized - Not authenticated
- 403 Forbidden - Cannot delete other user's review
- 404 Not Found - Review not found

---

### GET /movies/{movieId}/reviews
**Get all reviews for a movie with pagination**
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

**Status Codes:**
- 200 OK - Success
- 404 Not Found - Movie not found

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
- 200 OK - Liked successfully
- 400 Bad Request - Already liked this review
- 401 Unauthorized - Not authenticated
- 404 Not Found - Review not found

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

**Status Codes:**
- 200 OK - Removed successfully
- 401 Unauthorized - Not authenticated
- 404 Not Found - Review or like not found

---

## 🔑 Authentication

Add Bearer token to protected endpoints:
```http
Authorization: Bearer <access_token>
```

---

## 📝 Pagination

All paginated endpoints support:
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

## ⚠️ Error Response

Error responses follow this standard format:
```json
{
  "message": "Error description"
}
```

**Common Status Codes:**
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource already exists
- `500 Internal Server Error` - Server error
