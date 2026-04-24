using System.Text.Json;
using CinePass_be.DTOs;
using CinePass_be.Models;
using CinePass_be.Repositories;
using CinePass_be.Clients.Tmdb;

namespace CinePass_be.Services;

public class MovieService : IMovieService
{
  private readonly IMovieRepository _movieRepository;
  private readonly ITmdbClient _tmdbClient;

  public MovieService(IMovieRepository movieRepository, ITmdbClient tmdbClient)
  {
    _movieRepository = movieRepository;
    _tmdbClient = tmdbClient;
  }

  public async Task<PaginatedResponseDto<MovieResponseDto>> GetAllAsync(
      int page = 1,
      int pageSize = 20,
      string? sortBy = "releaseDate",
      string? order = "desc",
      string? search = null)
  {
    // Validate page and pageSize
    if (page < 1) page = 1;
    if (pageSize < 1 || pageSize > 100) pageSize = 20;

    var movies = await _movieRepository.GetAllAsync(page, pageSize, sortBy, order, search);
    var total = await _movieRepository.GetTotalCountAsync(search);

    var movieDtos = movies.Select(MapToDto).ToList();

    return new PaginatedResponseDto<MovieResponseDto>
    {
      Data = movieDtos,
      Total = total,
      Page = page,
      PageSize = pageSize
    };
  }

  public async Task<MovieResponseDto> GetByIdAsync(int id)
  {
    if (id <= 0)
      throw new Exception("Id không hợp lệ - Movie Service");

    var movie = await _movieRepository.GetByIdAsync(id)
        ?? throw new Exception($"Không tìm thấy phim có id = {id} - Movie Service");

    return MapToDto(movie);
  }

  public async Task<MovieResponseDto?> FetchAndSaveFromTmdbAsync(int tmdbId)
  {
    try
    {
      // Check if movie already exists
      var existingMovie = await _movieRepository.GetByTmdbIdAsync(tmdbId);
      if (existingMovie != null)
      {
        return MapToDto(existingMovie);
      }

      // Fetch from TMDB
      var tmdbMovie = await _tmdbClient.GetMovieDetailsAsync(tmdbId);
      if (tmdbMovie == null)
        throw new Exception($"Could not fetch movie from TMDB: {tmdbId}");

      // Convert and save
      var movieModel = ConvertTmdbToMovieModel(tmdbMovie);
      var savedMovie = await _movieRepository.CreateAsync(movieModel);

      return MapToDto(savedMovie);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error fetching TMDB movie {tmdbId}: {ex.Message}");
      return null;
    }
  }

  public async Task<List<MovieResponseDto>> SearchAndFetchFromTmdbAsync(string query, int page = 1)
  {
    var results = new List<MovieResponseDto>();

    try
    {
      if (string.IsNullOrWhiteSpace(query))
        return results;

      // Search on TMDB
      var searchResults = await _tmdbClient.SearchMoviesAsync(query, page);
      if (searchResults?.Results == null || searchResults.Results.Count == 0)
        return results;

      // Fetch details and save each movie
      foreach (var searchResult in searchResults.Results.Take(10)) // Limit to top 10 results
      {
        try
        {
          // Check if already exists
          var existingMovie = await _movieRepository.GetByTmdbIdAsync(searchResult.Id);
          if (existingMovie != null)
          {
            results.Add(MapToDto(existingMovie));
            continue;
          }

          // Fetch full details
          var tmdbMovie = await _tmdbClient.GetMovieDetailsAsync(searchResult.Id);
          if (tmdbMovie == null)
            continue;

          // Convert and save
          var movieModel = ConvertTmdbToMovieModel(tmdbMovie);
          var savedMovie = await _movieRepository.CreateAsync(movieModel);
          results.Add(MapToDto(savedMovie));
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Error processing search result: {ex.Message}");
        }
      }

      return results;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error searching TMDB: {ex.Message}");
      return results;
    }
  }

