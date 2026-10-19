# Implementation Plan: Create .NET 8 Backend & SQLite DB for Product Catalog

## Overview & Architecture Strategy
This implementation establishes a production-grade .NET 8 Web API backend using Entity Framework Core and Command Query Responsibility Segregation (CQRS) backed by a local SQLite database. The backend will replace mock product data in the React frontend.

### Architectural Layout
The backend will reside in a dedicated `backend/` directory structured according to clean architecture and CQRS principles:
- **Domain**: Pure business entities (`Product`), domain exceptions, and value types.
- **Application**: CQRS Commands, Queries, Handlers (using MediatR), DTOs, and request validation (FluentValidation).
- **Infrastructure**: Entity Framework Core DbContext (`ProductDbContext`), SQLite provider configuration, migrations, and repository/read-store implementations.
- **Api**: ASP.NET Core Web API controllers, middleware (exception handling, logging), CORS policies, and dependency injection wiring.

```
├── backend/
│   ├── ProductCatalog.sln
│   ├── src/
│   │   ├── ProductCatalog.Domain/
│   │   ├── ProductCatalog.Application/
│   │   ├── ProductCatalog.Infrastructure/
│   │   └── ProductCatalog.Api/
│   └── tests/
│       ├── ProductCatalog.Application.UnitTests/
│       └── ProductCatalog.Api.IntegrationTests/
├── src/ (React frontend)
└── package.json
```

---

## Micro-Tasks

### 1. Project Initialization & Directory Structure
- [ ] Scaffold .NET 8 Solution and projects under `backend/`:
  - `dotnet new sln -n ProductCatalog -o backend`
  - `dotnet new classlib -n ProductCatalog.Domain -o backend/src/ProductCatalog.Domain`
  - `dotnet new classlib -n ProductCatalog.Application -o backend/src/ProductCatalog.Application`
  - `dotnet new classlib -n ProductCatalog.Infrastructure -o backend/src/ProductCatalog.Infrastructure`
  - `dotnet new webapi -n ProductCatalog.Api -o backend/src/ProductCatalog.Api`
- [ ] Establish project dependencies:
  - `Api` references `Application` and `Infrastructure`.
  - `Infrastructure` references `Application` and `Domain`.
  - `Application` references `Domain`.
- [ ] Install required NuGet packages:
  - Infrastructure: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`.
  - Application: `MediatR`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`.
  - Api: `Microsoft.AspNetCore.OpenApi`, `Swashbuckle.AspNetCore`.

### 2. Domain Models & Schemas (Database & EF Core)
- [ ] Define `Product` entity in `ProductCatalog.Domain/Entities/Product.cs`:
  - Attributes: `Id` (Guid), `Name` (string), `Description` (string), `Price` (decimal), `Sku` (string), `StockQuantity` (int), `IsActive` (bool), `CreatedAtUtc` (DateTime), `UpdatedAtUtc` (DateTime?).
- [ ] Implement `ProductDbContext` in `ProductCatalog.Infrastructure/Persistence/ProductDbContext.cs`:
  - Implement entity configuration via `IEntityTypeConfiguration<Product>` configuring required fields, string lengths, SQLite decimal precision mappings, and unique indexes on `Sku`.
- [ ] Configure SQLite Connection String in `appsettings.json`:
  - `"ConnectionStrings": { "DefaultConnection": "Data Source=products.db" }`.
- [ ] Create initial EF Core migration (`InitialCreate`) and SQLite seeding mechanism for demo/starter product data matching the React app's mock records.

### 3. Application CQRS Layer (Commands, Queries, DTOs)
- [ ] Define Data Transfer Objects (DTOs) in `ProductCatalog.Application/Products/DTOs`:
  - `ProductResponseDto` (Id, Name, Description, Price, Sku, StockQuantity, IsActive).
  - `CreateProductDto`, `UpdateProductDto`.
- [ ] Implement CQRS Queries:
  - `GetProductsQuery` (with optional search filter, pagination: `PageNumber`, `PageSize`).
  - `GetProductByIdQuery(Guid Id)`.
  - Query Handlers utilizing EF Core `AsNoTracking()` for high-throughput reads.
