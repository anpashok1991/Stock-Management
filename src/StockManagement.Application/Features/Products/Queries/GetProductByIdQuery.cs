using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Features.Products.Queries;

public class GetProductByIdQuery : IRequest<Result<ProductDto>>
{
    public Guid Id { get; set; }
}

internal class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IAppDbContext _context;

    public GetProductByIdQueryHandler(IAppDbContext context) => _context = context;

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var p = await _context.Products.FindAsync(new object[] { request.Id }, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Product), request.Id);

        var dto = new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Sku = p.Sku,
            Barcode = p.Barcode,
            ImageUrl = p.ImageUrl,
            CostPrice = p.CostPrice,
            SellingPrice = p.SellingPrice,
            TaxRate = p.TaxRate,
            Discount = p.Discount,
            Unit = p.Unit,
            StockQuantity = p.StockQuantity,
            MinimumStock = p.MinimumStock,
            MaximumStock = p.MaximumStock,
            Status = p.Status,
            IsFeatured = p.IsFeatured,
            CategoryId = p.CategoryId,
            BrandId = p.BrandId,
            CreatedAt = p.CreatedAt
        };

        return Result<ProductDto>.Success(dto);
    }
}
