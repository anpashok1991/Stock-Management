using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities.Inventory;

/// <summary>
/// Per-warehouse, per-product stock level. One row per product-warehouse combination.
/// </summary>
public class StockItem : TenantEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public int Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
