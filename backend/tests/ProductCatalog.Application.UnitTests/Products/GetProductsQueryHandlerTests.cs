using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Products.Queries.GetProductById;
using ProductCatalog.Application.Products.Queries.GetProducts;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.Persistence;
using Xunit;

namespace ProductCatalog.Application.UnitTests.Products;

public class GetProductsQueryHandlerTests
{
    private ProductDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ProductDbContext(options);
    }

    [Fact]
    public async Task Handle_EmptyStore_ShouldReturnEmptyPaginatedList()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await context.Products.AddRangeAsync(
            Product.Create("Gaming Mouse", "Wireless mouse", 59.99m, "GM-001", 10),
            Product.Create("Mechanical Keyboard", "Clicky switches", 129.99m, "KB-002", 5),
            Product.Create("Mousepad XL", "Deskmat", 19.99m, "MP-003", 25)
        );
        await context.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery(SearchTerm: "Mouse");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().Contain(p => p.Name == "Gaming Mouse");
        result.Items.Should().Contain(p => p.Name == "Mousepad XL");
        result.Items.Should().NotContain(p => p.Name == "Mechanical Keyboard");
    }

    [Fact]
    public async Task Handle_GetProductById_ShouldReturnProductWhenFound()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var product = Product.Create("Standalone Monitor", "4K HDR", 499.99m, "MON-001", 8);
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        var handler = new GetProductByIdQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Standalone Monitor");
        result.Price.Should().Be(499.99m);
    }

    [Fact]
    public async Task Handle_GetProductById_ShouldReturnNullWhenMissing()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetProductByIdQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}