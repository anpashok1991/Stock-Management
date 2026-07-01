using StockManagement.Domain.Common;

namespace StockManagement.Domain.Entities;

public class ProductAttributeValue : TenantEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid AttributeDefinitionId { get; set; }
    public ProductAttributeDefinition? AttributeDefinition { get; set; }
    public string Value { get; set; } = string.Empty;
}
