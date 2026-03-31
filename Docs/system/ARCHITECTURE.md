# CinePass - Architecture Diagrams

## 📊 Entity Relationship Diagram (ERD)

```
User (Users)
├── PK: Id (int)
├── Username (string, UNIQUE)
├── Email (string, UNIQUE)
├── PasswordHash (string)
├── Bio (string)
├── AvatarUrl (string)
├── Role (string: USER | MODERATOR | ADMIN)
├── IsActive (bool)
├── FollowerCount (int, denormalized)
├── FollowingCount (int, denormalized)
├── ReviewCount (int, denormalized)
├── CreatedAt (datetime)
└── UpdatedAt (datetime)

      ↓ 1:n (FK: UserId)
      
├─ Review (Reviews)
│  ├── PK: Id (int)
│  ├── UserId (int, FK)
│  ├── MovieId (int, FK)
│  ├── Title (string)
│  ├── Content (string)
│  ├── Rating (decimal 1-10)
│  ├── HasSpoiler (bool)
│  ├── IsEdited (bool)
│  ├── EditedAt (datetime)
│  ├── LikeCount (int, denormalized)
│  ├── CommentCount (int, denormalized)
│  ├── UNIQUE(UserId, MovieId)
│  ├── CreatedAt (datetime)
│  └── UpdatedAt (datetime)
│      ↓ 1:n (FK: ReviewId)
│      ├─ Comment (Comments)
│      │  ├── PK: Id (int)
│      │  ├── UserId (int, FK, CASCADE)
│      │  ├── ReviewId (int, FK, CASCADE)
│      │  ├── Content (string)
│      │  ├── CreatedAt (datetime)
│      │  └── UpdatedAt (datetime)
│      │
│      └─ Like (Likes)
│         ├── PK: Id (int)
│         ├── UserId (int, FK, CASCADE)
│         ├── ReviewId (int, FK, CASCADE)
│         ├── UNIQUE(UserId, ReviewId)
│         └── CreatedAt (datetime)
│
├─ Like (Likes) - aggregated above
│
└─ ReviewEmbedding (ReviewEmbeddings)
   ├── PK: Id (int)
   ├── ReviewId (int, FK, UNIQUE, CASCADE)
   ├── MovieId (int, FK, CASCADE)
   ├── MovieDescriptionVector (LONGBLOB) - float32 binary
   ├── ReviewContentVector (LONGBLOB) - float32 binary
   ├── CombinedVector (LONGBLOB) - float32 binary
   ├── EmbeddingModel (string)
   ├── CreatedAt (datetime)
   └── UpdatedAt (datetime, nullable)
   
   (For vector search, use MySQL 8.0.32+ or external DB)

Movie (Movies)
├── PK: Id (int)
├── TmdbId (int, UNIQUE)
├── Title (string)
├── LocalTitle (string)
├── Description (string)
├── PosterUrl (string)
├── BackdropUrl (string)
├── TrailerUrl (string)
├── Duration (int)
├── ReleaseDate (date)
├── Language (string)
├── Director (string)
├── Cast (string)
├── GenresJson (string, JSON array)
├── RatingAvg (decimal 0.00-99.99, denormalized)
├── ReviewCount (int, denormalized)
├── CreatedAt (datetime)
└── UpdatedAt (datetime)

      ↓ 1:n (FK: MovieId, CASCADE)
      
├─ Review (Reviews) - see above
└─ ReviewEmbedding - see above

Follow (Follows) - Self-referencing
├── PK: Id (int)
├── FollowerId (int, FK → User, RESTRICT)
├── FollowingId (int, FK → User, RESTRICT)
├── UNIQUE(FollowerId, FollowingId)
└── CreatedAt (datetime)

(When FollowerId = User A and FollowingId = User B)
(User B.FollowerCount++ and User A.FollowingCount++)
```

---

## 🔄 Request-Response Flow

### Flow 1: Create Review with Embedding

