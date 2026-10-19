namespace ProductCatalog.Application.Products.DTOs;

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    string Sku,
    int StockQuantity);