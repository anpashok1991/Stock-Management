using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Products.Queries;

public class GetProductsQuery : IRequest<Result<PagedResult<ProductDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public ProductStatus? Status { get; set; }
    public bool? IsFeatured { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? ImageUrl { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Discount { get; set; }
    public string Unit { get; set; } = "Piece";
    public int StockQuantity { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public ProductStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public string? CategoryName { get; set; }
    public Guid CategoryId { get; set; }
    public string? BrandName { get; set; }
    public Guid? BrandId { get; set; }
    public DateTime CreatedAt { get; set; }
}

internal class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetProductsQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var query = _context.Products
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (_tenant.TenantId.HasValue)
            query = query.Where(p => p.TenantId == _tenant.TenantId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || (p.Sku != null && p.Sku.ToLower().Contains(term)));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.BrandId.HasValue)
            query = query.Where(p => p.BrandId == request.BrandId.Value);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.SellingPrice >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.SellingPrice <= request.MaxPrice.Value);

        var totalCount = await query.CountAsync(ct);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => request.SortDesc ? query.OrderByDescending(p => p.SellingPrice) : query.OrderBy(p => p.SellingPrice),
            "created" => request.SortDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
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
                CategoryName = p.Category != null ? p.Category.Name : null,
                CategoryId = p.CategoryId,
                BrandName = p.Brand != null ? p.Brand.Name : null,
                BrandId = p.BrandId,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return Result<PagedResult<ProductDto>>.Success(new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}
