using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Manufacturing.Queries;

public class GetManufacturingOrdersQuery : IRequest<Result<PagedResult<ManufacturingOrderDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public ManufacturingStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class ManufacturingOrderDto
{
    public Guid Id { get; set; }
    public string ManufacturingNumber { get; set; } = string.Empty;
    public Guid FinishedProductId { get; set; }
    public string FinishedProductName { get; set; } = string.Empty;
    public string? BOMName { get; set; }
    public int Quantity { get; set; }
    public ManufacturingStatus Status { get; set; }
    public DateTime ProductionDate { get; set; }
    public decimal TotalMaterialCost { get; set; }
    public decimal AdditionalManufacturingCost { get; set; }
    public decimal LabourCost { get; set; }
    public decimal PackagingCost { get; set; }
    public decimal EstimatedProductCost { get; set; }
    public int ItemCount { get; set; }
    public string? CreatedBy { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

internal class GetManufacturingOrdersQueryHandler : IRequestHandler<GetManufacturingOrdersQuery, Result<PagedResult<ManufacturingOrderDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetManufacturingOrdersQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<PagedResult<ManufacturingOrderDto>>> Handle(GetManufacturingOrdersQuery request, CancellationToken ct)
    {
        var query = _context.ManufacturingOrders.AsQueryable();

        if (_tenant.TenantId.HasValue)
            query = query.Where(m => m.TenantId == _tenant.TenantId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(m => m.ManufacturingNumber.ToLower().Contains(term));
        }

        if (request.Status.HasValue)
            query = query.Where(m => m.Status == request.Status.Value);

        if (request.FromDate.HasValue)
            query = query.Where(m => m.ProductionDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(m => m.ProductionDate <= request.ToDate.Value.AddDays(1));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new ManufacturingOrderDto
            {
                Id = m.Id,
                ManufacturingNumber = m.ManufacturingNumber,
                FinishedProductId = m.FinishedProductId,
                FinishedProductName = m.FinishedProduct != null ? m.FinishedProduct.Name : string.Empty,
                BOMName = m.BillOfMaterial != null ? m.BillOfMaterial.Name : null,
                Quantity = m.Quantity,
                Status = m.Status,
                ProductionDate = m.ProductionDate,
                TotalMaterialCost = m.TotalMaterialCost,
                AdditionalManufacturingCost = m.AdditionalManufacturingCost,
                LabourCost = m.LabourCost,
                PackagingCost = m.PackagingCost,
                EstimatedProductCost = m.EstimatedProductCost,
                ItemCount = m.Items.Count(),
                CreatedBy = m.CreatedBy,
                Remarks = m.Remarks,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(ct);

        return Result<PagedResult<ManufacturingOrderDto>>.Success(new PagedResult<ManufacturingOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}
