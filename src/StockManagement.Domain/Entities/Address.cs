using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities;

public class Address : TenantEntity
{
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string Label { get; set; } = "Home";
    public string Street { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; } = "IN";
    public bool IsDefault { get; set; }
}
