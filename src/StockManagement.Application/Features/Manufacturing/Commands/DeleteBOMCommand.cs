using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Manufacturing.Commands;

public class DeleteBOMCommand : IRequest<Result>
{
    public Guid BOMId { get; set; }
}

internal class DeleteBOMCommandHandler : IRequestHandler<DeleteBOMCommand, Result>
{
    private readonly IAppDbContext _context;

    public DeleteBOMCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteBOMCommand request, CancellationToken ct)
    {
        var bom = await _context.BillOfMaterials
            .FirstOrDefaultAsync(b => b.Id == request.BOMId, ct);

        if (bom == null)
            return Result.Failure("Bill of Materials not found");

        var hasActiveOrders = await _context.ManufacturingOrders
            .AnyAsync(m => m.BillOfMaterialId == request.BOMId && m.Status != Domain.Enums.ManufacturingStatus.Cancelled, ct);

        if (hasActiveOrders)
            return Result.Failure("Cannot delete BOM with active manufacturing orders");

        var items = await _context.BillOfMaterialItems
            .Where(bi => bi.BillOfMaterialId == request.BOMId)
            .ToListAsync(ct);

        _context.BillOfMaterialItems.RemoveRange(items);
        _context.BillOfMaterials.Remove(bom);
        await _context.SaveChangesAsync(ct);

        return Result.Success("Bill of Materials deleted successfully");
    }
}
