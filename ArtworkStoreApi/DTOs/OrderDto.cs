namespace ArtworkStoreApi.DTOs
{
    public record class OrderDto
    {
        public int ArtworkId { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public string DeliveryMethod { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
