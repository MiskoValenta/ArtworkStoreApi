namespace ArtworkStoreApi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; } // 1-5 stars
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsApproved { get; set; } = false;
    
        // Foreign Keys
        public int UserId { get; set; }
        public int ArtworkId { get; set; }
    
        // Navigation properties
        public virtual User User { get; set; }
        public virtual Artwork Artwork { get; set; }

    }
}
