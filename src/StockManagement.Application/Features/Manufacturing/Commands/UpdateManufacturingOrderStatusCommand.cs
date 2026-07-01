using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Manufacturing.Commands;

public class UpdateManufacturingOrderStatusCommand : IRequest<Result>
{
    public Guid ManufacturingOrderId { get; set; }
    public ManufacturingStatus Status { get; set; }
    public string? Remarks { get; set; }
}

internal class UpdateManufacturingOrderStatusCommandHandler : IRequestHandler<UpdateManufacturingOrderStatusCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public UpdateManufacturingOrderStatusCommandHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result> Handle(UpdateManufacturingOrderStatusCommand request, CancellationToken ct)
    {
        var order = await _context.ManufacturingOrders
            .Include(m => m.Items).ThenInclude(mi => mi.RawMaterial)
            .FirstOrDefaultAsync(m => m.Id == request.ManufacturingOrderId, ct);

        if (order == null)
            return Result.Failure("Manufacturing order not found");

        if (order.Status == ManufacturingStatus.Completed)
            return Result.Failure("Manufacturing order is already completed");

        if (order.Status == ManufacturingStatus.Cancelled)
            return Result.Failure("Manufacturing order is cancelled");

        if (request.Status == ManufacturingStatus.InProgress && order.Status != ManufacturingStatus.Planned && order.Status != ManufacturingStatus.Draft)
            return Result.Failure("Manufacturing order must be in Draft or Planned status to start production");

        if (request.Status == ManufacturingStatus.Completed)
        {
            var tenantId = _tenant.TenantId ?? Guid.Empty;

            foreach (var item in order.Items)
            {
                var stockItem = await _context.StockItems
                    .FirstOrDefaultAsync(s => s.ProductId == item.RawMaterialId && s.TenantId == tenantId, ct);

                if (stockItem == null || stockItem.Quantity < item.QuantityConsumed)
                {
                    if (item.IsOptional) continue;
                    return Result.Failure($"Insufficient stock for {item.RawMaterial?.Name ?? "raw material"}. Available: {stockItem?.Quantity ?? 0}, Required: {item.QuantityConsumed}");
                }

                var beforeQty = stockItem.Quantity;
                stockItem.Quantity -= (int)item.QuantityConsumed;
                stockItem.ModifiedAt = DateTime.UtcNow;

                _context.ManufacturingTransactions.Add(new Domain.Entities.Manufacturing.ManufacturingTransaction
                {
                    ManufacturingOrderId = order.Id,
                    ProductId = item.RawMaterialId,
                    Type = ManufacturingTransactionType.ConsumedInManufacturing,
                    Quantity = (int)item.QuantityConsumed,
                    BeforeQuantity = beforeQty,
                    AfterQuantity = stockItem.Quantity,
                    ManufacturingNumber = order.ManufacturingNumber,
                    TenantId = tenantId,
                    OccurredAt = DateTime.UtcNow
                });
            }

            var finishedStockItem = await _context.StockItems
                .FirstOrDefaultAsync(s => s.ProductId == order.FinishedProductId && s.TenantId == tenantId, ct);

            if (finishedStockItem != null)
            {
                var beforeQty = finishedStockItem.Quantity;
                finishedStockItem.Quantity += order.Quantity;
                finishedStockItem.ModifiedAt = DateTime.UtcNow;

                _context.ManufacturingTransactions.Add(new Domain.Entities.Manufacturing.ManufacturingTransaction
                {
                    ManufacturingOrderId = order.Id,
                    ProductId = order.FinishedProductId,
                    Type = ManufacturingTransactionType.Manufactured,
                    Quantity = order.Quantity,
                    BeforeQuantity = beforeQty,
                    AfterQuantity = finishedStockItem.Quantity,
                    ManufacturingNumber = order.ManufacturingNumber,
                    TenantId = tenantId,
                    OccurredAt = DateTime.UtcNow
                });
            }

            var finishedProduct = await _context.Products.FindAsync(new object[] { order.FinishedProductId }, ct);
            if (finishedProduct != null)
            {
                finishedProduct.StockQuantity += order.Quantity;
                finishedProduct.ModifiedAt = DateTime.UtcNow;
            }
        }

        order.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Remarks))
            order.Remarks = request.Remarks;
        order.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Result.Success($"Manufacturing order status updated to {request.Status}");
    }
}
