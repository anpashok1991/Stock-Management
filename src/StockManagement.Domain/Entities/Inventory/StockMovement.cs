using StockManagement.Domain.Common;
using StockManagement.Domain.Enums;

namespace StockManagement.Domain.Entities.Inventory;

/// <summary>
/// Immutable ledger of every stock movement for audit and traceability.
/// </summary>
public class StockMovement : TenantEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