  public async Task<object> SearchTmdbPreviewAsync(string query, int page = 1)
  {
    if (string.IsNullOrWhiteSpace(query))
      return new { data = new List<object>(), total = 0 };

    var tmdbResponse = await _tmdbClient.SearchMoviesAsync(query, page);
    if (tmdbResponse?.Results == null)
      return new { data = new List<object>(), total = 0 };

    var results = tmdbResponse.Results.Select(r => new
    {
      TmdbId = r.Id,
      Title = r.Title,
      Year = !string.IsNullOrEmpty(r.ReleaseDate) && r.ReleaseDate.Length >= 4
          ? r.ReleaseDate.Substring(0, 4) : "",
      PosterUrl = !string.IsNullOrEmpty(r.PosterPath)
          ? $"https://image.tmdb.org/t/p/w500{r.PosterPath}" : null,
      Rating = r.VoteAverage
    }).ToList<object>();

    return new { data = results, total = results.Count };
  }

  private Movie ConvertTmdbToMovieModel(TmdbMovieDetailsResponse tmdbMovie)
  {
    // Extract genres
    var genreNames = tmdbMovie.Genres?.Select(g => g.Name).ToList() ?? [];
    var genresJson = genreNames.Count > 0
        ? JsonSerializer.Serialize(genreNames)
        : "[]";

    // Extract director
    var director = tmdbMovie.Credits?.Crew
        .FirstOrDefault(c => c.Job.Equals("Director", StringComparison.OrdinalIgnoreCase))
        ?.Name;

    // Extract cast (top 5)
    var cast = tmdbMovie.Credits?.Cast
        .OrderBy(c => c.Order)
        .Take(5)
        .Select(c => c.Name)
        .ToList() ?? [];
    var castJson = cast.Count > 0 ? string.Join(", ", cast) : null;

    // Extract trailer URL
    var trailerUrl = tmdbMovie.Videos?.Results
        .FirstOrDefault(v => v.Type.Equals("Trailer", StringComparison.OrdinalIgnoreCase)
                            && v.Site.Equals("YouTube", StringComparison.OrdinalIgnoreCase))
        ?.Key;

    var trailerFullUrl = !string.IsNullOrEmpty(trailerUrl)
        ? $"https://www.youtube.com/watch?v={trailerUrl}"
        : null;

    var posterUrl = !string.IsNullOrEmpty(tmdbMovie.PosterPath)
        ? $"https://image.tmdb.org/t/p/w500{tmdbMovie.PosterPath}"
        : null;

    var backdropUrl = !string.IsNullOrEmpty(tmdbMovie.BackdropPath)
        ? $"https://image.tmdb.org/t/p/w1280{tmdbMovie.BackdropPath}"
        : null;

    DateOnly? releaseDate = null;
    if (!string.IsNullOrEmpty(tmdbMovie.ReleaseDate) && DateOnly.TryParse(tmdbMovie.ReleaseDate, out var date))
    {
      releaseDate = date;
    }

    return new Movie
    {
      TmdbId = tmdbMovie.Id,
      Title = tmdbMovie.Title,
      LocalTitle = null, // Will be filled manually or by admin
      Description = tmdbMovie.Overview,
      PosterUrl = posterUrl,
      BackdropUrl = backdropUrl,
      TrailerUrl = trailerFullUrl,
      Duration = tmdbMovie.Runtime > 0 ? tmdbMovie.Runtime : null,
      ReleaseDate = releaseDate,
      Language = tmdbMovie.OriginalLanguage,
      Director = director,
      Cast = castJson,
      GenresJson = genresJson,
      RatingAvg = 0m, // Will be calculated from user reviews
      ReviewCount = 0,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };
  }

  private MovieResponseDto MapToDto(Movie movie)
  {
    var genres = new List<string>();

    // Parse genres from JSON
    if (!string.IsNullOrWhiteSpace(movie.GenresJson))
    {
      try
      {
        genres = JsonSerializer.Deserialize<List<string>>(movie.GenresJson) ?? [];
      }
      catch
      {
        // If JSON parsing fails, return empty list
        genres = [];
      }
    }

    return new MovieResponseDto
    {
      Id = movie.Id,
      TmdbId = movie.TmdbId,
      Title = movie.Title,
      LocalTitle = movie.LocalTitle,
      Description = movie.Description,
      PosterUrl = movie.PosterUrl,
      BackdropUrl = movie.BackdropUrl,
      TrailerUrl = movie.TrailerUrl,
      Duration = movie.Duration,
      ReleaseDate = movie.ReleaseDate?.ToString("yyyy-MM-dd"),
      Director = movie.Director,
      Cast = movie.Cast,
      Genres = genres,
      RatingAvg = movie.RatingAvg,
      ReviewCount = movie.ReviewCount,
      CreatedAt = movie.CreatedAt,
      UpdatedAt = movie.UpdatedAt
    };
  }

