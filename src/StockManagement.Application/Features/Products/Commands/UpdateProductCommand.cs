using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Products.Commands;

public class UpdateProductCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
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
    public string? ImageUrl { get; set; }
    public ProductStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? SupplierId { get; set; }
}

internal class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IAppDbContext _context;

    public UpdateProductCommandHandler(IAppDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Product), request.Id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Sku = request.Sku;
        product.CostPrice = request.CostPrice;
        product.SellingPrice = request.SellingPrice;
        product.TaxRate = request.TaxRate;
        product.Discount = request.Discount;
        product.Unit = request.Unit;
        product.StockQuantity = request.StockQuantity;
        product.MinimumStock = request.MinimumStock;
        product.MaximumStock = request.MaximumStock;
        product.ReorderLevel = request.ReorderLevel;
        product.ExpiryDate = request.ExpiryDate;
        product.BatchNumber = request.BatchNumber;
        product.ImageUrl = request.ImageUrl;
        product.Status = request.Status;
        product.IsFeatured = request.IsFeatured;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.SupplierId = request.SupplierId;
        product.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
