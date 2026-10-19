using ProductCatalog.Domain.Exceptions;

namespace ProductCatalog.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    // Required by EF Core
    private Product() { }

    public Product(
        Guid id,
        string name,
        string description,
        decimal price,
        string sku,
        int stockQuantity,
        bool isActive = true,
        DateTime? createdAtUtc = null)
    {
        if (id == Guid.Empty)
            throw new DomainValidationException("Product Id must not be empty.");

        ValidateInputs(name, sku, price, stockQuantity);

        Id = id;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        Sku = sku.Trim().ToUpperInvariant();
        StockQuantity = stockQuantity;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public static Product Create(
        string name,
        string description,
        decimal price,
        string sku,
        int stockQuantity)
    {
        return new Product(
            Guid.NewGuid(),
            name,
            description,
            price,
            sku,
            stockQuantity,
            isActive: true,
            createdAtUtc: DateTime.UtcNow);
    }

    public void Update(
        string name,
        string description,
        decimal price,
        string sku,
        int stockQuantity,
        bool isActive)
    {
        ValidateInputs(name, sku, price, stockQuantity);

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        Sku = sku.Trim().ToUpperInvariant();
        StockQuantity = stockQuantity;
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AdjustStock(int delta)
    {
        var newStock = StockQuantity + delta;
        if (newStock < 0)
        {
            throw new DomainValidationException($"Cannot reduce stock quantity below zero. Current: {StockQuantity}, change: {delta}");
        }

        StockQuantity = newStock;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void ValidateInputs(string name, string sku, decimal price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Product Name is required.");

        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainValidationException("Product Sku is required.");

        if (price <= 0)
            throw new DomainValidationException("Product Price must be greater than zero.");

        if (stockQuantity < 0)
            throw new DomainValidationException("Product StockQuantity cannot be negative.");
    }
}