using StockManagement.Domain.Common;
using StockManagement.Domain.Entities.Inventory;
using StockManagement.Domain.Enums;

namespace StockManagement.Domain.Entities.Manufacturing;

public class ManufacturingOrder : TenantEntity
{
    public string ManufacturingNumber { get; set; } = string.Empty;
    public Guid FinishedProductId { get; set; }
    public Product? FinishedProduct { get; set; }
    public Guid? BillOfMaterialId { get; set; }
    public BillOfMaterial? BillOfMaterial { get; set; }
    public int Quantity { get; set; }
    public ManufacturingStatus Status { get; set; } = ManufacturingStatus.Draft;
    public DateTime ProductionDate { get; set; } = DateTime.UtcNow;
    public decimal TotalMaterialCost { get; set; }
    public decimal AdditionalManufacturingCost { get; set; }
    public decimal LabourCost { get; set; }
    public decimal PackagingCost { get; set; }
    public decimal EstimatedProductCost { get; set; }
    public Guid? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public string? Remarks { get; set; }

    public ICollection<ManufacturingOrderItem> Items { get; set; } = new List<ManufacturingOrderItem>();
}
