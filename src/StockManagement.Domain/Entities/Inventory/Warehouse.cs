using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities.Inventory;

public class Warehouse : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}
