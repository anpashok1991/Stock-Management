using StockManagement.Domain.Common;
using StockManagement.Domain.Enums;

namespace StockManagement.Domain.Entities.Manufacturing;

public class BillOfMaterial : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid FinishedProductId { get; set; }
    public Product? FinishedProduct { get; set; }
    public string VersionNumber { get; set; } = "1.0";
    public BOMStatus Status { get; set; } = BOMStatus.Draft;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<BillOfMaterialItem> Items { get; set; } = new List<BillOfMaterialItem>();
}
