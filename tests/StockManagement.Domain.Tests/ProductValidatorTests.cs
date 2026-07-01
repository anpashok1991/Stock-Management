using FluentValidation.TestHelper;
using StockManagement.Application.Features.Products.Commands;
using StockManagement.Application.Features.Products.Validators;

namespace StockManagement.Domain.Tests;

public class ProductValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateProductCommand { Name = "", Sku = "SKU-001", SellingPrice = 100, CategoryId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Sku_Is_Empty()
    {
        var command = new CreateProductCommand { Name = "Product", Sku = "", SellingPrice = 100, CategoryId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Sku);
    }

    [Fact]
    public void Should_Have_Error_When_SellingPrice_Is_Zero()
    {
        var command = new CreateProductCommand { Name = "Product", Sku = "SKU-001", SellingPrice = 0, CategoryId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.SellingPrice);
    }

    [Fact]
    public void Should_Have_Error_When_Category_Is_Empty()
    {
        var command = new CreateProductCommand { Name = "Product", Sku = "SKU-001", SellingPrice = 100, CategoryId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "SKU-001",
            SellingPrice = 99.99m,
            CostPrice = 50m,
            TaxRate = 18,
            CategoryId = Guid.NewGuid()
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
