namespace ArtworkStoreApi.DTOs
{
    public record class ArtworkDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public int GenreId { get; set; }
        public decimal Price { get; set; }
        public required string Description { get; set; }
        public int Quantity { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public double AverageRating { get; set; }
    }
}
