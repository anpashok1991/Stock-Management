using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities.Manufacturing;

public class ManufacturingCost : TenantEntity
{
    public Guid ManufacturingOrderId { get; set; }
    public ManufacturingOrder? ManufacturingOrder { get; set; }
    public decimal RawMaterialCost { get; set; }
    public decimal AdditionalManufacturingCost { get; set; }
    public decimal LabourCost { get; set; }
    public decimal PackagingCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal CostPerUnit { get; set; }
    public string? Notes { get; set; }
}
