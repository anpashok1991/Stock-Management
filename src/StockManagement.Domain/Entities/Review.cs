using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities;

public class Review : TenantEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsVerified { get; set; }
    public bool IsApproved { get; set; }
}
