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
            // Deduct raw materials from product stock (check against Product.StockQuantity)
            foreach (var item in order.Items)
            {
                var rawProduct = await _context.Products.FindAsync(new object[] { item.RawMaterialId }, ct);
                var available = rawProduct?.StockQuantity ?? 0;

                if (available < item.QuantityConsumed)
                {
                    if (item.IsOptional) continue;
                    return Result.Failure($"Insufficient stock for {item.RawMaterial?.Name ?? "raw material"}. Available in Products: {available}, Required: {item.QuantityConsumed}");
                }

                // Reduce product-level stock (this is what Products page shows)
                if (rawProduct != null)
                {
                    rawProduct.StockQuantity -= (int)item.QuantityConsumed;
                    rawProduct.ModifiedAt = DateTime.UtcNow;
                }

                // Also update StockItem if one exists (for warehouse tracking)
                var stockItem = await _context.StockItems
                    .FirstOrDefaultAsync(s => s.ProductId == item.RawMaterialId, ct);
                if (stockItem != null)
                {
                    stockItem.Quantity -= (int)item.QuantityConsumed;
                    stockItem.ModifiedAt = DateTime.UtcNow;
                }

                _context.ManufacturingTransactions.Add(new Domain.Entities.Manufacturing.ManufacturingTransaction
                {
                    ManufacturingOrderId = order.Id,
                    ProductId = item.RawMaterialId,
                    Type = ManufacturingTransactionType.ConsumedInManufacturing,
                    Quantity = (int)item.QuantityConsumed,
                    BeforeQuantity = available,
                    AfterQuantity = rawProduct?.StockQuantity ?? 0,
                    ManufacturingNumber = order.ManufacturingNumber,
                    OccurredAt = DateTime.UtcNow
                });
            }

            // Add finished product stock
            var finishedProduct = await _context.Products.FindAsync(new object[] { order.FinishedProductId }, ct);
            if (finishedProduct != null)
            {
                var beforeQty = finishedProduct.StockQuantity;
                finishedProduct.StockQuantity += order.Quantity;
                finishedProduct.ModifiedAt = DateTime.UtcNow;

                // Also update StockItem (for warehouse tracking); create if missing
                var finishedStockItem = await _context.StockItems
                    .FirstOrDefaultAsync(s => s.ProductId == order.FinishedProductId, ct);
                if (finishedStockItem != null)
                {
                    finishedStockItem.Quantity += order.Quantity;
                    finishedStockItem.ModifiedAt = DateTime.UtcNow;
                }
                else
                {
                    var whId = order.WarehouseId ?? await _context.Warehouses.Select(w => (Guid?)w.Id).FirstOrDefaultAsync(ct) ?? Guid.Empty;
                    finishedStockItem = new StockManagement.Domain.Entities.Inventory.StockItem
                    {
                        ProductId = order.FinishedProductId,
                        Quantity = order.Quantity,
                        WarehouseId = whId
                    };
                    _context.StockItems.Add(finishedStockItem);
                }

                _context.ManufacturingTransactions.Add(new Domain.Entities.Manufacturing.ManufacturingTransaction
                {
                    ManufacturingOrderId = order.Id,
                    ProductId = order.FinishedProductId,
                    Type = ManufacturingTransactionType.Manufactured,
                    Quantity = order.Quantity,
                    BeforeQuantity = beforeQty,
                    AfterQuantity = finishedProduct.StockQuantity,
                    ManufacturingNumber = order.ManufacturingNumber,
                    OccurredAt = DateTime.UtcNow
                });
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
