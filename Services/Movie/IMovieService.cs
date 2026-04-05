using CinePass_be.DTOs.Movie;
using CinePass_be.DTOs.Tmdb;

namespace CinePass_be.Services;

public interface IMovieService
{
  Task<PaginatedResponseDto<MovieResponseDto>> GetAllAsync(
      int page = 1,
      int pageSize = 20,
      string? sortBy = "releaseDate",
      string? order = "desc",
      string? search = null);
  Task<MovieResponseDto> GetByIdAsync(int id);
  Task<MovieResponseDto?> FetchAndSaveFromTmdbAsync(int tmdbId);
  Task<List<MovieResponseDto>> SearchAndFetchFromTmdbAsync(string query, int page = 1);
  Task<BatchOperationResultDto> FetchAndSaveMovieListAsync(AdminFetchMoviesRequestDto request);
  Task<MovieResponseDto?> UpdateMovieAsync(int id, UpdateMovieDto dto);
  Task<bool> DeleteMovieAsync(int id);
}
