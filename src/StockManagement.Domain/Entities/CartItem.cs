using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities;

public class CartItem : TenantEntity
{
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
