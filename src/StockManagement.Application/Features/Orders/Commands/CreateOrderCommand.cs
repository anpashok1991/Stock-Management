using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Orders.Commands;

public class CreateOrderCommand : IRequest<Result<Guid>>
{
    public Guid CustomerId { get; set; }
    public Guid? AddressId { get; set; }
    public PaymentMode PaymentMode { get; set; }
    public string? Notes { get; set; }
    public string? CouponCode { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Discount { get; set; }
}

internal class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public CreateOrderCommandHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.TenantId ?? Guid.Empty;

        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            CustomerId = request.CustomerId,
            AddressId = request.AddressId,
            PaymentMode = request.PaymentMode,
            Notes = request.Notes,
            CouponCode = request.CouponCode,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };

        decimal subTotal = 0;
        decimal taxTotal = 0;
        decimal discountTotal = 0;

        foreach (var item in request.Items)
        {
            var lineTax = item.Quantity * item.UnitPrice * (item.TaxRate / 100m);
            var lineDiscount = item.Quantity * item.Discount;
            var lineTotal = item.Quantity * item.UnitPrice + lineTax - lineDiscount;

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                TaxAmount = lineTax,
                Discount = lineDiscount,
                LineTotal = lineTotal,
                TenantId = tenantId
            });

            subTotal += item.Quantity * item.UnitPrice;
            taxTotal += lineTax;
            discountTotal += lineDiscount;

            var product = await _context.Products.FindAsync(new object[] { item.ProductId }, ct);
            if (product != null && product.StockQuantity >= item.Quantity)
            {
                product.StockQuantity -= item.Quantity;
                product.ModifiedAt = DateTime.UtcNow;
            }
        }

        order.SubTotal = subTotal;
        order.TaxAmount = taxTotal;
        order.DiscountAmount = discountTotal;
        order.GrandTotal = subTotal + taxTotal - discountTotal;

        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(order.Id);
    }
}
