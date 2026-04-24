using CinePass_be.Data;
using CinePass_be.Models;
using Microsoft.EntityFrameworkCore;

namespace CinePass_be.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly AppDbContext _db;

    public CollectionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Collection>> GetAllWithMoviesAsync()
    {
        return await _db.Collections
            .Include(c => c.CollectionMovies)
                .ThenInclude(cm => cm.Movie)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Collection?> GetByIdWithMoviesAsync(int id)
    {
        return await _db.Collections
            .Include(c => c.CollectionMovies)
                .ThenInclude(cm => cm.Movie)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Collection> CreateAsync(Collection collection)
    {
        _db.Collections.Add(collection);
        await _db.SaveChangesAsync();
        return collection;
    }

    public async Task<Collection> UpdateAsync(Collection collection)
    {
        _db.Collections.Update(collection);
        await _db.SaveChangesAsync();
        return collection;
    }

    public async Task DeleteAsync(Collection collection)
    {
        _db.Collections.Remove(collection);
        await _db.SaveChangesAsync();
    }

    public async Task<CollectionMovie?> GetCollectionMovieAsync(int collectionId, int movieId)
    {
        return await _db.CollectionMovies
            .FirstOrDefaultAsync(x => x.CollectionId == collectionId && x.MovieId == movieId);
    }

    public async Task AddCollectionMovieAsync(CollectionMovie collectionMovie)
    {
        _db.CollectionMovies.Add(collectionMovie);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveCollectionMovieAsync(CollectionMovie collectionMovie)
    {
        _db.CollectionMovies.Remove(collectionMovie);
        await _db.SaveChangesAsync();
    }

    public async Task<List<CollectionMovie>> GetCollectionMoviesAsync(int collectionId)
    {
        return await _db.CollectionMovies
            .Where(cm => cm.CollectionId == collectionId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
