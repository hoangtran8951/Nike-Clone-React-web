namespace ProductCatalog.Application.Products.DTOs;

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price,
    string Sku,
    int StockQuantity,
    bool IsActive);