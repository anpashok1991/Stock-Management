using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities.Manufacturing;

public class BillOfMaterialItem : TenantEntity
{
    public Guid BillOfMaterialId { get; set; }
    public BillOfMaterial? BillOfMaterial { get; set; }
    public Guid RawMaterialId { get; set; }
    public Product? RawMaterial { get; set; }
    public decimal QuantityRequired { get; set; }
    public string Unit { get; set; } = "Piece";
    public decimal WastagePercentage { get; set; }
    public bool IsOptional { get; set; }
    public string? Remarks { get; set; }
}
