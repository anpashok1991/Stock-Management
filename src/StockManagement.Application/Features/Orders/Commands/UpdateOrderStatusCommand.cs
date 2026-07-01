using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Orders.Commands;

public class UpdateOrderStatusCommand : IRequest<Result>
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? TrackingNumber { get; set; }
}

internal class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly IAppDbContext _context;

    public UpdateOrderStatusCommandHandler(IAppDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken ct)
    {
        var order = await _context.Orders.FindAsync(new object[] { request.OrderId }, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Order), request.OrderId);

        order.Status = request.Status;
        if (request.TrackingNumber != null) order.TrackingNumber = request.TrackingNumber;
        order.ModifiedAt = DateTime.UtcNow;

        switch (request.Status)
        {
            case OrderStatus.Shipped:
                order.ShippedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
                order.DeliveredAt = DateTime.UtcNow;
                break;
            case OrderStatus.Cancelled:
            case OrderStatus.Refunded:
                foreach (var item in order.Items)
                {
                    var product = await _context.Products.FindAsync(new object[] { item.ProductId }, ct);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        product.ModifiedAt = DateTime.UtcNow;
                    }
                }
                break;
        }

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
