using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.Products.Commands.CreateProduct;
using ProductCatalog.Application.Products.Commands.DeleteProduct;
using ProductCatalog.Application.Products.Commands.UpdateProduct;
using ProductCatalog.Application.Products.DTOs;
using ProductCatalog.Application.Products.Queries.GetProductById;
using ProductCatalog.Application.Products.Queries.GetProducts;

namespace ProductCatalog.Api.Controllers;

public class ProductsController : ApiControllerBase
{
    /// <summary>
    /// Gets a paginated list of products with optional search and active status filter.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<ProductResponseDto>>> GetProducts(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? activeOnly = null)
    {
        var result = await Mediator.Send(new GetProductsQuery(searchTerm, pageNumber, pageSize, activeOnly));
        return Ok(result);
    }

    /// <summary>
    /// Gets a single product by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> GetProductById(Guid id)
    {
        var product = await Mediator.Send(new GetProductByIdQuery(id));
        if (product == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"Product with id '{id}' was not found."
            });
        }

        return Ok(product);
    }

    /// <summary>
    /// Creates a new product catalog item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        var command = new CreateProductCommand(
            dto.Name,
            dto.Description,
            dto.Price,
            dto.Sku,
            dto.StockQuantity);

        var id = await Mediator.Send(command);
        var created = await Mediator.Send(new GetProductByIdQuery(id));

        return CreatedAtAction(nameof(GetProductById), new { id }, created);
    }

    /// <summary>
    /// Updates an existing product catalog item.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponseDto>> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var command = new UpdateProductCommand(
            id,
            dto.Name,
            dto.Description,
            dto.Price,
            dto.Sku,
            dto.StockQuantity,
            dto.IsActive);

        await Mediator.Send(command);
        var updated = await Mediator.Send(new GetProductByIdQuery(id));

        return Ok(updated);
    }

    /// <summary>
    /// Deletes a product catalog item.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await Mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }
}