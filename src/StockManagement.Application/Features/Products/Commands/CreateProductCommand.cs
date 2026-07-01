using AutoMapper;
using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Features.Products.Commands;

public class CreateProductCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
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
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Guid? SupplierId { get; set; }
}

internal class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public CreateProductCommandHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Sku = request.Sku,
            Barcode = request.Barcode,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            TaxRate = request.TaxRate,
            Discount = request.Discount,
            Unit = request.Unit,
            StockQuantity = request.StockQuantity,
            MinimumStock = request.MinimumStock,
            MaximumStock = request.MaximumStock,
            ReorderLevel = request.ReorderLevel,
            ExpiryDate = request.ExpiryDate,
            BatchNumber = request.BatchNumber,
            ImageUrl = request.ImageUrl,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            SupplierId = request.SupplierId,
            TenantId = _tenant.TenantId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(product.Id);
    }
}
