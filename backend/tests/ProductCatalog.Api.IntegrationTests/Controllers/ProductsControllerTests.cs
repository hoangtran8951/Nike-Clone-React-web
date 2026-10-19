using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ProductCatalog.Api.IntegrationTests.Infrastructure;
using ProductCatalog.Application.Products.DTOs;
using Xunit;

namespace ProductCatalog.Api.IntegrationTests.Controllers;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSuccessStatusCodeAndPaginatedList()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<ProductResponseDto>>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task PostProduct_WithValidData_Returns201CreatedAndPersistsItem()
    {
        // Arrange
        var uniqueSku = $"TEST-{Guid.NewGuid():N}".Substring(0, 15);
        var payload = new CreateProductDto(
            Name: "Integration Test Product",
            Description: "Created via integration test",
            Price: 89.95m,
            Sku: uniqueSku,
            StockQuantity: 14);

        // Act
        var createResponse = await _client.PostAsJsonAsync("/api/products", payload);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Headers.Location.Should().NotBeNull();

        var createdProduct = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>(_jsonOptions);
        createdProduct.Should().NotBeNull();
        createdProduct!.Id.Should().NotBeEmpty();
        createdProduct.Name.Should().Be(payload.Name);
        createdProduct.Price.Should().Be(payload.Price);
        createdProduct.Sku.Should().Be(uniqueSku);
        createdProduct.StockQuantity.Should().Be(14);
        createdProduct.IsActive.Should().BeTrue();

        // Verify retrieval by Id works
        var getByIdResponse = await _client.GetAsync($"/api/products/{createdProduct.Id}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostProduct_WithInvalidData_Returns400BadRequest()
    {
        // Arrange (invalid price and empty sku)
        var invalidPayload = new CreateProductDto(
            Name: "",
            Description: "Invalid",
            Price: -5.00m,
            Sku: "",
            StockQuantity: -1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutProduct_WithValidUpdate_UpdatesAndReturns200Ok()
    {
        // Arrange: first create a product
        var uniqueSku = $"UPD-{Guid.NewGuid():N}".Substring(0, 14);
        var createPayload = new CreateProductDto(
            Name: "Initial Item",
            Description: "Initial Desc",
            Price: 25.00m,
            Sku: uniqueSku,
            StockQuantity: 10);

        var createResponse = await _client.PostAsJsonAsync("/api/products", createPayload);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>(_jsonOptions);

        var updatePayload = new UpdateProductDto(
            Name: "Updated Item Name",
            Description: "Updated Desc",
            Price: 35.50m,
            Sku: uniqueSku,
            StockQuantity: 20,
            IsActive: false);

        // Act
        var putResponse = await _client.PutAsJsonAsync($"/api/products/{created!.Id}", updatePayload);

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedProduct = await putResponse.Content.ReadFromJsonAsync<ProductResponseDto>(_jsonOptions);
        updatedProduct.Should().NotBeNull();
        updatedProduct!.Name.Should().Be("Updated Item Name");
        updatedProduct.Price.Should().Be(35.50m);
        updatedProduct.StockQuantity.Should().Be(20);
        updatedProduct.IsActive.Should().BeFalse();
        updatedProduct.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteProduct_ThenGetById_Returns204Then404()
    {
        // Arrange
        var sku = $"DEL-{Guid.NewGuid():N}".Substring(0, 14);
        var createPayload = new CreateProductDto("To Delete", "Delete me", 10.0m, sku, 1);
        var createResponse = await _client.PostAsJsonAsync("/api/products", createPayload);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponseDto>(_jsonOptions);

        // Act: Delete
        var deleteResponse = await _client.DeleteAsync($"/api/products/{created!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Confirm 404 on subsequent get
        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductById_NonExistent_Returns404NotFound()
    {
        var nonExistentId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/products/{nonExistentId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}