```
Timeline: Request → Response (Async Job)
─────────────────────────────────────────

[User]
  │ POST /api/reviews
  │ {movieId: 550, title: "...", content: "...", rating: 9.5}
  │
  ├─→ [AuthMiddleware]
  │   ├─ Validate JWT token
  │   └─ Extract userId from token
  │
  ├─→ [ReviewController.CreateReview(dto)]
  │   ├─ ModelState validation
  │   └─ Call ReviewService
  │
  ├─→ [ReviewService.CreateReviewAsync(dto)]
  │   ├─ Check: Review doesn't exist (userId, movieId)
  │   ├─ Create Review entity
  │   ├─ Save to DB (EF Core)
  │   ├─ Update Movie.ReviewCount, RatingAvg
  │   ├─ Update User.ReviewCount
  │   └─ Queue async job: GenerateEmbedding(reviewId)
  │
  ├→ [Response] 201 Created
  │   {id: 1, userId: 1, movieId: 550, ...}
  │
  (Async - doesn't block response)
  │
  └─→ [BackgroundJobQueue]
      ├─ Get Review + Movie data
      ├─ Call OpenAI API
      │   └─ text-embedding-3-small
      │       → MovieDescriptionVector (1536 dims)
      │       → ReviewContentVector (1536 dims)
      │
      ├─ Create ReviewEmbedding
      └─ Save to DB (pgvector)
```

### Flow 2: Like/Unlike Review

```
[User A]
  │ POST /api/reviews/{reviewId}/like
  │
  ├─→ [AuthMiddleware]
  │   └─ userId = A
  │
  ├─→ [LikeController.ToggleLike(reviewId)]
  │   └─ Call LikeService
  │
  ├─→ [LikeService.ToggleLikeAsync(userId, reviewId)]
  │   │
  │   ├─ Check: existing Like?
  │   │
  │   ├─ IF EXISTS:
  │   │  ├─ DELETE Like from DB
  │   │  ├─ Review.LikeCount--
  │   │  └─ liked = false
  │   │
  │   └─ IF NOT EXISTS:
  │      ├─ INSERT new Like
  │      ├─ Review.LikeCount++
  │      └─ liked = true
  │
  └─→ [Response] 200 OK
      {liked: true/false, likeCount: 13}
```

### Flow 3: Follow/Unfollow User

```
[User A] wants to follow [User B]
  │
  │ POST /api/users/{userId=B}/follow
  │
  ├─→ [AuthMiddleware]
  │   └─ userId = A
  │
  ├─→ [FollowController.ToggleFollow(userIdB)]
  │   └─ Call FollowService
  │
  ├─→ [FollowService.ToggleFollowAsync(userIdA, userIdB)]
  │   │
  │   ├─ Check: already following?
  │   │
  │   ├─ IF EXISTS (already following):
  │   │  ├─ DELETE Follow(A → B)
  │   │  ├─ User A.FollowingCount--
  │   │  ├─ User B.FollowerCount--
  │   │  └─ followed = false
  │   │
  │   └─ IF NOT EXISTS:
  │      ├─ INSERT Follow(A → B)
  │      ├─ User A.FollowingCount++
  │      ├─ User B.FollowerCount++
  │      └─ followed = true
  │
  └─→ [Response] 200 OK
      {followed: true/false, followerCount: 43}
```

---

## 🎯 Main Workflows (Use Case Flows)

### UC1: User Registration & Login

