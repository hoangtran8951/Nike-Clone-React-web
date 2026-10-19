using MediatR;

namespace ProductCatalog.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Sku,
    int StockQuantity) : IRequest<Guid>;