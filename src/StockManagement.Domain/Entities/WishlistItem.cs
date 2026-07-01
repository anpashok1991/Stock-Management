using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities;

public class WishlistItem : TenantEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
