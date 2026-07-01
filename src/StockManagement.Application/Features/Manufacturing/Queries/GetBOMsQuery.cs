using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Manufacturing.Queries;

public class GetBOMsQuery : IRequest<Result<PagedResult<BOMDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public BOMStatus? Status { get; set; }
}

public class BOMDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid FinishedProductId { get; set; }
    public string FinishedProductName { get; set; } = string.Empty;
    public string VersionNumber { get; set; } = string.Empty;
    public BOMStatus Status { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public int ItemCount { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

internal class GetBOMsQueryHandler : IRequestHandler<GetBOMsQuery, Result<PagedResult<BOMDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetBOMsQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<PagedResult<BOMDto>>> Handle(GetBOMsQuery request, CancellationToken ct)
    {
        var query = _context.BillOfMaterials.AsQueryable();

        if (_tenant.TenantId.HasValue)
            query = query.Where(b => b.TenantId == _tenant.TenantId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(b => b.Name.ToLower().Contains(term));
        }

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BOMDto
            {
                Id = b.Id,
                Name = b.Name,
                FinishedProductId = b.FinishedProductId,
                FinishedProductName = b.FinishedProduct != null ? b.FinishedProduct.Name : string.Empty,
                VersionNumber = b.VersionNumber,
                Status = b.Status,
                Description = b.Description,
                Notes = b.Notes,
                ItemCount = b.Items.Count(),
                CreatedBy = b.CreatedBy,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(ct);

        return Result<PagedResult<BOMDto>>.Success(new PagedResult<BOMDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}
