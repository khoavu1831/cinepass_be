namespace CinePass_be.Models;

/// <summary>
/// Movie model - Catalog from TMDB API
/// Stores movie metadata for review platform
/// </summary>
public class Movie
{
    // Identity
    public int Id { get; set; }
    public int? TmdbId { get; set; }                    // Reference to TMDB API

    // Content
    public string Title { get; set; } = string.Empty;  // English title
    public string? LocalTitle { get; set; }             // Vietnamese title (optional)
    public string? Description { get; set; }            // Plot/synopsis (used for AI embedding)
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string? TrailerUrl { get; set; }

    // Metadata
    public int? Duration { get; set; }                  // Minutes
    public DateOnly? ReleaseDate { get; set; }
    public string? Language { get; set; }               // e.g., "en", "vi"
    public string? Director { get; set; }
    public string? Cast { get; set; }                   // JSON array or comma-separated

    // Genres (stored as JSON array: ["Action", "Drama"])
    public string? GenresJson { get; set; }

    // Aggregated Stats (denormalized)
    public decimal RatingAvg { get; set; } = 0m;       // 0.0-10.0, calculated from reviews
    public int ReviewCount { get; set; } = 0;          // Count of reviews
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<ReviewEmbedding> ReviewEmbeddings { get; set; } = [];

}
