using ArtworkStoreApi.Models;
using ArtworkStoreApi.Repositories;

namespace ArtworkStoreApi.Extensions;

public static class RepositoryExtensions
{
    // Artwork specific extensions
    public static async Task<IEnumerable<Artwork>> GetArtworksByGenreAsync(
        this IGenericRepository<Artwork> repository, int genreId)
    {
        return await repository.FindAsync(a => a.GenreId == genreId);
    }

    public static async Task<IEnumerable<Artwork>> GetFeaturedArtworksAsync(
        this IGenericRepository<Artwork> repository)
    {
        return await repository.FindAsync(a => a.IsFeatured);
    }

    public static async Task<IEnumerable<Artwork>> GetArtworksByPriceRangeAsync(
        this IGenericRepository<Artwork> repository, decimal minPrice, decimal maxPrice)
    {
        return await repository.FindAsync(a => a.Price >= minPrice && a.Price <= maxPrice);
    }

    // Order specific extensions
    public static async Task<IEnumerable<Order>> GetOrdersByUserAsync(
        this IGenericRepository<Order> repository, int userId)
    {
        return await repository.FindAsync(o => o.UserId == userId);
    }

    public static async Task<IEnumerable<Order>> GetOrdersByStatusAsync(
        this IGenericRepository<Order> repository, string status)
    {
        return await repository.FindAsync(o => o.Status == status);
    }

    // Review specific extensions  
    public static async Task<IEnumerable<Review>> GetReviewsByArtworkAsync(
        this IGenericRepository<Review> repository, int artworkId)
    {
        return await repository.FindAsync(r => r.ArtworkId == artworkId);
    }

    public static async Task<double> GetAverageRatingAsync(
        this IGenericRepository<Review> repository, int artworkId)
    {
        var reviews = await repository.FindAsync(r => r.ArtworkId == artworkId);
        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    // Genre specific extensions
    public static async Task<IEnumerable<Genre>> GetActiveGenresAsync(
        this IGenericRepository<Genre> repository)
    {
        return await repository.FindAsync(g => g.IsActive);
    }
}