namespace CinePass_be.DTOs.Movie;

public class MovieResponseDto
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
  public string? ReleaseDate { get; set; }
  public string? Director { get; set; }
  public string? Cast { get; set; }
  public List<string>? Genres { get; set; } = [];
  public decimal RatingAvg { get; set; }
  public int ReviewCount { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
}