```
┌─ New User Registration ─────────────────────────────────────────────┐
│                                                                     │
│  [User inputs]                                                      │
│   └─ username, email, password                                     │
│                                                                     │
│  [Frontend Form]                                                    │
│   └─ Validate: email format, password strength                     │
│                                                                     │
│  [API Request]                                                      │
│   POST /api/auth/register                                          │
│   {username, email, password}                                      │
│                                                                     │
│  [AuthController.Register]                                          │
│   ├─ ModelState validation                                         │
│   └─ Call AuthService.RegisterAsync                                │
│                                                                     │
│  [AuthService.RegisterAsync]                                        │
│   ├─ Check: Username already exists? → Error 409                  │
│   ├─ Check: Email already exists? → Error 409                     │
│   ├─ Hash password (bcrypt)                                        │
│   ├─ Create User entity (role = USER)                             │
│   ├─ Save to DB                                                    │
│   ├─ Generate JWT token                                            │
│   └─ Return {user, token}                                          │
│                                                                     │
│  [Response] 201 Created                                             │
│   {id, username, email, role, token, expiresIn}                   │
│                                                                     │
│  [Frontend]                                                         │
│   ├─ Store token in localStorage/secure cookie                    │
│   └─ Redirect to home page                                         │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘

┌─ Existing User Login ──────────────────────────────────────────────┐
│                                                                    │
│  [User inputs]                                                     │
│   └─ email, password                                              │
│                                                                    │
│  [API Request]                                                     │
│   POST /api/auth/login                                            │
│   {email, password}                                               │
│                                                                    │
│  [AuthController.Login]                                            │
│   └─ Call AuthService.LoginAsync                                  │
│                                                                    │
│  [AuthService.LoginAsync]                                          │
│   ├─ Find User by email                                           │
│   ├─ User exists? → No: Error 401                                │
│   ├─ Verify password (bcrypt)                                     │
│   ├─ Match? → No: Error 401                                      │
│   ├─ Generate JWT token                                           │
│   └─ Return {user, token}                                         │
│                                                                    │
│  [Response] 200 OK                                                 │
│   {id, username, email, role, token, expiresIn}                  │
│                                                                    │
│  [Frontend]                                                        │
│   ├─ Store token                                                  │
│   └─ Redirect to dashboard                                        │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

### UC2: Browse & Review Movies

```
┌─ User Browse Movies Flow ─────────────────────────────────────────┐
│                                                                   │
│  [User on Homepage]                                               │
│   └─ Wants to see trending/popular movies                        │
│                                                                   │
│  [Frontend requests]                                              │
│   GET /api/movies?sort=ratingAvg:desc&limit=20                  │
│                                                                   │
│  [MovieController.GetMovies(query)]                               │
│   ├─ Parse filters: sort, genre, year, limit, offset            │
│   └─ Call MovieService.GetMoviesAsync                            │
│                                                                   │
│  [MovieService.GetMoviesAsync]                                    │
│   ├─ Build query (EF Core)                                       │
│   ├─ Apply filters                                               │
│   ├─ Sort by specified field                                     │
│   ├─ Execute query                                               │
│   └─ Return {total, data, pagination}                            │
│                                                                   │
│  [Response] 200 OK                                                │
│   {total: 1250, count: 20, data: [Movies...]}                   │
│                                                                   │
│  [Frontend Display]                                               │
│   └─ Show movie cards (poster, title, rating, reviews)          │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘

┌─ User Write Review Flow ──────────────────────────────────────────┐
│                                                                   │
│  [User clicks movie]                                              │
│   └─ Views movie details                                         │
│                                                                   │
│  [Frontend Form]                                                  │
│   ├─ Rating: 1-10 slider                                         │
│   ├─ Title: text input                                           │
│   ├─ Content: long-form editor                                   │
│   └─ Spoiler checkbox                                            │
│                                                                   │
│  [User submits]                                                   │
│   POST /api/reviews                                              │
│   Authorization: Bearer {token}                                  │
│   {movieId, title, content, rating, hasSpoiler}                 │
│                                                                   │
│  [ReviewController.CreateReview]                                  │
│   ├─ Auth check (JWT validation)                                 │
│   ├─ ModelState validation                                       │
│   └─ Call ReviewService                                          │
│                                                                   │
│  [ReviewService.CreateReviewAsync]                                │
│   ├─ Check: Review doesn't exist                                 │
│   ├─ Create Review                                               │
│   ├─ Update Movie stats                                          │
│   ├─ Queue embedding job                                         │
│   └─ Return JSON                                                 │
│                                                                   │
│  [Response] 201 Created                                           │
│   {id, movieId, userId, rating, ...}                            │
│                                                                   │
│  [Frontend]                                                       │
│   ├─ Show success toast                                          │
│   └─ Refresh review list                                         │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
```

### UC3: Social Interaction (Like, Comment, Follow)

```
┌─ Like Review Flow ────────────────────────────────────────────────┐
│                                                                  │
│  [User on Review]                                                │
│   └─ Clicks heart icon                                           │
│                                                                  │
│  POST /api/reviews/{reviewId}/like                              │
│  Authorization: Bearer {token}                                  │
│                                                                  │
│  [LikeService.ToggleLikeAsync]                                   │
│   ├─ IF already liked:                                          │
│   │  ├─ DELETE Like                                             │
│   │  ├─ Review.LikeCount--                                      │
│   │  └─ liked = false                                           │
│   │                                                              │
│   └─ ELSE:                                                      │
│      ├─ INSERT Like                                             │
│      ├─ Review.LikeCount++                                      │
│      └─ liked = true                                            │
│                                                                  │
│  [Response] 200 OK                                               │
│   {liked: boolean, likeCount: number}                           │
│                                                                  │
│  [Frontend Update]                                               │
│   ├─ Toggle heart color                                         │
│   └─ Update count: likeCount                                    │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘

