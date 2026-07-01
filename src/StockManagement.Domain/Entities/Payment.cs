using StockManagement.Domain.Common;
using StockManagement.Domain.Enums;

namespace StockManagement.Domain.Entities;

public class Payment : TenantEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public decimal Amount { get; set; }
    public PaymentMode Mode { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionRef { get; set; }
    public string? Notes { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}
