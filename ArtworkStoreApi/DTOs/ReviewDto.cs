
using ArtworkStoreApi.Models;

namespace ArtworkStoreApi.DTOs
{
    public record class ReviewDto
    {
        public int ArtworkId { get; set; }
        public string ReviewerFirstName { get; set; }
        public string ReviewerLastName { get; set; }
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; }
    }