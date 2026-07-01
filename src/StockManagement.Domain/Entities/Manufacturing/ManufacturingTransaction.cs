using StockManagement.Domain.Common;
using StockManagement.Domain.Enums;

namespace StockManagement.Domain.Entities.Manufacturing;

public class ManufacturingTransaction : TenantEntity
{
    public Guid ManufacturingOrderId { get; set; }
    public ManufacturingOrder? ManufacturingOrder { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public ManufacturingTransactionType Type { get; set; }
    public int Quantity { get; set; }
    public int BeforeQuantity { get; set; }
    public int AfterQuantity { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? ManufacturingNumber { get; set; }
    public string? Remarks { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
