using MediatR;
using ProductCatalog.Application.Products.DTOs;

namespace ProductCatalog.Application.Products.Queries.GetProducts;

public record GetProductsQuery(
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10,
    bool? ActiveOnly = null) : IRequest<PaginatedList<ProductResponseDto>>;