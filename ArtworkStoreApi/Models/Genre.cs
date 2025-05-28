namespace ArtworkStoreApi.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
        // Navigation properties
        public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
    }
}
