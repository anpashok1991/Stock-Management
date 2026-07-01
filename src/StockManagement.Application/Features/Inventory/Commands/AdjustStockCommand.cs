using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Entities.Inventory;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Inventory.Commands;

public class AdjustStockCommand : IRequest<Result>
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

internal class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public AdjustStockCommandHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.TenantId ?? Guid.Empty;

        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.ProductId == request.ProductId && s.WarehouseId == request.WarehouseId && s.TenantId == tenantId, ct);

        if (stockItem == null)
        {
            stockItem = new StockItem
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                Quantity = 0,
                TenantId = tenantId
            };
            await _context.StockItems.AddAsync(stockItem, ct);
        }

        int delta = request.Type switch
        {
            StockMovementType.PurchaseIn or StockMovementType.ReturnIn or StockMovementType.TransferIn or StockMovementType.OpeningStock => request.Quantity,
            StockMovementType.SaleOut or StockMovementType.Damaged or StockMovementType.TransferOut => -request.Quantity,
            StockMovementType.Adjustment => request.Quantity,
            _ => request.Quantity
        };

        stockItem.Quantity += delta;
        stockItem.ModifiedAt = DateTime.UtcNow;

        var product = await _context.Products.FindAsync(new object[] { request.ProductId }, ct);
        if (product != null)
        {
            product.StockQuantity = stockItem.Quantity;
            product.ModifiedAt = DateTime.UtcNow;
        }

        var movement = new StockMovement
        {
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            Type = request.Type,
            Quantity = Math.Abs(delta),
            Reference = request.Reference,
            Notes = request.Notes,
            OccurredAt = DateTime.UtcNow,
            TenantId = tenantId
        };

        await _context.StockMovements.AddAsync(movement, ct);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
