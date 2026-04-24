using CinePass_be.Models;

namespace CinePass_be.Repositories;

public interface ICollectionRepository
{
    Task<List<Collection>> GetAllWithMoviesAsync();
    Task<Collection?> GetByIdWithMoviesAsync(int id);
    Task<Collection> CreateAsync(Collection collection);
    Task<Collection> UpdateAsync(Collection collection);
    Task DeleteAsync(Collection collection);
    Task<CollectionMovie?> GetCollectionMovieAsync(int collectionId, int movieId);
    Task AddCollectionMovieAsync(CollectionMovie collectionMovie);
    Task RemoveCollectionMovieAsync(CollectionMovie collectionMovie);
    Task<List<CollectionMovie>> GetCollectionMoviesAsync(int collectionId);
    Task SaveChangesAsync();
}
