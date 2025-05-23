using System.ComponentModel.DataAnnotations;

namespace ArtworkStoreApi.DTOs
{
    public record class ArtworkCreateDto
    {
        [Required] string? Title { get; set; }
        int GenreId { get; set; }
        decimal Price { get; set; }
        [Required] string? Description { get; set; }
        int Quantity { get; set; }
        DateOnly ReleaseDate { get; set; }
        double AverageRating { get; set; }
    }
}
