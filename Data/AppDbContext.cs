using CinePass_be.Models;
using Microsoft.EntityFrameworkCore;

namespace CinePass_be.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // ===== Core MVP DbSets (7 tables) ===== //
    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<ReviewEmbedding> ReviewEmbeddings => Set<ReviewEmbedding>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ========== 1. USER ==========
        b.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);

            // Unique constraints
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();

            // Property configs
            e.Property(u => u.Role).HasConversion<string>();

            // Relationships
            e.HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(u => u.Likes)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Followers (self-referencing)
            e.HasMany(u => u.FollowersCollection)
                .WithOne(f => f.Following)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Following (self-referencing)
            e.HasMany(u => u.FollowingCollection)
                .WithOne(f => f.Follower)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Refresh token
            e.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== 2. MOVIE ==========
        b.Entity<Movie>(e =>
        {
            e.HasKey(m => m.Id);

            // Unique & indexes
            e.HasIndex(m => m.TmdbId).IsUnique();
            e.HasIndex(m => m.ReviewCount).IsDescending();
            e.HasIndex(m => m.RatingAvg).IsDescending();

            // Property configs
            e.Property(m => m.RatingAvg)
                .HasPrecision(4, 2)  // Allows: 0.00 - 99.99
                .HasDefaultValue(0m);

            // Relationships
            e.HasMany(m => m.Reviews)
                .WithOne(r => r.Movie)
                .HasForeignKey(r => r.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(m => m.ReviewEmbeddings)
                .WithOne(re => re.Movie)
                .HasForeignKey(re => re.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== 3. REVIEW ==========
        b.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);

            // Unique & indexes
            e.HasIndex(r => new { r.UserId, r.MovieId })
                .IsUnique()  // One review per user per movie
                .HasDatabaseName("IX_Review_UserMovie_Unique");

            e.HasIndex(r => r.CreatedAt).IsDescending();
            e.HasIndex(r => r.Rating).IsDescending();
            e.HasIndex(r => r.LikeCount).IsDescending();

            // Property configs
            e.Property(r => r.Rating)
                .HasPrecision(4, 1);  // Allows: 0.0 - 99.9 (for 1-10 scale)

            // Relationships
            e.HasMany(r => r.Comments)
                .WithOne(c => c.Review)
                .HasForeignKey(c => c.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(r => r.Likes)
                .WithOne(l => l.Review)
                .HasForeignKey(l => l.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.ReviewEmbedding)
                .WithOne(re => re.Review)
                .HasForeignKey<ReviewEmbedding>(re => re.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ========== 4. COMMENT ==========
        b.Entity<Comment>(e =>
        {
            e.HasKey(c => c.Id);

            // Indexes
            e.HasIndex(c => c.CreatedAt).IsDescending();
            e.HasIndex(c => c.ReviewId);
            e.HasIndex(c => c.UserId);
        });

        // ========== 5. LIKE ==========
        b.Entity<Like>(e =>
        {
            e.HasKey(l => l.Id);

            // Unique constraint: ensure one like per user per review
            e.HasIndex(l => new { l.UserId, l.ReviewId })
                .IsUnique()
                .HasDatabaseName("IX_Like_UserReview_Unique");
        });

        // ========== 6. FOLLOW ==========
        b.Entity<Follow>(e =>
        {
            e.HasKey(f => f.Id);

            // Unique constraint: one follow relationship per pair
            e.HasIndex(f => new { f.FollowerId, f.FollowingId })
                .IsUnique()
                .HasDatabaseName("IX_Follow_FollowerFollowing_Unique");

            // Additional indexes for queries
            e.HasIndex(f => f.FollowerId);
            e.HasIndex(f => f.FollowingId);

            // Navigation properties already configured in User entity
        });

        // ========== 7. REVIEW_EMBEDDING (AI Vectors) ==========
        b.Entity<ReviewEmbedding>(e =>
        {
            e.HasKey(re => re.Id);

            // Indexes
            e.HasIndex(re => re.ReviewId).IsUnique();
            e.HasIndex(re => re.MovieId);
            e.HasIndex(re => re.CreatedAt);

            // Note: For pgvector, these will need IVFFlat or HNSW indexes at database level:
            // CREATE INDEX ON review_embeddings USING ivfflat 
            //     (movie_description_vector vector_cosine_ops) WITH (lists = 100);
            // This migration should be done separately via raw SQL or pgvector extension setup
        });
    }
}
