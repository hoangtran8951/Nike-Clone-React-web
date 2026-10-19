using MediatR;

namespace ProductCatalog.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Sku,
    int StockQuantity,
    bool IsActive) : IRequest<Unit>;