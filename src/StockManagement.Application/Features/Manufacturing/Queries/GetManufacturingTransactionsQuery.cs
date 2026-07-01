using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Manufacturing.Queries;

public class GetManufacturingTransactionsQuery : IRequest<Result<PagedResult<ManufacturingTransactionDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ManufacturingOrderId { get; set; }
    public ManufacturingTransactionType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class ManufacturingTransactionDto
{
    public Guid Id { get; set; }
    public string ManufacturingNumber { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public ManufacturingTransactionType Type { get; set; }
    public int Quantity { get; set; }
    public int BeforeQuantity { get; set; }
    public int AfterQuantity { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? Remarks { get; set; }
}

internal class GetManufacturingTransactionsQueryHandler : IRequestHandler<GetManufacturingTransactionsQuery, Result<PagedResult<ManufacturingTransactionDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetManufacturingTransactionsQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<PagedResult<ManufacturingTransactionDto>>> Handle(GetManufacturingTransactionsQuery request, CancellationToken ct)
    {
        var query = _context.ManufacturingTransactions.AsQueryable();

        if (_tenant.TenantId.HasValue)
            query = query.Where(t => t.TenantId == _tenant.TenantId.Value);

        if (request.ManufacturingOrderId.HasValue)
            query = query.Where(t => t.ManufacturingOrderId == request.ManufacturingOrderId.Value);

        if (request.Type.HasValue)
            query = query.Where(t => t.Type == request.Type.Value);

        if (request.FromDate.HasValue)
            query = query.Where(t => t.OccurredAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(t => t.OccurredAt <= request.ToDate.Value.AddDays(1));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.OccurredAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new ManufacturingTransactionDto
            {
                Id = t.Id,
                ManufacturingNumber = t.ManufacturingNumber ?? string.Empty,
                ProductId = t.ProductId,
                ProductName = t.Product != null ? t.Product.Name : string.Empty,
                Type = t.Type,
                Quantity = t.Quantity,
                BeforeQuantity = t.BeforeQuantity,
                AfterQuantity = t.AfterQuantity,
                OccurredAt = t.OccurredAt,
                Remarks = t.Remarks
            })
            .ToListAsync(ct);

        return Result<PagedResult<ManufacturingTransactionDto>>.Success(new PagedResult<ManufacturingTransactionDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}
