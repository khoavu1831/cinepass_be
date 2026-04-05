using CinePass_be.Data;
using CinePass_be.Models;
using Microsoft.EntityFrameworkCore;

namespace CinePass_be.Repositories;

public class MovieRepository : IMovieRepository
{
  private readonly AppDbContext _db;

  public MovieRepository(AppDbContext db)
  {
    _db = db;
  }

  public async Task<List<Movie>> GetAllAsync(
      int page = 1,
      int pageSize = 20,
      string? sortBy = "releaseDate",
      string? order = "desc",
      string? search = null)
  {
    var query = _db.Movies.AsNoTracking();

    // Search filter
    if (!string.IsNullOrWhiteSpace(search))
    {
      query = query.Where(m =>
          m.Title.ToLower().Contains(search.ToLower()) ||
          (m.LocalTitle != null && m.LocalTitle.ToLower().Contains(search.ToLower())));
    }

    // Sort
    query = (sortBy?.ToLower(), order?.ToLower()) switch
    {
      ("releasedate", "asc") => query.OrderBy(m => m.ReleaseDate),
      ("releasedate", _) => query.OrderByDescending(m => m.ReleaseDate),
      ("ratingavg", "asc") => query.OrderBy(m => m.RatingAvg),
      ("ratingavg", _) => query.OrderByDescending(m => m.RatingAvg),
      ("title", "asc") => query.OrderBy(m => m.Title),
      ("title", _) => query.OrderByDescending(m => m.Title),
      (_, "asc") => query.OrderBy(m => m.ReleaseDate),
      _ => query.OrderByDescending(m => m.ReleaseDate),
    };

    // Pagination
    return await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
  }

  public async Task<int> GetTotalCountAsync(string? search = null)
  {
    var query = _db.Movies.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(search))
    {
      query = query.Where(m =>
          m.Title.ToLower().Contains(search.ToLower()) ||
          (m.LocalTitle != null && m.LocalTitle.ToLower().Contains(search.ToLower())));
    }

    return await query.CountAsync();
  }

  public async Task<Movie?> GetByIdAsync(int id)
  {
    return await _db.Movies
        .AsNoTracking()
        .FirstOrDefaultAsync(m => m.Id == id);
  }

  public async Task<Movie?> GetByTmdbIdAsync(int tmdbId)
  {
    return await _db.Movies
        .AsNoTracking()
        .FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
  }

  public async Task<Movie> CreateAsync(Movie movie)
  {
    _db.Movies.Add(movie);
    await _db.SaveChangesAsync();
    return movie;
  }

  public async Task<Movie> UpdateAsync(Movie movie)
  {
    _db.Movies.Update(movie);
    await _db.SaveChangesAsync();
    return movie;
  }

  public async Task<bool> DeleteAsync(int id)
  {
    var movie = await _db.Movies.FindAsync(id);
    if (movie == null)
      return false;

    _db.Movies.Remove(movie);
    await _db.SaveChangesAsync();
    return true;
  }
}
