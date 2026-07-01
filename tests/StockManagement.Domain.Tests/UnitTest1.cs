using StockManagement.Domain.Common;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Enums;

namespace StockManagement.Domain.Tests;

public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_HasGuidId()
    {
        var entity = new Tenant { Name = "Test" };
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void BaseEntity_HasCreatedAtUtc()
    {
        var before = DateTime.UtcNow;
        var entity = new Tenant { Name = "Test" };
        var after = DateTime.UtcNow;

        Assert.InRange(entity.CreatedAt, before, after);
    }

    [Fact]
    public void TenantEntity_HasTenantId()
    {
        var tenantId = Guid.NewGuid();
        var product = new Product
        {
            Name = "Test Product",
            TenantId = tenantId,
            Sku = "TEST-001",
            CategoryId = Guid.NewGuid()
        };

        Assert.Equal(tenantId, product.TenantId);
    }
}

public class ProductTests
{
    [Fact]
    public void Product_DefaultStatusIsActive()
    {
        var product = new Product
        {
            Name = "Test",
            Sku = "T-001",
            TenantId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid()
        };

        Assert.Equal(ProductStatus.Active, product.Status);
    }

    [Fact]
    public void Product_StockQuantityCanBeSet()
    {
        var product = new Product
        {
            Name = "Test",
            Sku = "T-001",
            TenantId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            StockQuantity = 100
        };

        Assert.Equal(100, product.StockQuantity);
    }
}

public class OrderTests
{
    [Fact]
    public void Order_DefaultStatusIsPending()
    {
        var order = new Order
        {
            OrderNumber = "ORD-001",
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Order_DefaultPaymentStatusIsPending()
    {
        var order = new Order
        {
            OrderNumber = "ORD-001",
            CustomerId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);
    }
}

public class RolesTests
{
    [Fact]
    public void Roles_AllContainsExpectedRoles()
    {
        Assert.Contains(Roles.SuperAdmin, Roles.All);
        Assert.Contains(Roles.ShopAdmin, Roles.All);
        Assert.Contains(Roles.Customer, Roles.All);
        Assert.Contains(Roles.Cashier, Roles.All);
        Assert.Equal(10, Roles.All.Length);
    }
}

public class ResultTests
{
    [Fact]
    public void Result_SuccessReturnsSuccess()
    {
        var result = StockManagement.Application.Common.Models.Result<int>.Success(42);
        Assert.True(result.Succeeded);
        Assert.Equal(42, result.Data);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Result_FailureReturnsErrors()
    {
        var result = StockManagement.Application.Common.Models.Result.Failure("Something went wrong");
        Assert.False(result.Succeeded);
        Assert.Single(result.Errors);
        Assert.Equal("Something went wrong", result.Errors[0]);
    }
}