┌─ Comment on Review Flow ──────────────────────────────────────────┐
│                                                                  │
│  [User on Review]                                                │
│   └─ Types comment in textarea                                  │
│                                                                  │
│  POST /api/reviews/{reviewId}/comments                          │
│  Authorization: Bearer {token}                                  │
│  {content: "..."}                                               │
│                                                                  │
│  [CommentService.CreateCommentAsync]                             │
│   ├─ Validate: content not empty                               │
│   ├─ Create Comment                                             │
│   ├─ Review.CommentCount++                                      │
│   └─ Return Comment                                             │
│                                                                  │
│  [Response] 201 Created                                          │
│   {id, userId, reviewId, content, user: {...}, ...}            │
│                                                                  │
│  [Frontend]                                                      │
│   ├─ Add comment to comments list                              │
│   ├─ Update commentCount                                        │
│   └─ Clear textarea                                             │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘

┌─ Follow User Flow ────────────────────────────────────────────────┐
│                                                                  │
│  [User on Profile]                                               │
│   └─ Clicks "Follow" button                                     │
│                                                                  │
│  POST /api/users/{userId}/follow                                │
│  Authorization: Bearer {token}                                  │
│                                                                  │
│  [FollowService.ToggleFollowAsync]                               │
│   ├─ IF already following:                                      │
│   │  ├─ DELETE Follow                                           │
│   │  ├─ FollowingUser.FollowingCount--                          │
│   │  ├─ TargetUser.FollowerCount--                              │
│   │  └─ followed = false                                        │
│   │                                                              │
│   └─ ELSE:                                                      │
│      ├─ INSERT Follow                                           │
│      ├─ FollowingUser.FollowingCount++                          │
│      ├─ TargetUser.FollowerCount++                              │
│      └─ followed = true                                         │
│                                                                  │
│  [Response] 200 OK                                               │
│   {followed: boolean, followerCount: number}                    │
│                                                                  │
│  [Frontend]                                                      │
│   ├─ Toggle button state                                        │
│   └─ Update follower count                                      │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

---

## 🏛️ Database Schema (SQL)

