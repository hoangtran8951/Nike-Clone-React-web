using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Products.Commands.CreateProduct;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Infrastructure.Persistence;
using Xunit;

namespace ProductCatalog.Application.UnitTests.Products;

public class CreateProductCommandHandlerTests
{
    private ProductDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ProductDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateProductAndReturnGuid()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CreateProductCommandHandler(context);
        var command = new CreateProductCommand(
            Name: "Mechanical Keyboard",
            Description: "A fine mechanical keyboard",
            Price: 99.99m,
            Sku: "KB-001",
            StockQuantity: 15);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == result);
        product.Should().NotBeNull();
        product!.Name.Should().Be("Mechanical Keyboard");
        product.Sku.Should().Be("KB-001");
        product.Price.Should().Be(99.99m);
        product.StockQuantity.Should().Be(15);
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DuplicateSku_ShouldThrowDomainValidationException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var existing = Product.Create("Existing Product", "Desc", 45m, "DUP-001", 10);
        await context.Products.AddAsync(existing);
        await context.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(context);
        var command = new CreateProductCommand(
            Name: "Another Product",
            Description: "Another Desc",
            Price: 50m,
            Sku: "dup-001", // case-insensitive check
            StockQuantity: 5);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("*already exists*");
    }
}