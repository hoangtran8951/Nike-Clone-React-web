using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Exceptions;

namespace ProductCatalog.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductDbContext _context;

    public CreateProductCommandHandler(IProductDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var normalizedSku = request.Sku.Trim().ToUpperInvariant();
        var skuExists = await _context.Products
            .AnyAsync(p => p.Sku == normalizedSku, cancellationToken);

        if (skuExists)
        {
            throw new DomainValidationException($"Product with SKU '{normalizedSku}' already exists.");
        }

        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            normalizedSku,
            request.StockQuantity);

        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}