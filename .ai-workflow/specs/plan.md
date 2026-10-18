# Implementation Plan: Bring UI Data to the Backend (.NET 8 / EF Core / CQRS / SQLite)

## Overview & Architecture Strategy

This plan creates a new .NET 8 Web API backend (`ProductService`) inside the existing repository root (alongside the TypeScript frontend) to replace mocked product data. 

The backend will follow Clean/Vertical Slice patterns with CQRS powered by `MediatR` and `FluentValidation`. Data persistence uses `Microsoft.EntityFrameworkCore.Sqlite` targeting a local SQLite file database (`products.db`). Cross-Origin Resource Sharing (CORS) is enabled to support local UI development ports (e.g., `http://localhost:3000`, `http://localhost:5173`).

```
Repository Root /
├── src/                          (Existing TypeScript UI)
├── backend/
│   ├── ProductService.sln
│   ├── src/
│   │   └── ProductService.Api/   (.NET 8 Web API, EF Core, CQRS handlers)
│   └── tests/
│       └── ProductService.Tests/ (xUnit, WebApplicationFactory, SQLite In-Memory)
```

---

## Micro-Tasks

### 1. Module & Directory Structure Setup
- [ ] Initialize backend project structure:
  - Create `/backend/ProductService.sln`.
  - Create `/backend/src/ProductService.Api/ProductService.Api.csproj` (Target: `net8.0`).
  - Create `/backend/tests/ProductService.Tests/ProductService.Tests.csproj` (Target: `net8.0`).
  - Add projects to solution: `dotnet sln add ...`.
- [ ] Add required NuGet packages to `ProductService.Api`:
  - `Microsoft.EntityFrameworkCore.Sqlite` (8.x)
  - `Microsoft.EntityFrameworkCore.Design` (8.x)
  - `MediatR` (12.x)
  - `FluentValidation.AspNetCore` / `FluentValidation.DependencyInjectionExtensions` (11.x)
- [ ] Add required NuGet packages to `ProductService.Tests`:
  - `Microsoft.AspNetCore.Mvc.Testing` (8.x)
  - `Microsoft.EntityFrameworkCore.Sqlite` (8.x)
  - `FluentAssertions`
  - `xunit` & `xunit.runner.visualstudio`
- [ ] Define folder layout under `ProductService.Api`:
  - `Domain/Entities/`
  - `Infrastructure/Persistence/` (DbContext, Migrations, Seeder)
  - `Features/Products/Commands/` (Create, Update, Delete)
  - `Features/Products/Queries/` (GetProducts, GetProductById)
  - `Features/Products/Dtos/` (ProductResponseDto, PagedResultDto)
  - `Common/Behaviors/` (ValidationPipelineBehavior, GlobalExceptionHandler)
  - `Controllers/` (ProductsController)

---

### 2. Domain Models, DTOs, and Database Persistence
- [ ] Create domain entity `Product` in `Domain/Entities/Product.cs` matching specification fields (`Id`, `Name`, `Description`, `Sku`, `Price`, `Category`, `ImageUrl`, `StockQuantity`, `IsActive`, `CreatedAtUtc`, `UpdatedAtUtc`).
- [ ] Implement DTOs in `Features/Products/Dtos/`:
  - `ProductResponseDto.cs` mapping all domain properties to camelCase JSON representations matching the frontend TypeScript interface.
  - `PagedResultDto<T>.cs` (`items`, `page`, `pageSize`, `totalCount`, `totalPages`).
- [ ] Implement `ProductDbContext` in `Infrastructure/Persistence/ProductDbContext.cs`:
  - Define `DbSet<Product> Products`.
  - Configure entity mappings via Fluent API: primary key `Id`, max lengths (`Name` 200, `Description` 2000, `Sku` 100, `Category` 100), precision for `Price` (18, 2), unique index on `Sku`, and index on `Category`.
- [ ] Implement database seeder in `Infrastructure/Persistence/DbInitializer.cs`:
  - Checks `context.Database.EnsureCreatedAsync()` or executes pending EF migrations.
  - Seeds initial catalog if `Products` table is empty (e.g., Electronics and Apparel mock items matching UI mocks).
- [ ] Generate initial EF Core migration (`InitialCreate`) or configure automatic creation for SQLite in `appsettings.Development.json` (`Data Source=products.db`).

---

### 3. Service, CQRS Pipeline, and Controller Logic
- [ ] Implement MediatR Validation Pipeline Behavior in `Common/Behaviors/ValidationBehavior.cs`:
  - Intercepts requests, runs registered `FluentValidation` validators, and throws a custom `ValidationException` or returns 400 Bad Request RFC 7807 responses.
