using StockManagement.Domain.Common;
using StockManagement.Domain.Enums;
using StockManagement.Domain.Entities.Inventory;

namespace StockManagement.Domain.Entities;

public class Product : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? QrCode { get; set; }
    public string? ImageUrl { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Discount { get; set; }
    public string Unit { get; set; } = "Piece";
    public int StockQuantity { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public int ReorderLevel { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? BatchNumber { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public bool IsFeatured { get; set; }
    public bool IsDeleted { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
