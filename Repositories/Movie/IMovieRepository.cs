using CinePass_be.Models;

namespace CinePass_be.Repositories;

public interface IMovieRepository
{
  Task<List<Movie>> GetAllAsync(
      int page = 1,
      int pageSize = 20,
      string? sortBy = "releaseDate",
      string? order = "desc",
      string? search = null);
  Task<int> GetTotalCountAsync(string? search = null);
  Task<Movie?> GetByIdAsync(int id);
  Task<Movie?> GetByTmdbIdAsync(int tmdbId);
  Task<Movie> CreateAsync(Movie movie);
  Task<Movie> UpdateAsync(Movie movie);
  Task<bool> DeleteAsync(int id);
}
