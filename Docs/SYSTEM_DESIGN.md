# CinePass Backend - System Design & API Plan

## 📋 Mục lục
1. [Tổng Quan Dự Án](#tổng-quan-dự-án)
2. [Tóm Tắt Thực Thi](#tóm-tắt-thực-thi)
3. [Kiến Trúc Hệ Thống](#kiến-trúc-hệ-thống)
4. [Cơ Sở Dữ Liệu](#cơ-sở-dữ-liệu)
5. [Use Cases](#use-cases)
6. [Thiết Kế API](#thiết-kế-api)
7. [Luồng Dữ Liệu](#luồng-dữ-liệu)

---

## 🎬 Tổng Quan Dự Án

### Giới Thiệu
**CinePass** là một nền tảng mạng xã hội chuyên về review phim, cho phép người dùng:
- Xem danh sách phim (từ TMDB API)
- Viết, bình luận, thích reviews phim
- Theo dõi người dùng khác
- Khám phá phim thông qua semantic search dựa trên AI embeddings

### Stack Công Nghệ
- **Backend**: ASP.NET Core 10
- **Database**: MySQL 8.0+ (Vector Search từ v8.0.32+)
- **ORM**: Entity Framework Core
- **API Architecture**: RESTful API
- **Authentication**: JWT (dự kiến)
- **AI Services**: OpenAI API (text-embedding-3-small)
- **Vector Database (Optional)**: Qdrant/Weaviate/Pinecone (nếu cần advanced search)

### Core Entities (7 Bảng)
| Bảng | Mô Tả |
|------|-------|
| **Users** | Thông tin người dùng, profil, role (USER/MODERATOR/ADMIN) |
| **Movies** | Danh sách phim từ TMDB, metadata, rating trung bình |
| **Reviews** | Review của người dùng cho phim, đánh giá 1-10 |
| **Comments** | Bình luận trên reviews |
| **Likes** | Thích reviews (1 like/user/review) |
| **Follows** | Theo dõi người dùng (self-referencing) |
| **ReviewEmbeddings** | Vector embeddings của review & phim cho semantic search |

---

## 📊 Tóm Tắt Thực Thi

### Phase 1: MVP Core Features (Hiện tại)
- ✅ Database Schema & Models (đã hoàn thành)
- ⏳ **API Layer** (Tiếp theo)
- ⏳ Authentication & Authorization
- ⏳ Business Logic Layer

### Phase 2: Social Features
- Hệ thống notification
- User feed/timeline
- Trending movies

### Phase 3: AI Features
- Semantic search dựa trên embeddings
- Recommendation engine
- Review sentiment analysis

---

## 🏗️ Kiến Trúc Hệ Thống

```
┌─────────────────────────────────────────────────────────┐
│                    Frontend (React/Vue)                 │
└─────────────────┬───────────────────────────────────────┘
                  │ HTTP/REST
┌─────────────────▼───────────────────────────────────────┐
│             ASP.NET Core 10 API Server                  │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Controllers Layer (API Endpoints)               │  │
│  ├──────────────────────────────────────────────────┤  │
│  │  Service Layer (Business Logic)                  │  │
│  ├──────────────────────────────────────────────────┤  │
│  │  Repository Pattern / Unit of Work               │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────┬───────────────────────────────────────┘
                  │ Entity Framework Core
┌─────────────────▼───────────────────────────────────────┐
│             MySQL 8.0+ Database                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Users | Movies | Reviews | Comments              │  │
│  │ Likes | Follows | ReviewEmbeddings (JSON/BLOB)   │  │
│  └──────────────────────────────────────────────────┘  │
└──────────────────┬──────────────────────────────────────┘
                   │ External Services
      ┌────────────┼────────────────┐
      │            │                │
 ┌────▼─────┐ ┌──▼────────┐  ┌────▼─────┐
 │ TMDB API  │ │OpenAI API │  │Vector DB │
 │(Movies)   │ │(Embeddings)  │(Optional)│
 └───────────┘ └───────────┘  └──────────┘
```

---

## 💾 Cơ Sở Dữ Liệu

### Entity Relationship Diagram

```
User (1) ──────── *(n) Review
  │                     │
  │                     │ (1)
  │ (1)                 └─── (1) Movie
  │                          │
  │ (n)                       │ (1)
  └─────── *(n) Comment       └─── ReviewEmbedding
            │
            └─ (1) Review

User (1) ──────── *(n) Like ────── (1) Review
           
User (1) ──────── *(n) Follow ──── (1) User
         (Follower)     (Following)
```

### Chi Tiết Các Bảng

#### 1. **Users**
```
Id (int, PK, AI)
Username (string, UNIQUE, required)
Email (string, UNIQUE, required)
PasswordHash (string, required)
Bio (string, nullable, max 500)
AvatarUrl (string, nullable)
Role (string: USER | MODERATOR | ADMIN, default: USER)
IsActive (bool, default: true)
FollowerCount (int, denormalized, default: 0)
FollowingCount (int, denormalized, default: 0)
ReviewCount (int, denormalized, default: 0)
CreatedAt (datetime)
UpdatedAt (datetime, nullable)

Relationships:
- 1:n Reviews
- 1:n Comments
- 1:n Likes
- 1:n FollowersCollection (self-ref, incoming)
- 1:n FollowingCollection (self-ref, outgoing)
```

#### 2. **Movies**
```
Id (int, PK, AI)
TmdbId (int, UNIQUE, nullable) - Reference to TMDB API
Title (string, required)
LocalTitle (string, nullable)
Description (string, nullable) - For AI embeddings
PosterUrl (string, nullable)
BackdropUrl (string, nullable)
TrailerUrl (string, nullable)
Duration (int, nullable) - Minutes
ReleaseDate (date, nullable)
Language (string, nullable)
Director (string, nullable)
Cast (string, nullable) - JSON or CSV
GenresJson (string, nullable) - JSON array
RatingAvg (decimal 4,2, default: 0) - 0.00-99.99
ReviewCount (int, default: 0) - Denormalized count
CreatedAt (datetime)
UpdatedAt (datetime, nullable)

Indexes:
- TmdbId (UNIQUE)
- ReviewCount DESC
- RatingAvg DESC

Relationships:
- 1:n Reviews
- 1:n ReviewEmbeddings
```

#### 3. **Reviews**
```
Id (int, PK, AI)
UserId (int, FK) - User who reviewed
MovieId (int, FK) - Movie being reviewed
Title (string, required)
Content (string, required) - Long-form review
Rating (decimal 4,1) - 1-10 scale
HasSpoiler (bool, default: false)
IsEdited (bool, default: false)
EditedAt (datetime, nullable)
LikeCount (int, denormalized, default: 0)
CommentCount (int, denormalized, default: 0)
CreatedAt (datetime)
UpdatedAt (datetime, nullable)

Constraints:
- UNIQUE (UserId, MovieId) - One review per user per movie

Indexes:
- (UserId, MovieId) UNIQUE
- CreatedAt DESC
- Rating DESC
- LikeCount DESC

Relationships:
- n:1 User (Cascade Delete)
- n:1 Movie (Cascade Delete)
- 1:n Comments
- 1:n Likes
- 1:1 ReviewEmbedding
```

#### 4. **Comments**
```
Id (int, PK, AI)
UserId (int, FK)
ReviewId (int, FK)
Content (string, required)
CreatedAt (datetime)
UpdatedAt (datetime, nullable)

Indexes:
- CreatedAt DESC
- ReviewId
- UserId

Relationships:
- n:1 User (Cascade Delete)
- n:1 Review (Cascade Delete)
```

#### 5. **Likes**
```
Id (int, PK, AI)
UserId (int, FK)
ReviewId (int, FK)
CreatedAt (datetime)

Constraints:
- UNIQUE (UserId, ReviewId) - One like per user per review

Indexes:
- (UserId, ReviewId) UNIQUE

Relationships:
- n:1 User (Cascade Delete)
- n:1 Review (Cascade Delete)
```

#### 6. **Follows**
```
Id (int, PK, AI)
FollowerId (int, FK) - User who follows
FollowingId (int, FK) - User being followed
CreatedAt (datetime)

Constraints:
- UNIQUE (FollowerId, FollowingId)

Indexes:
- (FollowerId, FollowingId) UNIQUE
- FollowerId
- FollowingId

Relationships:
- n:1 User (Follower) - Restrict Delete
- n:1 User (Following) - Restrict Delete
```

#### 7. **ReviewEmbeddings** (Vector Storage)
```
Id (int, PK, AI)
ReviewId (int, FK, UNIQUE)
MovieId (int, FK)
MovieDescriptionVector (LONGBLOB) - Movie embedding (float32 binary)
ReviewContentVector (LONGBLOB) - Review embedding (float32 binary)
CombinedVector (LONGBLOB) - Weighted average (float32 binary)
EmbeddingModel (string, default: "text-embedding-3-small")
CreatedAt (datetime)
UpdatedAt (datetime, nullable)

Indexes:
- ReviewId UNIQUE
- MovieId
- CreatedAt DESC

Note: 
  - Vectors lưu dưới dạng binary BLOB (efficient storage)
  - Nếu dùng MySQL Vector Search (v8.0.32+): có thể lưu JSON
  - Nếu cần advanced search: dùng external vector DB (Qdrant/Weaviate)

Relationships:
- n:1 Review (Cascade Delete)
- n:1 Movie (Cascade Delete)
```

---

## 👥 Use Cases

### UC1: Quản Lý Tài Khoản Người Dùng
**Actors**: User
**Preconditions**: -
**Main Flow**:
1. Người dùng đăng ký với username, email, password
2. Hệ thống validate dữ liệu (unique email/username)
3. Hash password, lưu vào database
4. Return JWT token
5. Người dùng có thể đăng nhập, cập nhật profile (bio, avatar)

**Alternative Flows**:
- Email/username đã tồn tại → Error 400
- Invalid email format → Error 400
- Password quá yếu → Error 400

---

### UC2: Tìm Kiếm & Xem Danh Sách Phim
**Actors**: User (có thể anonymous)
**Main Flow**:
1. User yêu cầu danh sách phim (với filter/sort/pagination)
2. Hệ thống query Movies table
3. Return danh sách với thông tin: title, poster, ratingAvg, reviewCount
4. User chọn phim để xem chi tiết

**Queries hỗ trợ**:
- Sắp xếp theo: rating DESC, reviewCount DESC, releaseDate DESC
- Lọc theo: genre, language, year
- Tìm kiếm: keyword (title/director/actor)
- Pagination: limit, offset

---

### UC3: Viết & Quản Lý Review
**Actors**: Authenticated User
**Preconditions**: User đã đăng nhập, phim tồn tại
**Main Flow**:
1. User viết review cho phim:
   - Title, Content, Rating (1-10), HasSpoiler flag
2. Hệ thống validate:
   - User chưa review phim này
   - Rating 1-10, Content không để trống
3. Lưu Review, tạo ReviewEmbedding (call OpenAI API)
4. Update Movie.ReviewCount & RatingAvg (denormalized)
5. Update User.ReviewCount (denormalized)
6. Return review với ID

**Alternative Flows**:
- User đã review phim → Error 400 (hoặc cho phép update)
- Invalid rating → Error 400
- OpenAI API fail → Queue for later processing

---

### UC4: Tương Tác với Review (Like, Comment)
**Actors**: Authenticated User
**Preconditions**: Review tồn tại

#### UC4a: Like Review
**Main Flow**:
1. User like/unlike review
2. Hệ thsistem check: đã like chưa?
3. Nếu chưa → INSERT Like, increment Review.LikeCount
4. Nếu rồi → DELETE Like, decrement Review.LikeCount
5. Return success

#### UC4b: Comment on Review
**Main Flow**:
1. User tạo comment:
   - ReviewId, Content
2. Hệ thống validate:
   - Review tồn tại, Content không để trống
3. Lưu Comment
4. Update Review.CommentCount
5. Return comment

---

### UC5: Theo Dõi Người Dùng
**Actors**: Authenticated User
**Preconditions**: Target user tồn tại
**Main Flow**:
1. User follow/unfollow người dùng khác
2. Hệ thống:
   - Check: đã follow chưa?
   - Nếu chưa → INSERT Follow, increment Follower/Following counts
   - Nếu rồi → DELETE Follow, decrement counts
3. Return success

---

### UC6: Semantic Search (Future)
**Actors**: User (có thể anonymous)
**Main Flow**:
1. User nhập query: "Phim về tình yêu buồn"
2. Hệ thống:
   - Convert query thành embedding (OpenAI API)
   - Query MySQL: tìm ReviewEmbeddings tương tự
     - Option 1: Dùng MySQL Vector Search (MySQL 8.0.32+)
     - Option 2: Dùng External Vector DB (Qdrant/Weaviate)
     - Option 3: Load embeddings & compute similarity in-memory (cho MVP)
   - Return phim + review liên quan
3. Display trending reviews/movies

---

### UC7: Quản Lý Role (Admin)
**Actors**: Admin User
**Main Flow**:
1. Admin promote/demote user (USER → MODERATOR → ADMIN)
2. Admin có quyền delete review/comment spam

---

## 🔌 Thiết Kế API

### RESTful API Endpoints

#### **1. Authentication Endpoints**

##### 1.1 Đăng Ký
```
POST /api/auth/register
Content-Type: application/json

Request:
{
  "username": "john_cinema",
  "email": "john@example.com",
  "password": "SecurePass123!"
}

Response: 200 OK
{
  "id": 1,
  "username": "john_cinema",
  "email": "john@example.com",
  "role": "USER",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "createdAt": "2026-03-29T10:30:00Z"
}

Error: 400 Bad Request
{
  "error": "Email already exists"
}
```

##### 1.2 Đăng Nhập
```
POST /api/auth/login
Content-Type: application/json

Request:
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}

Response: 200 OK
{
  "id": 1,
  "username": "john_cinema",
  "email": "john@example.com",
  "role": "USER",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 86400
}

Error: 401 Unauthorized
{
  "error": "Invalid credentials"
}
```

---

#### **2. User Endpoints**

##### 2.1 Lấy Thông Tin User
```
GET /api/users/{id}

Response: 200 OK
{
  "id": 1,
  "username": "john_cinema",
  "email": "john@example.com",
  "bio": "Movie enthusiast",
  "avatarUrl": "https://...",
  "role": "USER",
  "isActive": true,
  "followerCount": 42,
  "followingCount": 30,
  "reviewCount": 15,
  "createdAt": "2026-01-15T10:30:00Z",
  "updatedAt": "2026-03-28T15:45:00Z"
}

Error: 404 Not Found
{
  "error": "User not found"
}
```

##### 2.2 Cập Nhật Profil (Auth Required)
```
PUT /api/users/{id}
Authorization: Bearer {token}
Content-Type: application/json

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
  "avatarUrl": "https://new-avatar-url",
  "updatedAt": "2026-03-29T11:00:00Z"
}

Error: 401 Unauthorized
{
  "error": "Not authenticated"
}

Error: 403 Forbidden
{
  "error": "Cannot edit other user's profile"
}
```

##### 2.3 Lấy Danh Sách Review của User
```
GET /api/users/{id}/reviews?limit=10&offset=0&sort=createdAt:desc

Response: 200 OK
{
  "total": 15,
  "count": 10,
  "offset": 0,
  "data": [
    {
      "id": 1,
      "movieId": 550,
      "title": "Amazing movie!",
      "content": "This is...",
      "rating": 9.5,
      "hasSpoiler": false,
      "likeCount": 12,
      "commentCount": 3,
      "createdAt": "2026-03-28T10:30:00Z"
    }
  ]
}
```

---

#### **3. Movie Endpoints**

##### 3.1 Lấy Danh Sách Phim
```
GET /api/movies?limit=20&offset=0&sort=ratingAvg:desc&genre=action&year=2024

Response: 200 OK
{
  "total": 1250,
  "count": 20,
  "offset": 0,
  "data": [
    {
      "id": 550,
      "tmdbId": 278,
      "title": "The Shawshank Redemption",
      "localTitle": "Nhà Tù Shawshank",
      "description": "Two imprisoned...",
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
  ]
}
```

##### 3.2 Lấy Chi Tiết Phim
```
GET /api/movies/{id}

Response: 200 OK
{
  "id": 550,
  "tmdbId": 278,
  "title": "The Shawshank Redemption",
  ... (same as above)
}
```

##### 3.3 Tìm Kiếm Phim
```
GET /api/movies/search?keyword=inception&limit=10

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
```

---

#### **4. Review Endpoints**

##### 4.1 Tạo Review (Auth Required)
```
POST /api/reviews
Authorization: Bearer {token}
Content-Type: application/json

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
  "content": "This movie is incredible...",
  "rating": 9.5,
  "hasSpoiler": false,
  "isEdited": false,
  "likeCount": 0,
  "commentCount": 0,
  "createdAt": "2026-03-29T11:00:00Z",
  "user": {
    "id": 1,
    "username": "john_cinema"
  },
  "movie": {
    "id": 550,
    "title": "The Shawshank Redemption"
  }
}

Error: 400 Bad Request
{
  "error": "User already has a review for this movie"
}
```

##### 4.2 Cập Nhật Review (Auth Required)
```
PUT /api/reviews/{id}
Authorization: Bearer {token}
Content-Type: application/json

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
  ...
  "isEdited": true,
  "editedAt": "2026-03-29T12:00:00Z",
  "updatedAt": "2026-03-29T12:00:00Z"
}

Error: 403 Forbidden
{
  "error": "Can only edit your own reviews"
}
```

##### 4.3 Xóa Review (Auth Required)
```
DELETE /api/reviews/{id}
Authorization: Bearer {token}

Response: 204 No Content

Error: 403 Forbidden
{
  "error": "Can only delete your own reviews"
}
```

##### 4.4 Lấy Danh Sách Review của Phim
```
GET /api/movies/{movieId}/reviews?limit=10&offset=0&sort=createdAt:desc

Response: 200 OK
{
  "total": 456,
  "count": 10,
  "data": [
    {
      "id": 1,
      "userId": 1,
      "movieId": 550,
      "title": "Amazing masterpiece!",
      "content": "This movie is incredible...",
      "rating": 9.5,
      "hasSpoiler": false,
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
```

---

#### **5. Like Endpoints**

##### 5.1 Like/Unlike Review (Auth Required)
```
POST /api/reviews/{reviewId}/like
Authorization: Bearer {token}

Request: {} (empty body)

Response: 200 OK
{
  "liked": true,
  "likeCount": 13
}
```

##### 5.2 Lấy Danh Sách Like của Review
```
GET /api/reviews/{reviewId}/likes?limit=20&offset=0

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
```

---

#### **6. Comment Endpoints**

##### 6.1 Tạo Comment (Auth Required)
```
POST /api/reviews/{reviewId}/comments
Authorization: Bearer {token}
Content-Type: application/json

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
```

##### 6.2 Xóa Comment (Auth Required)
```
DELETE /api/reviews/{reviewId}/comments/{commentId}
Authorization: Bearer {token}

Response: 204 No Content

Error: 403 Forbidden
{
  "error": "Can only delete your own comments"
}
```

##### 6.3 Lấy Danh Sách Comment
```
GET /api/reviews/{reviewId}/comments?limit=10&offset=0&sort=createdAt:desc

Response: 200 OK
{
  "total": 3,
  "count": 3,
  "data": [
    {
      "id": 1,
      "userId": 1,
      "content": "I totally agree with this review!",
      "createdAt": "2026-03-29T11:10:00Z",
      "user": {
        "id": 1,
        "username": "john_cinema"
      }
    }
  ]
}
```

---

#### **7. Follow Endpoints**

##### 7.1 Follow/Unfollow User (Auth Required)
```
POST /api/users/{userId}/follow
Authorization: Bearer {token}

Request: {} (empty body)

Response: 200 OK
{
  "followed": true,
  "followerCount": 43
}
```

##### 7.2 Lấy Danh Sách Followers
```
GET /api/users/{userId}/followers?limit=20&offset=0

Response: 200 OK
{
  "total": 43,
  "count": 20,
  "data": [
    {
      "id": 2,
      "username": "jane_movie",
      "avatarUrl": "https://...",
      "followedAt": "2026-03-20T10:30:00Z"
    }
  ]
}
```

##### 7.3 Lấy Danh Sách Following
```
GET /api/users/{userId}/following?limit=20&offset=0

Response: 200 OK
{
  "total": 30,
  "count": 20,
  "data": [
    {
      "id": 3,
      "username": "movie_critic",
      "avatarUrl": "https://...",
      "followedAt": "2026-03-15T10:30:00Z"
    }
  ]
}
```

---

#### **8. Search & Discovery Endpoints (Future)**

##### 8.1 Semantic Search
```
POST /api/search/semantic
Authorization: Bearer {token}
Content-Type: application/json

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
```

##### 8.2 Get Trending Movies (Future)
```
GET /api/movies/trending?timeRange=week&limit=10

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
```

---

### Common Response Patterns

#### Success Response (200, 201)
```json
{
  "success": true,
  "data": { /* entity data */ },
  "meta": {
    "timestamp": "2026-03-29T11:00:00Z"
  }
}
```

#### Paginated Response
```json
{
  "success": true,
  "data": [ /* array of entities */ ],
  "pagination": {
    "total": 100,
    "count": 10,
    "limit": 10,
    "offset": 0,
    "hasMore": true
  }
}
```

#### Error Response (4xx, 5xx)
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "User already has a review for this movie",
    "details": [
      {
        "field": "movieId",
        "message": "Review already exists"
      }
    ]
  },
  "timestamp": "2026-03-29T11:00:00Z"
}
```

---

## 🔄 Luồng Dữ Liệu

### Luồng 1: Tạo Review & Generate Embeddings
```
┌─────────────┐
│   User      │
├─────────────┤
│ POST Review │
└──────┬──────┘
       │
       ▼
┌──────────────────────────────┐
│  ReviewController            │
│  - Validate input            │
│  - Check auth                │
│  - Check 1 review/user/movie │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  ReviewService               │
│  - Create review             │
│  - Update Movie.RatingAvg    │
│  - Update denormalized counts│
│  - Queue embedding task      │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  Database (EF Core)          │
│  INSERT Review               │
│  UPDATE Movie stats          │
│  UPDATE User.ReviewCount     │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  BackgroundJob / Queue       │
│  - Get Movie description     │
│  - Get Review content        │
│  - Call OpenAI API           │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  OpenAI API                  │
│  text-embedding-3-small      │
│  (1536 dimensions)           │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  Create ReviewEmbedding      │
│  INSERT vectors to DB        │
│  (pgvector)                  │
└──────────────────────────────┘
```

### Luồng 2: Like/Unlike Review
```
┌──────────────┐
│   User       │
├──────────────┤
│ POST /like   │
└──────┬───────┘
       │
       ▼
┌──────────────────────────────┐
│  LikeController              │
│  - Check auth                │
│  - ReviewId validation       │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  LikeService                 │
│  IF already liked:           │
│    - DELETE Like             │
│    - Decrement count         │
│  ELSE:                       │
│    - INSERT Like             │
│    - Increment count         │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  Database (EF Core)          │
│  INSERT/DELETE Like          │
│  UPDATE Review.LikeCount     │
└──────────────────────────────┘
```

### Luồng 3: Follow User
```
┌──────────────────────┐
│    User A            │
├──────────────────────┤
│ POST /users/B/follow │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────────────┐
│  FollowController            │
│  - Check auth                │
│  - UserB exists              │
│  - Can't follow self         │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  FollowService               │
│  IF already following:       │
│    - DELETE Follow           │
│    - Decrement counts        │
│  ELSE:                       │
│    - INSERT Follow           │
│    - Increment counts        │
│    (A.FollowingCount++)      │
│    (B.FollowerCount++)       │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  Database (EF Core)          │
│  INSERT/DELETE Follow        │
│  UPDATE User.FollowerCount   │
│  UPDATE User.FollowingCount  │
└──────────────────────────────┘
```

### Luồng 4: Semantic Search (Future)
```
┌──────────────────────┐
│    User              │
├──────────────────────┤
│ POST /search/semantic│
│ Query: "phim buồn"   │
└──────┬───────────────┘
       │
       ▼
┌──────────────────────────────┐
│  SearchController            │
│  - Validate query            │
│  - Check auth                │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  SearchService               │
│  - Call OpenAI API           │
│  - Convert query → embedding │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  OpenAI API                  │
│  Query embedding (1536d)     │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  Database (pgvector)         │
│  Vector similarity search    │
│  SELECT top-k similar        │
│  ReviewEmbeddings            │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  Aggregate Results           │
│  - Get Movies                │
│  - Get Reviews               │
│  - Rank by similarity        │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  Return Results              │
│  (with similarity scores)    │
└──────────────────────────────┘
```

---

## 📈 Error Handling

### HTTP Status Codes
| Code | Meaning | Ví Dụ |
|------|---------|-------|
| **200** | OK | Retrieve, update success |
| **201** | Created | New resource created |
| **204** | No Content | Delete success |
| **400** | Bad Request | Invalid data, validation failed |
| **401** | Unauthorized | Not authenticated |
| **403** | Forbidden | No permission |
| **404** | Not Found | Resource doesn't exist |
| **409** | Conflict | Duplicate (email, username) |
| **413** | Payload Too Large | Content/request too big |
| **429** | Too Many Requests | Rate limit exceeded |
| **500** | Server Error | Unexpected error |

### Common Error Codes
```
USER_NOT_FOUND
USER_EXISTS (email/username)
INVALID_CREDENTIALS
REVIEW_ALREADY_EXISTS
MOVIE_NOT_FOUND
UNAUTHORIZED_ACTION
VALIDATION_ERROR
EXTERNAL_API_ERROR (when calling TMDB/OpenAI)
```

---

## 🔐 Security Considerations

1. **Authentication**: JWT tokens with expiration (24h recommended)
2. **Authorization**: Role-based access control (USER, MODERATOR, ADMIN)
3. **Input Validation**: Server-side validation for all inputs
4. **Rate Limiting**: Prevent abuse (e.g., 10 reviews/hour per user)
5. **CORS**: Configure for frontend domain
6. **SQL Injection**: Use EF Core parameterized queries (default)
7. **HTTPS**: Enforce in production
8. **Password**: Hash using bcrypt/PBKDF2 (not plain MD5)

---

## 📅 Implementation Timeline

### Phase 1: Core API (Weeks 1-2)
- [ ] User Controller & Service
- [ ] Authentication (Register, Login, JWT)
- [ ] Movie Controller & Integration with TMDB
- [ ] Review CRUD

### Phase 2: Social Features (Weeks 3-4)
- [ ] Like/Unlike functionality
- [ ] Comment system
- [ ] Follow/Unfollow
- [ ] Denormalized counter updates

### Phase 3: AI Features (Weeks 5-6)
- [ ] ReviewEmbedding generation (async job, store in MySQL LONGBLOB)
- [ ] Choose vector search strategy:
       - [ ] Option A: MySQL Vector Search (8.0.32+)
       - [ ] Option B: External Vector DB (Qdrant/Weaviate)
       - [ ] Option C: In-memory similarity search (MVP)
- [ ] Semantic search endpoint
- [ ] Recommendation engine

### Phase 4: Polish & Deploy (Week 7)
- [ ] Error handling & logging
- [ ] Performance optimization
- [ ] API documentation (Swagger)
- [ ] Unit tests & integration tests
- [ ] Deployment to staging/production

---

## 📚 References

- **TMDB API**: https://www.themoviedb.org/settings/api
- **OpenAI Embedding**: https://platform.openai.com/docs/guides/embeddings
- **MySQL Vector Search**: https://dev.mysql.com/doc/mysql-vector-search/en/
- **Qdrant Vector DB**: https://qdrant.tech/
- **Weaviate Vector DB**: https://weaviate.io/
- **JWT**: https://jwt.io/
- **ASP.NET Core**: https://learn.microsoft.com/en-us/aspnet/core

---

## 📝 Notes

- Deployments will require database migrations after schema changes
- Async jobs for embedding generation recommended to avoid blocking
- Vector embeddings stored as LONGBLOB in MySQL; optimize storage for large-scale
- For high-performance semantic search, consider external vector DB (Qdrant/Weaviate)
- Consider implementing caching for frequently accessed movies/reviews
- Monitor API performance with logging/metrics
- Plan for horizontal scaling of API servers

---

**Document Version**: 1.0  
**Last Updated**: 2026-03-29  
**Author**: CinePass Team
