using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Entities.Identity;
using StockManagement.Domain.Entities.Inventory;
using StockManagement.Domain.Enums;
using StockManagement.Infrastructure.Data;

namespace StockManagement.Infrastructure.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        // ---- Roles ----
        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.Roles.AnyAsync(r => r.Name == roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpper().Replace(" ", "_"),
                    Description = $"{roleName} role"
                });
            }
        }

        // ---- Tenant ----
        // Ignore query filters for seeding - tenant may not exist yet
        var tenant = context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.Name == "Default Shop");
        if (tenant == null)
        {
            tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Default Shop",
                Email = "admin@defaultshop.com",
                Phone = "+91-9876543210",
                Address = "123 Main Street, Mumbai, Maharashtra 400001",
                Currency = "INR",
                BusinessType = "Retail",
                BranchCode = "MUM-001",
                ThemePrimaryColor = "#594AE2",
                TaxRate = 18m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
        }

        // ---- Admin User ----
        var adminEmail = "admin@stockmgmt.com";
        if (!await userManager.Users.AnyAsync(u => u.Email == adminEmail))
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
                TenantId = tenant.Id,
                IsActive = true
            };

            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, Roles.SuperAdmin);
            await userManager.AddToRoleAsync(admin, Roles.ShopAdmin);
        }

        // ---- Shop Admin ----
        var shopEmail = "shop@stockmgmt.com";
        if (!await userManager.Users.AnyAsync(u => u.Email == shopEmail))
        {
            var shopAdmin = new ApplicationUser
            {
                UserName = shopEmail,
                Email = shopEmail,
                EmailConfirmed = true,
                FirstName = "Shop",
                LastName = "Manager",
                TenantId = tenant.Id,
                IsActive = true
            };
            await userManager.CreateAsync(shopAdmin, "Shop@123");
            await userManager.AddToRoleAsync(shopAdmin, Roles.ShopAdmin);
        }

        // ---- Categories ----
        // Use IgnoreQueryFilters() to bypass the global tenant filter during seeding
        if (!await context.Categories.IgnoreQueryFilters().AnyAsync(c => c.TenantId == tenant.Id))
        {
            var electronics = new Category { Name = "Electronics", TenantId = tenant.Id, DisplayOrder = 1 };
            var grocery = new Category { Name = "Grocery", TenantId = tenant.Id, DisplayOrder = 2 };
            var clothing = new Category { Name = "Clothing", TenantId = tenant.Id, DisplayOrder = 3 };
            var home = new Category { Name = "Home & Kitchen", TenantId = tenant.Id, DisplayOrder = 4 };

            context.Categories.AddRange(electronics, grocery, clothing, home);
            await context.SaveChangesAsync();
        }

        // ---- Brands ----
        if (!await context.Brands.IgnoreQueryFilters().AnyAsync(b => b.TenantId == tenant.Id))
        {
            context.Brands.AddRange(
                new Brand { Name = "Samsung", TenantId = tenant.Id },
                new Brand { Name = "Apple", TenantId = tenant.Id },
                new Brand { Name = "Nike", TenantId = tenant.Id },
                new Brand { Name = "Amul", TenantId = tenant.Id }
            );
            await context.SaveChangesAsync();
        }

        // ---- Warehouse ----
        if (!await context.Warehouses.IgnoreQueryFilters().AnyAsync(w => w.TenantId == tenant.Id))
        {
            context.Warehouses.Add(new Warehouse { Name = "Main Warehouse", Code = "WH-001", TenantId = tenant.Id });
            await context.SaveChangesAsync();
        }

        // ---- Products ----
        if (!await context.Products.IgnoreQueryFilters().AnyAsync(p => p.TenantId == tenant.Id))
        {
            var categories = await context.Categories.IgnoreQueryFilters().Where(c => c.TenantId == tenant.Id).ToListAsync();
            var warehouse = await context.Warehouses.IgnoreQueryFilters().FirstAsync(w => w.TenantId == tenant.Id);

            var products = new List<Product>
            {
                new() { Name = "Samsung Galaxy S24 Ultra", Sku = "SAM-S24U", CostPrice = 85000, SellingPrice = 129999, StockQuantity = 25, CategoryId = categories.First(c => c.Name == "Electronics").Id, TenantId = tenant.Id, Status = ProductStatus.Active, IsFeatured = true, Description = "Samsung's flagship smartphone with AI features" },
                new() { Name = "iPhone 15 Pro Max", Sku = "APL-15PM", CostPrice = 105000, SellingPrice = 159900, StockQuantity = 18, CategoryId = categories.First(c => c.Name == "Electronics").Id, TenantId = tenant.Id, Status = ProductStatus.Active, IsFeatured = true, Description = "Apple's latest titanium smartphone" },
                new() { Name = "Nike Air Max 270", Sku = "NKE-AM270", CostPrice = 8500, SellingPrice = 13999, StockQuantity = 50, CategoryId = categories.First(c => c.Name == "Clothing").Id, TenantId = tenant.Id, Status = ProductStatus.Active, Description = "Comfortable running shoes" },
                new() { Name = "Amul Butter 500g", Sku = "AML-BTR500", CostPrice = 200, SellingPrice = 280, StockQuantity = 200, CategoryId = categories.First(c => c.Name == "Grocery").Id, TenantId = tenant.Id, Status = ProductStatus.Active, Description = "Fresh Amul butter" },
                new() { Name = "Prestige Pressure Cooker 5L", Sku = "PST-PC5L", CostPrice = 2200, SellingPrice = 3499, StockQuantity = 30, CategoryId = categories.First(c => c.Name == "Home & Kitchen").Id, TenantId = tenant.Id, Status = ProductStatus.Active, Description = "Aluminium pressure cooker" },
                new() { Name = "Samsung 55\" 4K Smart TV", Sku = "SAM-TV55", CostPrice = 42000, SellingPrice = 59999, StockQuantity = 12, CategoryId = categories.First(c => c.Name == "Electronics").Id, TenantId = tenant.Id, Status = ProductStatus.Active, Description = "55 inch Crystal UHD Smart TV" },
            };

            foreach (var p in products)
            {
                p.CreatedAt = DateTime.UtcNow;
                p.TenantId = tenant.Id;
            }

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Seed stock items
            foreach (var p in products)
            {
                context.StockItems.Add(new StockItem
                {
                    ProductId = p.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = p.StockQuantity,
                    TenantId = tenant.Id
                });
            }
            await context.SaveChangesAsync();
        }

        // ---- Sample Customer ----
        if (!await context.Customers.IgnoreQueryFilters().AnyAsync(c => c.TenantId == tenant.Id))
        {
            context.Customers.Add(new Customer
            {
                FirstName = "Rajesh",
                LastName = "Kumar",
                Email = "rajesh@example.com",
                Phone = "+91-9876543211",
                TenantId = tenant.Id
            });
            await context.SaveChangesAsync();
        }
    }
}