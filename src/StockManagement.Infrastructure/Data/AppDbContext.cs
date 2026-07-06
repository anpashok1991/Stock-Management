using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StockManagement.Domain.Common;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Entities.Identity;
using StockManagement.Domain.Entities.Inventory;
using StockManagement.Domain.Entities.Manufacturing;

namespace StockManagement.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IAppDbContext
{
    private readonly ITenantContext? _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AppDbContext> _logger;

    // Mutable per-request tenant id used by query filters. Set this from middleware each HTTP request.
    // Use Guid.Empty to indicate no tenant selected.
    public Guid CurrentTenantId { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext? tenantContext, IHttpContextAccessor httpContextAccessor, ILogger<AppDbContext> logger)
        : base(options)
    {
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        CurrentTenantId = tenantContext?.TenantId ?? Guid.Empty;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductAttributeDefinition> ProductAttributeDefinitions => Set<ProductAttributeDefinition>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<BillOfMaterial> BillOfMaterials => Set<BillOfMaterial>();
    public DbSet<BillOfMaterialItem> BillOfMaterialItems => Set<BillOfMaterialItem>();
    public DbSet<ManufacturingOrder> ManufacturingOrders => Set<ManufacturingOrder>();
    public DbSet<ManufacturingOrderItem> ManufacturingOrderItems => Set<ManufacturingOrderItem>();
    public DbSet<ManufacturingTransaction> ManufacturingTransactions => Set<ManufacturingTransaction>();
    public DbSet<ManufacturingCost> ManufacturingCosts => Set<ManufacturingCost>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ---- Identity tables ----
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.Id).HasMaxLength(128);
            e.HasIndex(u => u.TenantId);
        });

        // ---- Tenant ----
        builder.Entity<Tenant>(e =>
        {
            e.HasIndex(t => t.Name).IsUnique();
            e.Property(t => t.Name).HasMaxLength(200);
        });

        // ---- Category (self-referencing) ----
        builder.Entity<Category>(e =>
        {
            e.HasIndex(c => new { c.TenantId, c.Name });
            e.HasOne(c => c.Parent).WithMany(c => c.Children).HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Brand ----
        builder.Entity<Brand>(e =>
        {
            e.HasIndex(b => new { b.TenantId, b.Name });
        });

        // ---- Supplier ----
        builder.Entity<Supplier>(e =>
        {
            e.HasIndex(s => new { s.TenantId, s.Name });
        });

        // ---- Product ----
        builder.Entity<Product>(e =>
        {
            e.HasIndex(p => new { p.TenantId, p.Sku }).IsUnique();
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
            e.HasOne(p => p.Brand).WithMany(b => b.Products).HasForeignKey(p => p.BrandId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(p => p.Supplier).WithMany(s => s.Products).HasForeignKey(p => p.SupplierId).OnDelete(DeleteBehavior.SetNull);
            e.Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
            e.Property(p => p.SellingPrice).HasColumnType("decimal(18,2)");
        });

        // ---- ProductAttributeDefinition / Value ----
        builder.Entity<ProductAttributeDefinition>(e =>
        {
            e.HasIndex(a => new { a.TenantId, a.Name });
        });

        builder.Entity<ProductAttributeValue>(e =>
        {
            e.HasOne(v => v.Product).WithMany(p => p.AttributeValues).HasForeignKey(v => v.ProductId);
            e.HasOne(v => v.AttributeDefinition).WithMany(a => a.Values).HasForeignKey(v => v.AttributeDefinitionId);
        });

        // ---- Warehouse ----
        builder.Entity<Warehouse>(e =>
        {
            e.HasIndex(w => new { w.TenantId, w.Name });
        });

        // ---- StockItem ----
        builder.Entity<StockItem>(e =>
        {
            e.HasIndex(s => new { s.TenantId, s.ProductId, s.WarehouseId }).IsUnique();
            e.HasOne(s => s.Product).WithMany(p => p.StockItems).HasForeignKey(s => s.ProductId);
            e.HasOne(s => s.Warehouse).WithMany(w => w.StockItems).HasForeignKey(s => s.WarehouseId);
        });

        // ---- StockMovement ----
        builder.Entity<StockMovement>(e =>
        {
            e.HasOne(m => m.Product).WithMany().HasForeignKey(m => m.ProductId);
            e.HasOne(m => m.Warehouse).WithMany().HasForeignKey(m => m.WarehouseId);
        });

        // ---- Customer ----
        builder.Entity<Customer>(e =>
        {
            e.HasIndex(c => new { c.TenantId, c.Email });
        });

        // ---- Address ----
        builder.Entity<Address>(e =>
        {
            e.HasOne(a => a.Customer).WithMany(c => c.Addresses).HasForeignKey(a => a.CustomerId);
        });

        // ---- Cart / CartItem ----
        builder.Entity<Cart>(e =>
        {
            e.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CartItem>(e =>
        {
            e.HasOne(ci => ci.Cart).WithMany(c => c.Items).HasForeignKey(ci => ci.CartId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ci => ci.Product).WithMany(p => p.CartItems).HasForeignKey(ci => ci.ProductId);
        });

        // ---- Order / OrderItem ----
        builder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.OrderNumber).IsUnique();
            e.HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId);
            e.HasOne(o => o.Address).WithMany().HasForeignKey(o => o.AddressId).OnDelete(DeleteBehavior.SetNull);
            e.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(o => o.TaxAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.ShippingCharges).HasColumnType("decimal(18,2)");
            e.Property(o => o.GrandTotal).HasColumnType("decimal(18,2)");
        });

        builder.Entity<OrderItem>(e =>
        {
            e.HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId);
            e.HasOne(oi => oi.Product).WithMany(p => p.OrderItems).HasForeignKey(oi => oi.ProductId);
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(oi => oi.TaxAmount).HasColumnType("decimal(18,2)");
            e.Property(oi => oi.Discount).HasColumnType("decimal(18,2)");
            e.Property(oi => oi.LineTotal).HasColumnType("decimal(18,2)");
        });

        // ---- Payment ----
        builder.Entity<Payment>(e =>
        {
            e.HasOne(p => p.Order).WithMany(o => o.Payments).HasForeignKey(p => p.OrderId);
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        });

        // ---- WishlistItem ----
        builder.Entity<WishlistItem>(e =>
        {
            e.HasIndex(w => new { w.CustomerId, w.ProductId }).IsUnique();
            e.HasOne(w => w.Customer).WithMany().HasForeignKey(w => w.CustomerId);
            e.HasOne(w => w.Product).WithMany().HasForeignKey(w => w.ProductId);
        });

        // ---- Review ----
        builder.Entity<Review>(e =>
        {
            e.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId);
            e.HasOne(r => r.Customer).WithMany(c => c.Reviews).HasForeignKey(r => r.CustomerId);
        });

        // ---- Coupon ----
        builder.Entity<Coupon>(e =>
        {
            e.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();
        });

        // ---- Notification ----
        builder.Entity<Notification>(e =>
        {
            e.HasIndex(n => new { n.UserId, n.IsRead });
        });

        // ---- AuditLog ----
        builder.Entity<AuditLog>(e =>
        {
            e.HasIndex(a => new { a.TenantId, a.Timestamp });
            e.Property(a => a.OldValues).HasMaxLength(4000);
            e.Property(a => a.NewValues).HasMaxLength(4000);
        });

        // ---- BillOfMaterial ----
        builder.Entity<BillOfMaterial>(e =>
        {
            e.HasIndex(b => new { b.TenantId, b.Name });
            e.HasOne(b => b.FinishedProduct).WithMany().HasForeignKey(b => b.FinishedProductId);
        });

        // ---- BillOfMaterialItem ----
        builder.Entity<BillOfMaterialItem>(e =>
        {
            e.HasOne(bi => bi.BillOfMaterial).WithMany(b => b.Items).HasForeignKey(bi => bi.BillOfMaterialId);
            e.HasOne(bi => bi.RawMaterial).WithMany().HasForeignKey(bi => bi.RawMaterialId);
            e.Property(bi => bi.QuantityRequired).HasColumnType("decimal(18,2)");
            e.Property(bi => bi.WastagePercentage).HasColumnType("decimal(18,2)");
        });

        // ---- ManufacturingOrder ----
        builder.Entity<ManufacturingOrder>(e =>
        {
            e.HasIndex(m => m.ManufacturingNumber).IsUnique();
            e.HasOne(m => m.FinishedProduct).WithMany().HasForeignKey(m => m.FinishedProductId);
            e.HasOne(m => m.BillOfMaterial).WithMany().HasForeignKey(m => m.BillOfMaterialId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(m => m.Warehouse).WithMany().HasForeignKey(m => m.WarehouseId).OnDelete(DeleteBehavior.SetNull);
            e.Property(m => m.TotalMaterialCost).HasColumnType("decimal(18,2)");
            e.Property(m => m.AdditionalManufacturingCost).HasColumnType("decimal(18,2)");
            e.Property(m => m.LabourCost).HasColumnType("decimal(18,2)");
            e.Property(m => m.PackagingCost).HasColumnType("decimal(18,2)");
            e.Property(m => m.EstimatedProductCost).HasColumnType("decimal(18,2)");
        });

        // ---- ManufacturingOrderItem ----
        builder.Entity<ManufacturingOrderItem>(e =>
        {
            e.HasOne(mi => mi.ManufacturingOrder).WithMany(m => m.Items).HasForeignKey(mi => mi.ManufacturingOrderId);
            e.HasOne(mi => mi.RawMaterial).WithMany().HasForeignKey(mi => mi.RawMaterialId);
            e.Property(mi => mi.QuantityRequired).HasColumnType("decimal(18,2)");
            e.Property(mi => mi.QuantityConsumed).HasColumnType("decimal(18,2)");
            e.Property(mi => mi.UnitCost).HasColumnType("decimal(18,2)");
            e.Property(mi => mi.TotalCost).HasColumnType("decimal(18,2)");
        });

        // ---- ManufacturingTransaction ----
        builder.Entity<ManufacturingTransaction>(e =>
        {
            e.HasOne(mt => mt.ManufacturingOrder).WithMany().HasForeignKey(mt => mt.ManufacturingOrderId);
            e.HasOne(mt => mt.Product).WithMany().HasForeignKey(mt => mt.ProductId);
        });

        // ---- ManufacturingCost ----
        builder.Entity<ManufacturingCost>(e =>
        {
            e.HasOne(mc => mc.ManufacturingOrder).WithMany().HasForeignKey(mc => mc.ManufacturingOrderId);
            e.Property(mc => mc.RawMaterialCost).HasColumnType("decimal(18,2)");
            e.Property(mc => mc.AdditionalManufacturingCost).HasColumnType("decimal(18,2)");
            e.Property(mc => mc.LabourCost).HasColumnType("decimal(18,2)");
            e.Property(mc => mc.PackagingCost).HasColumnType("decimal(18,2)");
            e.Property(mc => mc.TotalCost).HasColumnType("decimal(18,2)");
            e.Property(mc => mc.CostPerUnit).HasColumnType("decimal(18,2)");
        });

        // ---- Apply global tenant filters ----
        ApplyTenantFilters(builder);
    }

    private void ApplyTenantFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (typeof(TenantEntity).IsAssignableFrom(clrType))
            {
                // Build expression: e => e.TenantId == this.CurrentTenantId
                var parameter = System.Linq.Expressions.Expression.Parameter(clrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(TenantEntity.TenantId));
                var currentTenantProperty = System.Linq.Expressions.Expression.Property(System.Linq.Expressions.Expression.Constant(this), nameof(CurrentTenantId));
                var comparison = System.Linq.Expressions.Expression.Equal(property, currentTenantProperty);
                var lambda = System.Linq.Expressions.Expression.Lambda(comparison, parameter);

                builder.Entity(clrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Log current tenant and user for diagnostics
        try
        {
            var tenantVal = CurrentTenantId;
            var user = _httpContextAccessor?.HttpContext?.User;
            var userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? user?.Identity?.Name ?? "(anonymous)";
            _logger?.LogInformation("SaveChanges starting. TenantId={TenantId} User={User}", tenantVal, userId);
        }
        catch (Exception ex)
        {
            try { _logger?.LogWarning(ex, "Failed to write debug tenant info"); } catch { }
        }
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                if (CurrentTenantId != Guid.Empty)
                {
                    entry.Entity.TenantId = CurrentTenantId;
                }
                else if (_tenantContext?.TenantId != null)
                {
                    entry.Entity.TenantId = _tenantContext.TenantId.Value;
                }
                // else leave as Guid.Empty (seeding or explicit set)
            }
        }

        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            try { _logger?.LogInformation("SaveChanges completed. TenantId={TenantId} Entries={Entries}", CurrentTenantId, ChangeTracker.Entries().Count()); } catch { }
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SaveChanges failed. TenantId={TenantId}", CurrentTenantId);
            throw;
        }
    }
}
