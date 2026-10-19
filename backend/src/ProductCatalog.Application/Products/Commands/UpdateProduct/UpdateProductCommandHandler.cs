using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Exceptions;

namespace ProductCatalog.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly IProductDbContext _context;

    public UpdateProductCommandHandler(IProductDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            throw new EntityNotFoundException(nameof(Product), request.Id);
        }

        var normalizedSku = request.Sku.Trim().ToUpperInvariant();
        var skuTakenByOther = await _context.Products
            .AnyAsync(p => p.Sku == normalizedSku && p.Id != request.Id, cancellationToken);

        if (skuTakenByOther)
        {
            throw new DomainValidationException($"Another product already has the SKU '{normalizedSku}'.");
        }

        product.Update(
            request.Name,
            request.Description,
            request.Price,
            normalizedSku,
            request.StockQuantity,
            request.IsActive);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}