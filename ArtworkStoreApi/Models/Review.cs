namespace ArtworkStoreApi.Models
{
    public class Review
    {

        public int Id { get; set; }
        public int ArtworkId { get; set; }
        public Artwork Artwork { get; set; }
        public string ReviewerFirstName { get; set; }
        public string ReviewerLastName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }

    }
}
