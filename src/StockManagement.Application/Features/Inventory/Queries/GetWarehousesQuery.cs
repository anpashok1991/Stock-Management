using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Inventory.Queries;

public class GetWarehousesQuery : IRequest<Result<List<WarehouseDto>>>
{
}

public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

internal class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, Result<List<WarehouseDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetWarehousesQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<List<WarehouseDto>>> Handle(GetWarehousesQuery request, CancellationToken ct)
    {
        var query = _context.Warehouses.AsQueryable();
        if (_tenant.TenantId.HasValue)
            query = query.Where(w => w.TenantId == _tenant.TenantId.Value);

        var items = await query
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Name = w.Name
            })
            .ToListAsync(ct);

        return Result<List<WarehouseDto>>.Success(items);
    }
}
