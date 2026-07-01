using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Manufacturing.Commands;

public class DeleteManufacturingOrderCommand : IRequest<Result>
{
    public Guid ManufacturingOrderId { get; set; }
}

internal class DeleteManufacturingOrderCommandHandler : IRequestHandler<DeleteManufacturingOrderCommand, Result>
{
    private readonly IAppDbContext _context;

    public DeleteManufacturingOrderCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteManufacturingOrderCommand request, CancellationToken ct)
    {
        var order = await _context.ManufacturingOrders
            .FirstOrDefaultAsync(m => m.Id == request.ManufacturingOrderId, ct);

        if (order == null)
            return Result.Failure("Manufacturing order not found");

        if (order.Status == Domain.Enums.ManufacturingStatus.Completed)
            return Result.Failure("Cannot delete a completed manufacturing order");

        var items = await _context.ManufacturingOrderItems
            .Where(mi => mi.ManufacturingOrderId == request.ManufacturingOrderId)
            .ToListAsync(ct);

        _context.ManufacturingOrderItems.RemoveRange(items);
        _context.ManufacturingOrders.Remove(order);
        await _context.SaveChangesAsync(ct);

        return Result.Success("Manufacturing order deleted successfully");
    }
}