- [ ] Implement Queries:
  - **`GetProductsQuery`** & **`GetProductsQueryHandler`**:
    - Filter by `Category` and `SearchTerm` (against `Name` and `Description`).
    - Apply pagination (`Page`, `PageSize`).
    - Use `AsNoTracking()` and return `PagedResultDto<ProductResponseDto>`.
  - **`GetProductByIdQuery`** & **`GetProductByIdQueryHandler`**:
    - Fetch product by `Guid Id` with `AsNoTracking()`.
    - Return `ProductResponseDto?`.
- [ ] Implement Commands & Validators:
  - **`CreateProductCommand`**, validator (`CreateProductCommandValidator` checking non-empty name/sku, price > 0, stock >= 0), and handler:
    - Check SKU collision (throws conflict / returns 409).
    - Map to entity, assign `Id = Guid.NewGuid()` and `CreatedAtUtc = DateTime.UtcNow`.
    - Persist and return `ProductResponseDto`.
  - **`UpdateProductCommand`**, validator, and handler:
    - Fetch product by `Id`; return null/not found if missing.
    - Check SKU collision against other products.
    - Update entity fields and set `UpdatedAtUtc = DateTime.UtcNow`.
  - **`DeleteProductCommand`** and handler:
    - Remove product entity by `Id` (hard delete per specification default) or return not found.
- [ ] Implement `Controllers/ProductsController.cs` exposing base route `/api/v1/products`:
  - `GET /api/v1/products` -> `GetProductsQuery` (`200 OK`)
  - `GET /api/v1/products/{id:guid}` -> `GetProductByIdQuery` (`200 OK`, `404 Not Found`)
  - `POST /api/v1/products` -> `CreateProductCommand` (`201 Created` with `Location` header, `400 Bad Request`, `409 Conflict`)
  - `PUT /api/v1/products/{id:guid}` -> `UpdateProductCommand` (`200 OK`, `400 Bad Request`, `404 Not Found`, `409 Conflict`)
  - `DELETE /api/v1/products/{id:guid}` -> `DeleteProductCommand` (`204 No Content`, `404 Not Found`)
- [ ] Configure `Program.cs`:
  - Register SQLite connection string (`Data Source=products.db`).
  - Register MediatR and FluentValidation assemblies.
  - Configure CORS policy (`AllowFrontendOrigins`) allowing `http://localhost:3000` and `http://localhost:5173` with `AllowAnyMethod`, `AllowAnyHeader`, and `AllowCredentials`.
  - Register global error handling middleware / ProblemDetails.
  - Hook `DbInitializer.InitializeAsync(app)` on startup.

---

### 4. Automated Verification & Test Suite
- [ ] Setup `CustomWebApplicationFactory<Program>` in `ProductService.Tests` using SQLite In-Memory connection (`DataSource=:memory:` with open `SqliteConnection`).
- [ ] Implement Unit/Handler Tests:
  - `CreateProductCommandHandlerTests`: Validates SKU conflict raises exception, valid product persists.
  - `ValidationBehaviorTests`: Validates negative prices and empty fields trigger validation failure.
- [ ] Implement Integration Tests (`ProductsControllerTests`):
  - **Scenario 1 (Startup & Seeding)**: Verify seeded products are returned on fresh DB.
  - **Scenario 2 (Filter & Pagination)**: `GET /api/v1/products?category=Electronics` returns filtered items and metadata.
  - **Scenario 3 (Create Product)**: `POST /api/v1/products` returns `201 Created` with valid `Location` header and payload matching TS interface.
  - **Scenario 4 (Validation Error)**: `POST` with negative price returns `400 Bad Request` with RFC 7807 problem details.
  - **Scenario 5 (Duplicate SKU)**: Duplicate SKU creation returns `409 Conflict`.
  - **Scenario 6 (Not Found)**: `GET /api/v1/products/{nonExistentGuid}` returns `404 Not Found`.
  - **Scenario 7 (CORS Preflight)**: Send `OPTIONS` request with `Origin: http://localhost:5173` and verify `Access-Control-Allow-Origin` header.

---

### 5. Deployment & Sandbox Verification Steps
- [ ] Run `dotnet build backend/ProductService.sln` and ensure zero compilation warnings or errors.
- [ ] Execute `dotnet test backend/ProductService.sln` and ensure all test scenarios pass.
- [ ] Boot API using `dotnet run --project backend/src/ProductService.Api`:
  - Inspect working directory to verify `products.db` SQLite file is generated automatically.
  - Issue curl command:
    ```bash
    curl -i http://localhost:5000/api/v1/products
    ```
  - Verify JSON payload structure complies with frontend `Product` TypeScript definition (`id`, `name`, `sku`, `price`, `category`, etc.).
- [ ] Update frontend environment or documentation with the new backend API endpoint URL (`http://localhost:5000/api/v1/products`).