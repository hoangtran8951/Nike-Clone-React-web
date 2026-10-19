using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Persistence;

public class ProductDbInitializer
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductDbInitializer> _logger;

    public ProductDbInitializer(ProductDbContext context, ILogger<ProductDbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlite())
            {
                await _context.Database.MigrateAsync();
            }

            await SeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing or seeding the database.");
            throw;
        }
    }

    private async Task SeedAsync()
    {
        if (await _context.Products.AnyAsync())
        {
            return;
        }

        var initialProducts = new List<Product>
        {
            Product.Create(
                name: "Ergonomic Mechanical Keyboard",
                description: "Wireless RGB split ergonomic mechanical keyboard with tactile switches.",
                price: 159.99m,
                sku: "KB-MECH-001",
                stockQuantity: 45),
            Product.Create(
                name: "Ultra-Wide Gaming Monitor 34\"",
                description: "144Hz 34-inch curved ultra-wide gaming display with HDR support.",
                price: 549.50m,
                sku: "MON-34UW-002",
                stockQuantity: 20),
            Product.Create(
                name: "Noise-Cancelling Headphones",
                description: "Premium over-ear wireless headphones with active noise cancellation and 40h battery.",
                price: 299.00m,
                sku: "AUD-NCH-003",
                stockQuantity: 65),
            Product.Create(
                name: "Precision Ergonomic Mouse",
                description: "Rechargeable ergonomic wireless mouse with customizable thumb scroll and multi-device pairing.",
                price: 79.95m,
                sku: "MOU-PERG-004",
                stockQuantity: 110),
            Product.Create(
                name: "Aluminum Laptop Stand",
                description: "Adjustable ventilated aluminum desktop riser compatible with 11-17 inch laptops.",
                price: 39.99m,
                sku: "ACC-STAND-005",
                stockQuantity: 80)
        };

        await _context.Products.AddRangeAsync(initialProducts);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Database seeded with {Count} starter products.", initialProducts.Count);
    }
}