  // Admin methods
  public async Task<BatchOperationResultDto> FetchAndSaveMovieListAsync(AdminFetchMoviesRequestDto request)
  {
    var result = new BatchOperationResultDto();
    var errors = new List<string>();

    try
    {
      TmdbSearchMovieResponse? response = null;

      // Fetch from TMDB based on type
      switch (request.Type?.ToLower())
      {
        case "popular":
          response = await _tmdbClient.GetPopularMoviesAsync(request.Page, request.Region);
          break;
        case "top_rated":
          response = await _tmdbClient.GetTopRatedMoviesAsync(request.Page);
          break;
        case "upcoming":
          response = await _tmdbClient.GetUpcomingMoviesAsync(request.Page, request.Region);
          break;
        case "now_playing":
          response = await _tmdbClient.GetNowPlayingMoviesAsync(request.Page, request.Region);
          break;
        case "genre":
          response = await _tmdbClient.GetMoviesByGenreAsync(request.GenreId, request.Page, request.SortBy ?? "popularity.desc");
          break;
        default:
          throw new Exception("Invalid type. Must be: popular, top_rated, upcoming, now_playing, or genre");
      }

      if (response?.Results == null || response.Results.Count == 0)
      {
        result.FailureCount = 1;
        errors.Add("No movies found from TMDB");
        result.Errors = errors;
        return result;
      }

      result.SavedMovies = [];

      // Process each movie
      foreach (var tmdbMovie in response.Results.Take(request.PageSize))
      {
        try
        {
          // Check if already exists
          var existingMovie = await _movieRepository.GetByTmdbIdAsync(tmdbMovie.Id);
          if (existingMovie != null)
          {
            result.SuccessCount++;
            result.SavedMovies.Add(MapToDto(existingMovie));
            continue;
          }

          // Fetch full details
          var fullDetails = await _tmdbClient.GetMovieDetailsAsync(tmdbMovie.Id);
          if (fullDetails == null)
          {
            result.FailureCount++;
            errors.Add($"Failed to fetch details for TMDB ID: {tmdbMovie.Id}");
            continue;
          }

          // Convert and save
          var movieModel = ConvertTmdbToMovieModel(fullDetails);
          var savedMovie = await _movieRepository.CreateAsync(movieModel);
          result.SuccessCount++;
          result.SavedMovies.Add(MapToDto(savedMovie));
        }
        catch (Exception ex)
        {
          result.FailureCount++;
          errors.Add($"Error processing movie {tmdbMovie.Id}: {ex.Message}");
        }
      }

      result.Errors = errors;
      return result;
    }
    catch (Exception ex)
    {
      result.FailureCount = 1;
      result.Errors = new List<string> { $"Error: {ex.Message}" };
      return result;
    }
  }

  public async Task<MovieResponseDto?> UpdateMovieAsync(int id, UpdateMovieDto dto)
  {
    try
    {
      if (id <= 0)
        throw new Exception("Id không hợp lệ");

      var movie = await _movieRepository.GetByIdAsync(id)
          ?? throw new Exception($"Không tìm thấy phim có id = {id}");

      // Update fields
      if (!string.IsNullOrWhiteSpace(dto.LocalTitle))
        movie.LocalTitle = dto.LocalTitle;

      if (!string.IsNullOrWhiteSpace(dto.Description))
        movie.Description = dto.Description;

      if (!string.IsNullOrWhiteSpace(dto.Director))
        movie.Director = dto.Director;

      if (!string.IsNullOrWhiteSpace(dto.Cast))
        movie.Cast = dto.Cast;

      if (dto.IsActive.HasValue)
      {
        // Note: Movie model doesn't have IsActive, but you can add it if needed
        // For now, just updating other fields
      }

      movie.UpdatedAt = DateTime.UtcNow;

      var updatedMovie = await _movieRepository.UpdateAsync(movie);
      return MapToDto(updatedMovie);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error updating movie: {ex.Message}");
      return null;
    }
  }

  public async Task<bool> DeleteMovieAsync(int id)
  {
    try
    {
      if (id <= 0)
        throw new Exception("Id không hợp lệ");

      return await _movieRepository.DeleteAsync(id);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error deleting movie: {ex.Message}");
      return false;
    }
  }
}
