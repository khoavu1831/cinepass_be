using CinePass_be.DTOs;

namespace CinePass_be.Services;

public interface ICollectionService
{
    Task<IEnumerable<CollectionDto>> GetAllAsync();
    Task<CollectionDto> CreateAsync(CreateCollectionDto dto);
    Task<CollectionDto?> UpdateAsync(int id, UpdateCollectionDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> AddMovieAsync(int collectionId, int movieId);
    Task<bool> RemoveMovieAsync(int collectionId, int movieId);
    Task<bool> ReorderMoviesAsync(int collectionId, List<int> movieIds);
}
