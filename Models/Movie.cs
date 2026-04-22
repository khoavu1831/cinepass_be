namespace CinePass_be.Models;

public class Movie
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
    public string? Language { get; set; }
    public string? Director { get; set; }
    public string? Cast { get; set; }

    public string? GenresJson { get; set; }

    public decimal RatingAvg { get; set; } = 0m;
    public int ReviewCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<ReviewEmbedding> ReviewEmbeddings { get; set; } = [];
    public ICollection<MovieGenre> MovieGenres { get; set; } = [];
    public ICollection<CollectionMovie> CollectionMovies { get; set; } = [];
}
