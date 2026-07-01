using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Dashboard.Queries;

public class GetDashboardStatsQuery : IRequest<Result<DashboardDto>>
{
}

public class DashboardDto
{
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TodayOrders { get; set; }
    public int LowStockProducts { get; set; }
    public int PendingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}

public class RecentOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TopProductDto
{
    public string Name { get; set; } = string.Empty;
    public int Sold { get; set; }
    public decimal Revenue { get; set; }
}

internal class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardDto>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetDashboardStatsQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<DashboardDto>> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var ordersQuery = _context.Orders.AsQueryable();
        var productsQuery = _context.Products.Where(p => !p.IsDeleted).AsQueryable();

        if (_tenant.TenantId.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.TenantId == _tenant.TenantId.Value);
            productsQuery = productsQuery.Where(p => p.TenantId == _tenant.TenantId.Value);
        }

        var totalOrders = await ordersQuery.CountAsync(ct);
        var todayOrders = await ordersQuery.CountAsync(o => o.CreatedAt >= today, ct);
        var pendingOrders = await ordersQuery.CountAsync(o => o.Status == Domain.Enums.OrderStatus.Pending || o.Status == Domain.Enums.OrderStatus.Confirmed, ct);
        var deliveredOrders = await ordersQuery.CountAsync(o => o.Status == Domain.Enums.OrderStatus.Delivered, ct);
        var totalRevenue = await ordersQuery.SumAsync(o => o.GrandTotal, ct);
        var todayRevenue = await ordersQuery.Where(o => o.CreatedAt >= today).SumAsync(o => o.GrandTotal, ct);
        var monthlyRevenue = await ordersQuery.Where(o => o.CreatedAt >= monthStart).SumAsync(o => o.GrandTotal, ct);

        var dashboard = new DashboardDto
        {
            TotalProducts = await productsQuery.CountAsync(ct),
            TotalCustomers = await _context.Customers.CountAsync(ct),
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,
            TodayOrders = todayOrders,
            LowStockProducts = await productsQuery.CountAsync(p => p.StockQuantity <= p.ReorderLevel, ct),
            PendingOrders = pendingOrders,
            DeliveredOrders = deliveredOrders,
            MonthlyRevenue = monthlyRevenue,
            RecentOrders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentOrderDto
                {
                    OrderNumber = o.OrderNumber,
                    GrandTotal = o.GrandTotal,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync(ct),
            TopProducts = await productsQuery
                .OrderByDescending(p => p.StockQuantity)
                .Take(5)
                .Select(p => new TopProductDto
                {
                    Name = p.Name,
                    Sold = p.OrderItems.Count,
                    Revenue = p.OrderItems.Sum(oi => oi.LineTotal)
                })
                .ToListAsync(ct)
        };

        return Result<DashboardDto>.Success(dashboard);
    }
}