- [ ] Implement CQRS Commands:
  - `CreateProductCommand(string Name, string Description, decimal Price, string Sku, int StockQuantity)`.
  - `UpdateProductCommand(Guid Id, string Name, string Description, decimal Price, string Sku, int StockQuantity, bool IsActive)`.
  - `DeleteProductCommand(Guid Id)`.
  - Command Handlers with domain validation, atomic persistence via `SaveChangesAsync`, and optimistic concurrency checks.
- [ ] Implement Request Validation using FluentValidation:
  - Ensure `Price > 0`, `StockQuantity >= 0`, `Name` and `Sku` are non-empty and conform to length constraints.

### 4. Service & Controller Logic (API Presentation & Middleware)
- [ ] Implement `ProductsController` in `ProductCatalog.Api/Controllers/ProductsController.cs`:
  - `GET /api/products` -> Sends `GetProductsQuery`, returns `200 OK` with paginated list.
  - `GET /api/products/{id}` -> Sends `GetProductByIdQuery`, returns `200 OK` or `404 Not Found`.
  - `POST /api/products` -> Sends `CreateProductCommand`, returns `201 Created` with `Location` header and created entity.
  - `PUT /api/products/{id}` -> Sends `UpdateProductCommand`, returns `200 OK` or `400 Bad Request` / `404 Not Found`.
  - `DELETE /api/products/{id}` -> Sends `DeleteProductCommand`, returns `204 No Content` or `404 Not Found`.
- [ ] Implement Global Exception Middleware:
  - Catch unhandled exceptions and validation errors.
  - Map validation failures to `400 Bad Request` with structured field errors.
  - Map entity not found to `404 Not Found`.
  - Map internal exceptions to RFC 7807 `ProblemDetails` (`500 Internal Server Error`).
- [ ] Configure CORS in `Program.cs`:
  - Allow origins matching standard React local development environments (`http://localhost:3000`, `http://localhost:5173`).
- [ ] Ensure automatic migration apply on application startup during local development (`app.Services.CreateScope().ServiceProvider.GetRequiredService<ProductDbContext>().Database.Migrate()`).

### 5. Automated Verification
- [ ] Scaffold test projects under `backend/tests/`:
  - `ProductCatalog.Application.UnitTests` (xUnit, FluentAssertions, Moq/NSubstitute).
  - `ProductCatalog.Api.IntegrationTests` (xUnit, `Microsoft.AspNetCore.Mvc.Testing`).
- [ ] Unit Tests:
  - Test command handlers: Ensure product creation, price/stock validation, and update edge cases behave predictably.
  - Test query handlers: Test filtering, empty state, and ID lookups.
- [ ] Integration Tests:
  - Set up `CustomWebApplicationFactory` using an in-memory SQLite connection (`DataSource=:memory:`).
  - Verify complete request-response pipelines:
    - Happy paths: `POST` -> `GET by ID` -> `PUT` -> `DELETE` -> verify `404`.
    - Error paths: Malformed payloads returning `400 Bad Request` with structured JSON errors.
- [ ] Coverage Verification:
  - Run `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura` and ensure branch coverage exceeds 85%.

### 6. Deployment & Sandbox Verification
- [ ] Run backend locally (`dotnet run --project backend/src/ProductCatalog.Api`).
- [ ] Execute smoke test against Swagger UI (`/swagger/index.html`) or via cURL:
  - Seed and fetch initial products.
  - Execute sample `POST /api/products` and verify SQLite file creation on disk.
- [ ] Update React frontend configuration:
  - Add API client or update existing mock service to point to `http://localhost:<PORT>/api/products`.
  - Verify end-to-end rendering of SQLite-backed products in the React UI.

### 7. Pull Request Staging
- [ ] Clean up untracked SQLite binary files (`*.db`, `*.db-shm`, `*.db-wal`) by adding them to `.gitignore`.
- [ ] Verify clean builds: `dotnet build backend/ProductCatalog.sln` and all tests passing: `dotnet test backend/ProductCatalog.sln`.
- [ ] Ensure frontend `npm run test` or `npm run build` is unaffected.
- [ ] Stage and submit PR:
  - Commits organized by: (1) Solution setup & Domain/EF Core, (2) Application CQRS Handlers, (3) Api Controllers & Middleware, (4) Integration & Unit Tests.