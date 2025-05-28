using System.ComponentModel.DataAnnotations;

namespace ArtworkStoreApi.DTOs
{
    public class GenreDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ArtworksCount { get; set; }
    }

    public class GenreCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    
        [MaxLength(500)]
        public string Description { get; set; }
    
        public bool IsActive { get; set; } = true;
    }
}
