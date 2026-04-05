namespace CinePass_be.DTOs.Movie;

public class FetchFromTmdbRequestDto
{
    public int TmdbId { get; set; }
}

public class SearchTmdbMoviesRequestDto
{
    public string Query { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
}
