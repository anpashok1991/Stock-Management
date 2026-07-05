using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities.Manufacturing;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Manufacturing.Commands;

public class CreateManufacturingOrderCommand : IRequest<Result<Guid>>
{
    public Guid FinishedProductId { get; set; }
    public Guid? BillOfMaterialId { get; set; }
    public int Quantity { get; set; }
    public DateTime? ProductionDate { get; set; }
    public decimal AdditionalManufacturingCost { get; set; }
    public decimal LabourCost { get; set; }
    public decimal PackagingCost { get; set; }
    public string? Remarks { get; set; }
}

internal class CreateManufacturingOrderCommandHandler : IRequestHandler<CreateManufacturingOrderCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public CreateManufacturingOrderCommandHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateManufacturingOrderCommand request, CancellationToken ct)
    {
        // Rely on AppDbContext.CurrentTenantId and global query filters rather than explicit tenant matching

        var finishedProduct = await _context.Products.FindAsync(new object[] { request.FinishedProductId }, ct);
        if (finishedProduct == null)
            return Result<Guid>.Failure("Finished product not found");

        BillOfMaterial? bom = null;
        if (request.BillOfMaterialId.HasValue)
        {
            bom = await _context.BillOfMaterials
                .Include(b => b.Items).ThenInclude(bi => bi.RawMaterial)
                .FirstOrDefaultAsync(b => b.Id == request.BillOfMaterialId.Value, ct);

            if (bom == null)
                return Result<Guid>.Failure("Bill of Materials not found");
        }

        var manufacturingNumber = $"MFG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var order = new ManufacturingOrder
        {
            ManufacturingNumber = manufacturingNumber,
            FinishedProductId = request.FinishedProductId,
            BillOfMaterialId = request.BillOfMaterialId,
            Quantity = request.Quantity,
            ProductionDate = request.ProductionDate ?? DateTime.UtcNow,
            AdditionalManufacturingCost = request.AdditionalManufacturingCost,
            LabourCost = request.LabourCost,
            PackagingCost = request.PackagingCost,
            Remarks = request.Remarks,
            Status = ManufacturingStatus.Draft,
            CreatedBy = _tenant.TenantName,
            CreatedAt = DateTime.UtcNow
        };

        decimal totalMaterialCost = 0;

        if (bom != null)
        {
            foreach (var bomItem in bom.Items)
            {
                var quantityRequired = bomItem.QuantityRequired * request.Quantity;
                var wastageQty = quantityRequired * (bomItem.WastagePercentage / 100m);
                quantityRequired += wastageQty;

                var stockItem = await _context.StockItems
                    .FirstOrDefaultAsync(s => s.ProductId == bomItem.RawMaterialId, ct);

                var availableStock = stockItem?.Quantity ?? 0;
                var unitCost = bomItem.RawMaterial?.CostPrice ?? 0;
                var itemTotalCost = quantityRequired * unitCost;
                totalMaterialCost += itemTotalCost;

                order.Items.Add(new ManufacturingOrderItem
                {
                    RawMaterialId = bomItem.RawMaterialId,
                    QuantityRequired = quantityRequired,
                    QuantityConsumed = quantityRequired,
                    Unit = bomItem.Unit,
                    WastagePercentage = bomItem.WastagePercentage,
                    UnitCost = unitCost,
                    TotalCost = itemTotalCost,
                    AvailableStock = availableStock,
                    IsOptional = bomItem.IsOptional,
                    Remarks = bomItem.Remarks
                });
            }
        }

        order.TotalMaterialCost = totalMaterialCost;
        order.EstimatedProductCost = totalMaterialCost + request.AdditionalManufacturingCost + request.LabourCost + request.PackagingCost;

        await _context.ManufacturingOrders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(order.Id);
    }
}
