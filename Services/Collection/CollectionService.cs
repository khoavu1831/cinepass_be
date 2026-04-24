using CinePass_be.DTOs;
using CinePass_be.Models;
using CinePass_be.Repositories;

namespace CinePass_be.Services;

public class CollectionService : ICollectionService
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IMovieRepository _movieRepository;

    public CollectionService(ICollectionRepository collectionRepository, IMovieRepository movieRepository)
    {
        _collectionRepository = collectionRepository;
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<CollectionDto>> GetAllAsync()
    {
        var collections = await _collectionRepository.GetAllWithMoviesAsync();

        return collections.Select(c => new CollectionDto
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Type = c.Type,
            ThumbnailUrl = c.ThumbnailUrl,
            DisplayOrder = c.DisplayOrder,
            Movies = c.CollectionMovies
                .OrderBy(cm => cm.OrderIndex)
                .Select(cm => MapMovieToDto(cm.Movie))
                .ToList()
        });
    }

    public async Task<CollectionDto> CreateAsync(CreateCollectionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Title không được để trống");

        var collection = new Collection
        {
            Title = dto.Title,
            Description = dto.Description,
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "standard_horizontal" : dto.Type,
            ThumbnailUrl = dto.ThumbnailUrl,
            DisplayOrder = dto.DisplayOrder
        };

        var created = await _collectionRepository.CreateAsync(collection);

        return new CollectionDto
        {
            Id = created.Id,
            Title = created.Title,
            Description = created.Description,
            Type = created.Type,
            ThumbnailUrl = created.ThumbnailUrl,
            DisplayOrder = created.DisplayOrder,
            Movies = []
        };
    }

    public async Task<CollectionDto?> UpdateAsync(int id, UpdateCollectionDto dto)
    {
        var collection = await _collectionRepository.GetByIdWithMoviesAsync(id);
        if (collection == null) return null;

        collection.Title = dto.Title;
        collection.Description = dto.Description;
        collection.Type = string.IsNullOrWhiteSpace(dto.Type) ? "standard_horizontal" : dto.Type;
        collection.ThumbnailUrl = dto.ThumbnailUrl;
        collection.DisplayOrder = dto.DisplayOrder;

        var updated = await _collectionRepository.UpdateAsync(collection);

        return new CollectionDto
        {
            Id = updated.Id,
            Title = updated.Title,
            Description = updated.Description,
            Type = updated.Type,
            ThumbnailUrl = updated.ThumbnailUrl,
            DisplayOrder = updated.DisplayOrder,
            Movies = updated.CollectionMovies
                .OrderBy(cm => cm.OrderIndex)
                .Select(cm => MapMovieToDto(cm.Movie))
                .ToList()
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var collection = await _collectionRepository.GetByIdWithMoviesAsync(id);
        if (collection == null) return false;

        await _collectionRepository.DeleteAsync(collection);
        return true;
    }

    public async Task<bool> AddMovieAsync(int collectionId, int movieId)
    {
        var collection = await _collectionRepository.GetByIdWithMoviesAsync(collectionId);
        if (collection == null)
            throw new KeyNotFoundException("Không tìm thấy collection");

        var movie = await _movieRepository.GetByIdAsync(movieId);
        if (movie == null)
            throw new KeyNotFoundException("Không tìm thấy phim");

        if (collection.CollectionMovies.Any(cm => cm.MovieId == movieId))
            throw new InvalidOperationException("Phim đã tồn tại trong collection");

        var nextOrder = collection.CollectionMovies.Count > 0
            ? collection.CollectionMovies.Max(cm => cm.OrderIndex) + 1
            : 0;

        await _collectionRepository.AddCollectionMovieAsync(new CollectionMovie
        {
            CollectionId = collectionId,
            MovieId = movieId,
            OrderIndex = nextOrder
        });

        return true;
    }

    public async Task<bool> RemoveMovieAsync(int collectionId, int movieId)
    {
        var cm = await _collectionRepository.GetCollectionMovieAsync(collectionId, movieId);
        if (cm == null)
            throw new KeyNotFoundException("Không tìm thấy phim trong collection");

        await _collectionRepository.RemoveCollectionMovieAsync(cm);
        return true;
    }

    public async Task<bool> ReorderMoviesAsync(int collectionId, List<int> movieIds)
    {
        var collectionMovies = await _collectionRepository.GetCollectionMoviesAsync(collectionId);
        if (!collectionMovies.Any()) return true;

        for (int i = 0; i < movieIds.Count; i++)
        {
            var cm = collectionMovies.FirstOrDefault(x => x.MovieId == movieIds[i]);
            if (cm != null)
                cm.OrderIndex = i;
        }

        await _collectionRepository.SaveChangesAsync();
        return true;
    }

    private static MovieResponseDto MapMovieToDto(Movie movie)
    {
        var genres = new List<string>();

        if (!string.IsNullOrWhiteSpace(movie.GenresJson))
        {
            try
            {
                genres = System.Text.Json.JsonSerializer.Deserialize<List<string>>(movie.GenresJson) ?? [];
            }
            catch
            {
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
}
