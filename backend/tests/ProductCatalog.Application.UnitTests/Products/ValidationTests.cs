using FluentAssertions;
using ProductCatalog.Application.Products.Commands.CreateProduct;
using ProductCatalog.Application.Products.Commands.UpdateProduct;
using Xunit;

namespace ProductCatalog.Application.UnitTests.Products;

public class ValidationTests
{
    private readonly CreateProductCommandValidator _createValidator = new();
    private readonly UpdateProductCommandValidator _updateValidator = new();

    [Theory]
    [InlineData("", "SKU-1", 10.0, 5)]
    [InlineData("Valid Name", "", 10.0, 5)]
    [InlineData("Valid Name", "SKU-1", 0.0, 5)]
    [InlineData("Valid Name", "SKU-1", -1.0, 5)]
    [InlineData("Valid Name", "SKU-1", 10.0, -2)]
    public void CreateProductValidator_InvalidInputs_ShouldFail(
        string name, string sku, decimal price, int stock)
    {
        var command = new CreateProductCommand(name, "Desc", price, sku, stock);
        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateProductValidator_ValidInput_ShouldSucceed()
    {
        var command = new CreateProductCommand("Valid Name", "Desc", 49.99m, "SKU-OK-123", 10);
        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateProductValidator_EmptyGuid_ShouldFail()
    {
        var command = new UpdateProductCommand(Guid.Empty, "Name", "Desc", 15m, "SKU-1", 2, true);
        var result = _updateValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductCommand.Id));
    }
}