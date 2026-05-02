# Cinepass – Các tính năng đã được hoàn thiện (May 2026)

## Backend (cinepass-backend)

### 1. Review System (Mới hoàn toàn)
- DTOs: CreateReviewDto, UpdateReviewDto, ReviewResponseDto, CreateCommentDto, CommentResponseDto
- Repository: IReviewRepository + ReviewRepository (CRUD review, like toggle, CRUD comment)
- Service: IReviewService + ReviewService (cập nhật rating trung bình, counters tự động)
- Controller: ReviewController
  - GET /api/movies/{movieId}/reviews
  - POST /api/movies/{movieId}/reviews
  - PUT /api/movies/{movieId}/reviews/{reviewId}
  - DELETE /api/movies/{movieId}/reviews/{reviewId}
  - POST /api/movies/{movieId}/reviews/{reviewId}/like
  - GET /api/movies/{movieId}/reviews/{reviewId}/comments
  - POST /api/movies/{movieId}/reviews/{reviewId}/comments
  - DELETE /api/movies/{movieId}/reviews/{reviewId}/comments/{commentId}

### 2. Follow System (Mới hoàn toàn)
- Repository: IFollowRepository + FollowRepository
- UserController thêm: GET /reviews, POST /follow, DELETE /follow, GET /follow-status
- UserService thêm: IncrementFollowCountersAsync, DecrementFollowCountersAsync

### 3. RBAC Enhancements
- Policies: AdminOnly, SuperAdminOnly trong Program.cs
- ReviewController nhận diện admin để xóa bất kỳ review nào

### 4. Auth Response Improvements
- AuthResponseDto bổ sung AvatarUrl và Bio
- AuthService.LoginAsync trả về đầy đủ thông tin user

## Frontend (cinepass-frontend)

### 1. Review Tab trên Movie Page
- ReviewsTab.jsx: danh sách reviews, form viết review, edit/delete, like toggle
- Tab "Đánh giá" thêm vào Tabs.jsx và Tab.jsx

### 2. User Profile Page (Mới hoàn toàn)
- Route: /profile/:id
- Hiển thị avatar, stats, edit modal, follow/unfollow, danh sách reviews

### 3. Services mới
- reviewService.js, userService.js

### 4. Bug Fixes
- Info.jsx: hỗ trợ cả backend format và TMDB format
- PlayBars.jsx: dùng ratingAvg thay vì vote_average
- Signin.jsx: sửa response mapping
- Signup.jsx: sửa field username
- Header.jsx: link profile, hiển thị username thực
