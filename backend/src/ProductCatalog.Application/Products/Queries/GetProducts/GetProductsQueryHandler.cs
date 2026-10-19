using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.DTOs;

namespace ProductCatalog.Application.Products.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductResponseDto>>
{
    private readonly IProductDbContext _context;

    public GetProductsQueryHandler(IProductDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ProductResponseDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking();

        if (request.ActiveOnly.HasValue)
        {
            query = query.Where(p => p.IsActive == request.ActiveOnly.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Sku.ToLower().Contains(search) ||
                p.Description.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponseDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Sku,
                p.StockQuantity,
                p.IsActive,
                p.CreatedAtUtc,
                p.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PaginatedList<ProductResponseDto>(items, totalCount, pageNumber, pageSize);
    }
}