namespace ArtworkStoreApi.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    
    // Foreign Keys
    public int OrderId { get; set; }
    public int ArtworkId { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; }
    public virtual Artwork Artwork { get; set; }
}