```sql
-- Users Table
CREATE TABLE `Users` (
    `Id` INT PRIMARY KEY AUTO_INCREMENT,
    `Username` VARCHAR(100) NOT NULL UNIQUE,
    `Email` VARCHAR(255) NOT NULL UNIQUE,
    `PasswordHash` VARCHAR(255) NOT NULL,
    `Bio` TEXT,
    `AvatarUrl` VARCHAR(500),
    `Role` VARCHAR(50) NOT NULL DEFAULT 'USER',
    `IsActive` BOOLEAN NOT NULL DEFAULT true,
    `FollowerCount` INT NOT NULL DEFAULT 0,
    `FollowingCount` INT NOT NULL DEFAULT 0,
    `ReviewCount` INT NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME NOT NULL,
    `UpdatedAt` DATETIME
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Movies Table
CREATE TABLE `Movies` (
    `Id` INT PRIMARY KEY AUTO_INCREMENT,
    `TmdbId` INT UNIQUE,
    `Title` VARCHAR(255) NOT NULL,
    `LocalTitle` VARCHAR(255),
    `Description` TEXT,
    `PosterUrl` VARCHAR(500),
    `BackdropUrl` VARCHAR(500),
    `TrailerUrl` VARCHAR(500),
    `Duration` INT,
    `ReleaseDate` DATE,
    `Language` VARCHAR(10),
    `Director` VARCHAR(255),
    `Cast` TEXT,
    `GenresJson` JSON,
    `RatingAvg` DECIMAL(4,2) NOT NULL DEFAULT 0,
    `ReviewCount` INT NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME NOT NULL,
    `UpdatedAt` DATETIME,
    INDEX `IX_Movies_TmdbId` (`TmdbId`),
    INDEX `IX_Movies_ReviewCount` (`ReviewCount` DESC),
    INDEX `IX_Movies_RatingAvg` (`RatingAvg` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Reviews Table
CREATE TABLE `Reviews` (
    `Id` INT PRIMARY KEY AUTO_INCREMENT,
    `UserId` INT NOT NULL REFERENCES `Users`(`Id`) ON DELETE CASCADE,
    `MovieId` INT NOT NULL REFERENCES `Movies`(`Id`) ON DELETE CASCADE,
    `Title` VARCHAR(255) NOT NULL,
    `Content` LONGTEXT NOT NULL,
    `Rating` DECIMAL(4,1) NOT NULL,
    `HasSpoiler` BOOLEAN NOT NULL DEFAULT false,
    `IsEdited` BOOLEAN NOT NULL DEFAULT false,
    `EditedAt` DATETIME,
    `LikeCount` INT NOT NULL DEFAULT 0,
    `CommentCount` INT NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME NOT NULL,
    `UpdatedAt` DATETIME,
    UNIQUE KEY `UQ_Review_UserMovie` (`UserId`, `MovieId`),
    KEY `IX_Reviews_UserId` (`UserId`),
    KEY `IX_Reviews_MovieId` (`MovieId`),
    KEY `IX_Reviews_CreatedAt` (`CreatedAt` DESC),
    KEY `IX_Reviews_Rating` (`Rating` DESC),
    KEY `IX_Reviews_LikeCount` (`LikeCount` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Comments Table
CREATE TABLE `Comments` (
    `Id` INT PRIMARY KEY AUTO_INCREMENT,
    `UserId` INT NOT NULL REFERENCES `Users`(`Id`) ON DELETE CASCADE,
    `ReviewId` INT NOT NULL REFERENCES `Reviews`(`Id`) ON DELETE CASCADE,
    `Content` TEXT NOT NULL,
    `CreatedAt` DATETIME NOT NULL,
    `UpdatedAt` DATETIME,
    KEY `IX_Comments_ReviewId` (`ReviewId`),
    KEY `IX_Comments_UserId` (`UserId`),
    KEY `IX_Comments_CreatedAt` (`CreatedAt` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Likes Table
CREATE TABLE `Likes` (
    `Id` INT PRIMARY KEY AUTO_INCREMENT,
    `UserId` INT NOT NULL REFERENCES `Users`(`Id`) ON DELETE CASCADE,
    `ReviewId` INT NOT NULL REFERENCES `Reviews`(`Id`) ON DELETE CASCADE,
    `CreatedAt` DATETIME NOT NULL,
    UNIQUE KEY `UQ_Like_UserReview` (`UserId`, `ReviewId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Follows Table (Self-referencing)
CREATE TABLE `Follows` (
    `Id` INT PRIMARY KEY AUTO_INCREMENT,
    `FollowerId` INT NOT NULL REFERENCES `Users`(`Id`) ON DELETE RESTRICT,
    `FollowingId` INT NOT NULL REFERENCES `Users`(`Id`) ON DELETE RESTRICT,
    `CreatedAt` DATETIME NOT NULL,
    UNIQUE KEY `UQ_Follow_FollowerFollowing` (`FollowerId`, `FollowingId`),
    KEY `IX_Follows_FollowerId` (`FollowerId`),
    KEY `IX_Follows_FollowingId` (`FollowingId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ReviewEmbeddings Table (Vector Storage in LONGBLOB)
CREATE TABLE `ReviewEmbeddings` (
    `Id` INT PRIMARY KEY AUTO_INCREMENT,
    `ReviewId` INT NOT NULL UNIQUE REFERENCES `Reviews`(`Id`) ON DELETE CASCADE,
    `MovieId` INT NOT NULL REFERENCES `Movies`(`Id`) ON DELETE CASCADE,
    `MovieDescriptionVector` LONGBLOB,
    `ReviewContentVector` LONGBLOB,
    `CombinedVector` LONGBLOB,
    `EmbeddingModel` VARCHAR(100) NOT NULL DEFAULT 'text-embedding-3-small',
    `CreatedAt` DATETIME NOT NULL,
    `UpdatedAt` DATETIME,
    KEY `IX_ReviewEmbeddings_MovieId` (`MovieId`),
    KEY `IX_ReviewEmbeddings_CreatedAt` (`CreatedAt` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Note: Vector embeddings stored as binary LONGBLOB (float32 format)
-- For semantic search, choose one of the following:
-- Option 1: MySQL Vector Search (MySQL 8.0.32+) - native support
-- Option 2: External Vector DB (Qdrant/Weaviate) - for production scale
-- Option 3: In-memory similarity (load vectors to app, compute cosine distance)
```

---

**Architecture Documentation Complete**
