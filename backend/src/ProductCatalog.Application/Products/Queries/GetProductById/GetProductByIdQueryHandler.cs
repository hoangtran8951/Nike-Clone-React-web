using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.DTOs;

namespace ProductCatalog.Application.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductResponseDto?>
{
    private readonly IProductDbContext _context;

    public GetProductByIdQueryHandler(IProductDbContext context)
    {
        _context = context;
    }

    public async Task<ProductResponseDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            return null;

        return new ProductResponseDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Sku,
            product.StockQuantity,
            product.IsActive,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
    }
}