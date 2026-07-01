using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Common;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Entities.Identity;
using StockManagement.Domain.Entities.Inventory;

namespace StockManagement.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core DbContext for testability and clean architecture.
/// </summary>
public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductAttributeDefinition> ProductAttributeDefinitions { get; }
    DbSet<ProductAttributeValue> ProductAttributeValues { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<StockItem> StockItems { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<WishlistItem> WishlistItems { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
