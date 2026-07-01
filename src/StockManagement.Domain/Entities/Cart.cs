using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities;

public class Cart : TenantEntity
{
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? SessionId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
