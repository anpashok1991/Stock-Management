using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities.Manufacturing;

public class ManufacturingOrderItem : TenantEntity
{
    public Guid ManufacturingOrderId { get; set; }
    public ManufacturingOrder? ManufacturingOrder { get; set; }
    public Guid RawMaterialId { get; set; }
    public Product? RawMaterial { get; set; }
    public decimal QuantityRequired { get; set; }
    public decimal QuantityConsumed { get; set; }
    public string Unit { get; set; } = "Piece";
    public decimal WastagePercentage { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public int AvailableStock { get; set; }
    public bool IsOptional { get; set; }
    public string? Remarks { get; set; }
}
