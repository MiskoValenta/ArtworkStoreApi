using System.ComponentModel.DataAnnotations;

namespace ArtworkStoreApi.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public string UserEmail { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    public class OrderCreateDto
    {
        [Required]
        public string ShippingAddress { get; set; }
    
        [Required]
        public List<OrderItemCreateDto> OrderItems { get; set; } = new List<OrderItemCreateDto>();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string ArtworkTitle { get; set; }
        public string ArtworkImageUrl { get; set; }
    }

    public class OrderItemCreateDto
    {
        [Required]
        public int ArtworkId { get; set; }
    
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
