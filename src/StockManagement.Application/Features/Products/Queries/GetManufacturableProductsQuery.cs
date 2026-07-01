using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Products.Queries;

public class GetManufacturableProductsQuery : IRequest<Result<List<ManufacturableProductDto>>>
{
    public string? SearchTerm { get; set; }
}

public class ManufacturableProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string Unit { get; set; } = "Piece";
    public int StockQuantity { get; set; }
    public ProductStatus Status { get; set; }
    public string? CategoryName { get; set; }
}

internal class GetManufacturableProductsQueryHandler : IRequestHandler<GetManufacturableProductsQuery, Result<List<ManufacturableProductDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetManufacturableProductsQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<List<ManufacturableProductDto>>> Handle(GetManufacturableProductsQuery request, CancellationToken ct)
    {
        var query = _context.Products
            .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active)
            .AsQueryable();

        if (_tenant.TenantId.HasValue)
            query = query.Where(p => p.TenantId == _tenant.TenantId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || (p.Sku != null && p.Sku.ToLower().Contains(term)));
        }

        var items = await query
            .OrderBy(p => p.Name)
            .Select(p => new ManufacturableProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Sku = p.Sku,
                ImageUrl = p.ImageUrl,
                CostPrice = p.CostPrice,
                SellingPrice = p.SellingPrice,
                Unit = p.Unit,
                StockQuantity = p.StockQuantity,
                Status = p.Status,
                CategoryName = p.Category != null ? p.Category.Name : null
            })
            .ToListAsync(ct);

        return Result<List<ManufacturableProductDto>>.Success(items);
    }
}
