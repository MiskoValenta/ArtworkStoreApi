using System.ComponentModel.DataAnnotations;

namespace ArtworkStoreApi.DTOs
{
    public class ArtworkDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public int GenreId { get; set; }
        public decimal Price { get; set; }
        public required string Description { get; set; }
        public int Quantity { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public double AverageRating { get; set; }
        public string GenreName { get; set; }
    }
    public class ArtworkCreateDto
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
