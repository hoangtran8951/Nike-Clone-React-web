using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Products.Commands.UpdateProduct;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Infrastructure.Persistence;
using Xunit;

namespace ProductCatalog.Application.UnitTests.Products;

public class UpdateProductCommandHandlerTests
{
    private ProductDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ProductDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldUpdateAllFields()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var product = Product.Create("Old Name", "Old Desc", 20m, "OLD-SKU", 5);
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var handler = new UpdateProductCommandHandler(context);
        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: "New Name",
            Description: "New Desc",
            Price: 29.99m,
            Sku: "NEW-SKU",
            StockQuantity: 12,
            IsActive: false);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await context.Products.FindAsync(product.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("New Name");
        updated.Description.Should().Be("New Desc");
        updated.Price.Should().Be(29.99m);
        updated.Sku.Should().Be("NEW-SKU");
        updated.StockQuantity.Should().Be(12);
        updated.IsActive.Should().BeFalse();
        updated.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NonExistentProduct_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new UpdateProductCommandHandler(context);
        var command = new UpdateProductCommand(
            Id: Guid.NewGuid(),
            Name: "Doesn't Matter",
            Description: "Desc",
            Price: 10m,
            Sku: "SKU-404",
            StockQuantity: 1,
            IsActive: true);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_DuplicateSkuOnDifferentProduct_ShouldThrowDomainValidationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var product1 = Product.Create("Product 1", "Desc 1", 10m, "SKU-A", 1);
        var product2 = Product.Create("Product 2", "Desc 2", 20m, "SKU-B", 2);
        await context.Products.AddRangeAsync(product1, product2);
        await context.SaveChangesAsync();

        var handler = new UpdateProductCommandHandler(context);
        var command = new UpdateProductCommand(
            Id: product2.Id,
            Name: "Product 2 Renamed",
            Description: "Desc 2",
            Price: 20m,
            Sku: "SKU-A", // Attempting to use product 1's SKU
            StockQuantity: 2,
            IsActive: true);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("*already has the SKU*");
    }
}