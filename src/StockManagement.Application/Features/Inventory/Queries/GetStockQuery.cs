using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Inventory.Queries;

public class GetStockQuery : IRequest<Result<PagedResult<StockDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Guid? WarehouseId { get; set; }
    public bool LowStockOnly { get; set; }
}

public class StockDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumStock { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

internal class GetStockQueryHandler : IRequestHandler<GetStockQuery, Result<PagedResult<StockDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetStockQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<PagedResult<StockDto>>> Handle(GetStockQuery request, CancellationToken ct)
    {
        var query = _context.StockItems
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .AsQueryable();

        if (_tenant.TenantId.HasValue)
            query = query.Where(s => s.TenantId == _tenant.TenantId.Value);

        if (request.WarehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == request.WarehouseId.Value);

        if (request.LowStockOnly)
            query = query.Where(s => s.Quantity <= (s.Product != null ? s.Product.ReorderLevel : 0));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(s => s.Product!.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StockDto
            {
                ProductId = s.ProductId,
                ProductName = s.Product != null ? s.Product.Name : "",
                Sku = s.Product != null ? s.Product.Sku : null,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse != null ? s.Warehouse.Name : "",
                Quantity = s.Quantity,
                MinimumStock = s.Product != null ? s.Product.MinimumStock : 0,
                ReorderLevel = s.Product != null ? s.Product.ReorderLevel : 0,
                IsLowStock = s.Product != null && s.Quantity <= s.Product.ReorderLevel,
                BatchNumber = s.BatchNumber,
                ExpiryDate = s.ExpiryDate
            })
            .ToListAsync(ct);

        return Result<PagedResult<StockDto>>.Success(new PagedResult<StockDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}
