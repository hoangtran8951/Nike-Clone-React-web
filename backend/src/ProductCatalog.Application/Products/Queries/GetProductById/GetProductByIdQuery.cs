using MediatR;
using ProductCatalog.Application.Products.DTOs;

namespace ProductCatalog.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductResponseDto?>;