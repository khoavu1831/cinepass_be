namespace CinePass_be.DTOs.Movie;

public class AdminFetchMoviesRequestDto
{
  public string? Type { get; set; } // "popular", "top_rated", "upcoming", "now_playing"
  public int GenreId { get; set; } = 0; // For fetching by genre
  public string? SortBy { get; set; } = "popularity.desc"; // For discover endpoint
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 20; // How many to fetch and save from TMDB
  public string Region { get; set; } = "US";
}

public class UpdateMovieDto
{
  public string? LocalTitle { get; set; }
  public string? Description { get; set; }
  public string? Director { get; set; }
  public string? Cast { get; set; }
  public bool? IsActive { get; set; } = true;
}

public class BatchOperationResultDto
{
  public int SuccessCount { get; set; }
  public int FailureCount { get; set; }
  public List<string>? Errors { get; set; } = [];
  public List<MovieResponseDto>? SavedMovies { get; set; } = [];
}
