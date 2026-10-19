namespace ProductCatalog.Application.Products.DTOs;

public record ProductResponseDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Sku,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);