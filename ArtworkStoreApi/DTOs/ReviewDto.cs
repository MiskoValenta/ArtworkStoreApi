
using System.ComponentModel.DataAnnotations;
using ArtworkStoreApi.Models;

namespace ArtworkStoreApi.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsApproved { get; set; }
        public string UserEmail { get; set; }
        public string ArtworkTitle { get; set; }
    }

    public class ReviewCreateDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
    
        [MaxLength(1000)]
        public string Comment { get; set; }
    
        [Required]
        public int ArtworkId { get; set; }
    }
}