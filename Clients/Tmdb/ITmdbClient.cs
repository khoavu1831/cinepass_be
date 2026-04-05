using CinePass_be.DTOs.Tmdb;

namespace CinePass_be.Clients.Tmdb;

public interface ITmdbClient
{
    Task<TmdbMovieDetailsResponse?> GetMovieDetailsAsync(int tmdbId);
    Task<TmdbSearchMovieResponse?> SearchMoviesAsync(string query, int page = 1);
    Task<TmdbSearchMovieResponse?> GetPopularMoviesAsync(int page = 1, string region = "US");
    Task<TmdbSearchMovieResponse?> GetTopRatedMoviesAsync(int page = 1);
    Task<TmdbSearchMovieResponse?> GetUpcomingMoviesAsync(int page = 1, string region = "US");
    Task<TmdbSearchMovieResponse?> GetMoviesByGenreAsync(int genreId, int page = 1, string sortBy = "popularity.desc");
    Task<TmdbSearchMovieResponse?> GetNowPlayingMoviesAsync(int page = 1, string region = "US");
}