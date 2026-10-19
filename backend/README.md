# Product Catalog Backend & SQLite Database

This backend service implements a clean architecture and CQRS (Command Query Responsibility Segregation) pattern with .NET 8 Web API, Entity Framework Core, and SQLite. It serves catalog data directly to the React frontend application.

## Architecture

- **`ProductCatalog.Domain`**: Entities (`Product`), invariants, domain exceptions.
- **`ProductCatalog.Application`**: CQRS Commands and Queries mediated by MediatR, validation pipelines via FluentValidation, and data transfer objects.
- **`ProductCatalog.Infrastructure`**: EF Core persistence layer, SQLite database context (`ProductDbContext`), fluent schema configurations, and initial data seeding.
- **`ProductCatalog.Api`**: REST controllers, global exception middleware (ProblemDetails / RFC 7807), CORS configuration, and Swagger documentation.
- **`tests/`**: Unit tests covering command/query handlers and validators, and integration tests using WebApplicationFactory with in-memory SQLite.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Quick Start

### 1. Build Solution
```bash
cd backend
dotnet build ProductCatalog.sln
```

### 2. Run Automated Tests
```bash
dotnet test ProductCatalog.sln
```

### 3. Run the API Server
```bash
dotnet run --project src/ProductCatalog.Api
```

The server will automatically create `products.db` (if it does not exist), apply the EF Core migrations, seed initial demo products, and listen on `http://localhost:5000` (or the configured launch URL).

Swagger UI is available at:
`http://localhost:5000/swagger/index.html`

## Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/products` | Retrieve paginated products with optional search and active status filter |
| `GET` | `/api/products/{id}` | Get product details by GUID |
| `POST` | `/api/products` | Create a new product |
| `PUT` | `/api/products/{id}` | Update an existing product |
| `DELETE` | `/api/products/{id}` | Remove a product |