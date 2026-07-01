using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities;

public class ProductAttributeDefinition : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? DataType { get; set; } = "string";
    public bool IsRequired { get; set; }
    public bool IsFilterable { get; set; }

    public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
}
