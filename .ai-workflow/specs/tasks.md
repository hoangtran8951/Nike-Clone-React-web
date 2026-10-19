# Implementation Tasks & Checklist: Create Backend and SQLite DB for React App

## Overview & Delivery Scope
Establish a production-grade .NET 8 Web API backend using Clean Architecture, CQRS (via MediatR), and Entity Framework Core with SQLite. This replaces mock product data in the React frontend with persistent, verified database-backed endpoints. Work encompasses scaffolding the .NET solution, implementing domain models, EF Core mappings and migrations, CQRS queries/commands with FluentValidation, global error handling middleware, integration/unit test suites (>85% branch coverage), and updating the React frontend service layer to consume the new API.

---

## Micro-Tasks

- [ ] **TASK-01: Backend Solution & Project Structure Scaffolding**
  - **Target Files:**
    - `backend/ProductCatalog.sln`
    - `backend/src/ProductCatalog.Domain/ProductCatalog.Domain.csproj`
    - `backend/src/ProductCatalog.Application/ProductCatalog.Application.csproj`
    - `backend/src/ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj`
    - `backend/src/ProductCatalog.Api/ProductCatalog.Api.csproj`
  - **Description:** Scaffold the .NET 8 multi-project solution structure under `backend/`, establish cross-project references conforming to Clean Architecture principles, and install required NuGet packages.
  - **Action Items:**
    - Run CLI commands to generate `ProductCatalog.sln` and four projects: `Domain` (Classlib), `Application` (Classlib), `Infrastructure` (Classlib), and `Api` (WebAPI).
    - Establish project dependencies:
      - `ProductCatalog.Api` -> references `ProductCatalog.Application` and `ProductCatalog.Infrastructure`.
      - `ProductCatalog.Infrastructure` -> references `ProductCatalog.Application` and `ProductCatalog.Domain`.
      - `ProductCatalog.Application` -> references `ProductCatalog.Domain`.
    - Install package dependencies:
      - `ProductCatalog.Infrastructure`: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore.Tools`.
      - `ProductCatalog.Application`: `MediatR`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`.
      - `ProductCatalog.Api`: `Swashbuckle.AspNetCore`, `Microsoft.AspNetCore.OpenApi`.
  - **Acceptance / Verification:**
    - Execute `dotnet build backend/ProductCatalog.sln` with 0 errors and 0 warnings.

---

- [ ] **TASK-02: Domain Models & Domain Exceptions**
  - **Target Files:**
    - `backend/src/ProductCatalog.Domain/Entities/Product.cs`
    - `backend/src/ProductCatalog.Domain/Exceptions/EntityNotFoundException.cs`
    - `backend/src/ProductCatalog.Domain/Exceptions/DomainValidationException.cs`
  - **Description:** Implement pure domain models and custom domain exceptions without external infrastructure dependencies.
  - **Action Items:**
    - Create `Product.cs` with properties:
      - `Guid Id`, `string Name`, `string Description`, `decimal Price`, `string Sku`, `int StockQuantity`, `bool IsActive`, `DateTime CreatedAtUtc`, `DateTime? UpdatedAtUtc`.
    - Provide factory method or constructor enforcing invariant rules (non-empty ID, non-empty SKU, positive price, non-negative stock).
    - Create `EntityNotFoundException.cs` and `DomainValidationException.cs` to model domain-level failure states.
  - **Acceptance / Verification:**
    - Compile `backend/src/ProductCatalog.Domain` cleanly via `dotnet build backend/src/ProductCatalog.Domain/ProductCatalog.Domain.csproj`.

---

- [ ] **TASK-03: Infrastructure Persistence & EF Core SQLite DbContext**
  - **Target Files:**
    - `backend/src/ProductCatalog.Application/Common/Interfaces/IProductDbContext.cs`
    - `backend/src/ProductCatalog.Infrastructure/Persistence/ProductDbContext.cs`
    - `backend/src/ProductCatalog.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`
    - `backend/src/ProductCatalog.Infrastructure/DependencyInjection.cs`
  - **Description:** Implement EF Core database context, fluent entity configurations, SQLite data type conversions, and service registration.
  - **Action Items:**
    - Create `IProductDbContext.cs` in `ProductCatalog.Application` exposing `DbSet<Product> Products` and `Task<int> SaveChangesAsync(CancellationToken cancellationToken)`.
    - Create `ProductConfiguration.cs` implementing `IEntityTypeConfiguration<Product>`:
      - Primary key: `Id`.
      - Unique index on `Sku`.
      - Max lengths: `Name` (200), `Description` (1000), `Sku` (50).
      - Precision mapping for `Price` (decimal) for SQLite compatibility.
    - Implement `ProductDbContext.cs` inheriting from `DbContext` and implementing `IProductDbContext`.
    - Create `DependencyInjection.cs` in `ProductCatalog.Infrastructure` to register `ProductDbContext` using `options.UseSqlite(connectionString)`.
  - **Acceptance / Verification:**
    - `dotnet build backend/src/ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj` succeeds without compile errors.

---

- [ ] **TASK-04: EF Core Migrations & Seed Data Setup**
  - **Target Files:**
    - `backend/src/ProductCatalog.Infrastructure/Persistence/Migrations/*`
    - `backend/src/ProductCatalog.Infrastructure/Persistence/ProductDbInitializer.cs`
    - `backend/src/ProductCatalog.Api/appsettings.json`
    - `backend/src/ProductCatalog.Api/appsettings.Development.json`
  - **Description:** Generate initial database migrations for SQLite and create database initialization/seeding logic with initial mock-equivalent catalog items.
  - **Action Items:**
    - Configure default connection string `"Data Source=products.db"` in `appsettings.json` and `appsettings.Development.json`.
    - Run `dotnet ef migrations add InitialCreate --project backend/src/ProductCatalog.Infrastructure --startup-project backend/src/ProductCatalog.Api`.
    - Create `ProductDbInitializer.cs` to apply pending migrations (`db.Database.MigrateAsync()`) and seed starter product records if `Products` table is empty.
  - **Acceptance / Verification:**
    - Generated migrations folder contains the initial migration and snapshot files. Initializer applies schema cleanly without throwing exceptions.

---

- [ ] **TASK-05: Application CQRS Layer - DTOs & Queries**
  - **Target Files:**
    - `backend/src/ProductCatalog.Application/Products/DTOs/ProductResponseDto.cs`
    - `backend/src/ProductCatalog.Application/Products/DTOs/PaginatedList.cs`
    - `backend/src/ProductCatalog.Application/Products/Queries/GetProducts/GetProductsQuery.cs`
    - `backend/src/ProductCatalog.Application/Products/Queries/GetProducts/GetProductsQueryHandler.cs`
    - `backend/src/ProductCatalog.Application/Products/Queries/GetProductById/GetProductByIdQuery.cs`
    - `backend/src/ProductCatalog.Application/Products/Queries/GetProductById/GetProductByIdQueryHandler.cs`
    - `backend/src/ProductCatalog.Application/DependencyInjection.cs`
  - **Description:** Implement query handling for fetching products with pagination and retrieval by ID using MediatR and non-tracking EF Core queries.
  - **Action Items:**
    - Implement `ProductResponseDto` and generic `PaginatedList<T>` (items, totalCount, pageNumber, pageSize, totalPages).
    - Implement `GetProductsQuery` (parameters: `SearchTerm`, `PageNumber = 1`, `PageSize = 10`) returning `PaginatedList<ProductResponseDto>`.
    - Implement `GetProductsQueryHandler` using `AsNoTracking()`, filtering by name/SKU if provided, and paginating.
    - Implement `GetProductByIdQuery(Guid Id)` returning `ProductResponseDto?`.
    - Register MediatR handlers in `ProductCatalog.Application/DependencyInjection.cs`.
  - **Acceptance / Verification:**
    - Build `backend/src/ProductCatalog.Application` via `dotnet build`. Verify clean compilation.

---

- [ ] **TASK-06: Application CQRS Layer - Commands & Request Validation**
  - **Target Files:**
    - `backend/src/ProductCatalog.Application/Products/Commands/CreateProduct/CreateProductCommand.cs`
    - `backend/src/ProductCatalog.Application/Products/Commands/CreateProduct/CreateProductCommandHandler.cs`
    - `backend/src/ProductCatalog.Application/Products/Commands/CreateProduct/CreateProductCommandValidator.cs`
    - `backend/src/ProductCatalog.Application/Products/Commands/UpdateProduct/UpdateProductCommand.cs`
    - `backend/src/ProductCatalog.Application/Products/Commands/UpdateProduct/UpdateProductCommandHandler.cs`
    - `backend/src/ProductCatalog.Application/Products/Commands/UpdateProduct/UpdateProductCommandValidator.cs`
    - `backend/src/ProductCatalog.Application/Products/Commands/DeleteProduct/DeleteProductCommand.cs`
    - `backend/src/ProductCatalog.Application/Products/Commands/DeleteProduct/DeleteProductCommandHandler.cs`
    - `backend/src/ProductCatalog.Application/Common/Behaviors/ValidationBehavior.cs`
  - **Description:** Implement mutating operations (Create, Update, Delete) with MediatR pipeline validation behaviors and FluentValidation rules.
  - **Action Items:**
    - Implement `CreateProductCommand` & `CreateProductCommandHandler`: Validates uniqueness of SKU, instantiates domain entity, persists via `IProductDbContext.SaveChangesAsync`, returns generated `Guid`.
    - Implement `UpdateProductCommand` & `UpdateProductCommandHandler`: Loads product by ID, throws `EntityNotFoundException` if missing, updates fields, updates `UpdatedAtUtc`, and persists changes.
    - Implement `DeleteProductCommand` & `DeleteProductCommandHandler`: Deletes product or throws `EntityNotFoundException`.
    - Implement `ValidationBehavior<TRequest, TResponse>` pipeline behavior to execute FluentValidators before command handlers.
    - Create FluentValidation validators ensuring `Price > 0`, `StockQuantity >= 0`, `Name` not empty, and `Sku` conform to alphanumeric rules.
  - **Acceptance / Verification:**
    - `dotnet build backend/ProductCatalog.sln` compiles successfully with MediatR and FluentValidation registrations.

---

- [ ] **TASK-07: API Controllers, Global Exception Middleware & Program Pipeline**
  - **Target Files:**
    - `backend/src/ProductCatalog.Api/Controllers/ApiControllerBase.cs`
    - `backend/src/ProductCatalog.Api/Controllers/ProductsController.cs`
    - `backend/src/ProductCatalog.Api/Middleware/GlobalExceptionHandlingMiddleware.cs`
    - `backend/src/ProductCatalog.Api/Program.cs`
  - **Description:** Implement RESTful endpoints, exception-to-HTTP mapping middleware, CORS policies, and Web API bootstrapper.
  - **Action Items:**
    - Implement `ApiControllerBase` injecting `ISender` (MediatR).
    - Implement `ProductsController` with actions:
      - `GET /api/products`: calls `GetProductsQuery`, returns `200 OK`.
      - `GET /api/products/{id}`: calls `GetProductByIdQuery`, returns `200 OK` or `404 Not Found`.
      - `POST /api/products`: calls `CreateProductCommand`, returns `201 Created` with `CreatedAtAction`.
      - `PUT /api/products/{id}`: calls `UpdateProductCommand`, returns `200 OK` or `404 Not Found`.
      - `DELETE /api/products/{id}`: calls `DeleteProductCommand`, returns `204 No Content` or `404 Not Found`.
    - Implement `GlobalExceptionHandlingMiddleware`:
      - Catch `FluentValidation.ValidationException` -> return `400 Bad Request` with structured `{ errors: { field: [messages] } }`.
      - Catch `EntityNotFoundException` -> return `404 Not Found` with RFC 7807 ProblemDetails.
      - Catch unhandled `Exception` -> return `500 Internal Server Error` with generic ProblemDetails.
    - Configure `Program.cs`:
      - Register DI for Application, Infrastructure, and Api layers.
      - Enable CORS allowing `http://localhost:3000` and `http://localhost:5173` with any method/header.
      - Add Swagger / OpenAPI UI.
      - Run `ProductDbInitializer` on startup.
  - **Acceptance / Verification:**
    - Start API using `dotnet run --project backend/src/ProductCatalog.Api` and verify Swagger UI is accessible at `/swagger/index.html`.

---

- [ ] **TASK-08: Application Unit Tests**
  - **Target Files:**
    - `backend/tests/ProductCatalog.Application.UnitTests/ProductCatalog.Application.UnitTests.csproj`
    - `backend/tests/ProductCatalog.Application.UnitTests/Products/CreateProductCommandHandlerTests.cs`
    - `backend/tests/ProductCatalog.Application.UnitTests/Products/UpdateProductCommandHandlerTests.cs`
    - `backend/tests/ProductCatalog.Application.UnitTests/Products/GetProductsQueryHandlerTests.cs`
    - `backend/tests/ProductCatalog.Application.UnitTests/Products/ValidationTests.cs`
  - **Description:** Write comprehensive unit tests for commands, queries, and validators using xUnit, FluentAssertions, and Moq.
  - **Action Items:**
    - Create unit test project and reference `ProductCatalog.Application` and `ProductCatalog.Domain`.
    - Test `CreateProductCommandHandler`: successfully persists valid product; rejects duplicate SKU.
    - Test `UpdateProductCommandHandler`: throws `EntityNotFoundException` for invalid ID; successfully updates existing product.
    - Test `GetProductsQueryHandler`: handles empty state, applies search filtering, and paginates accurately.
    - Test `CreateProductCommandValidator` and `UpdateProductCommandValidator`: validates invalid price (`<= 0`), negative stock, and empty SKU/Name.
  - **Acceptance / Verification:**
    - Execute `dotnet test backend/tests/ProductCatalog.Application.UnitTests/ProductCatalog.Application.UnitTests.csproj` with 100% passing tests.

---

- [ ] **TASK-09: API Integration Tests with In-Memory SQLite**
  - **Target Files:**
    - `backend/tests/ProductCatalog.Api.IntegrationTests/ProductCatalog.Api.IntegrationTests.csproj`
    - `backend/tests/ProductCatalog.Api.IntegrationTests/Infrastructure/CustomWebApplicationFactory.cs`
    - `backend/tests/ProductCatalog.Api.IntegrationTests/Controllers/ProductsControllerTests.cs`
  - **Description:** Implement end-to-end integration tests using `Microsoft.AspNetCore.Mvc.Testing` and SQLite in-memory mode (`DataSource=:memory:`).
  - **Action Items:**
    - Scaffold `ProductCatalog.Api.IntegrationTests` referencing `ProductCatalog.Api`.
    - Implement `CustomWebApplicationFactory` replacing SQLite connection with an open `SqliteConnection("DataSource=:memory:")` and ensuring schema creation via `EnsureCreated()`.
    - Implement test scenarios in `ProductsControllerTests`:
      - `POST /api/products` with valid payload returns `201 Created` with expected location header.
      - `POST /api/products` with invalid payload returns `400 Bad Request` with validation error dictionary.
      - `GET /api/products` returns `200 OK` with paginated list containing created item.
      - `GET /api/products/{id}` returns `200 OK` for existing item, `404 Not Found` for non-existent GUID.
      - `PUT /api/products/{id}` updates attributes and returns `200 OK`.
      - `DELETE /api/products/{id}` returns `204 No Content` and subsequent `GET` returns `404 Not Found`.
  - **Acceptance / Verification:**
    - Execute `dotnet test backend/tests/ProductCatalog.Api.IntegrationTests` and ensure all tests pass.
    - Ensure combined solution test coverage exceeds 85% branch coverage via `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura`.

---

- [ ] **TASK-10: Frontend API Client Integration & Mock Removal**
  - **Target Files:**
    - `src/services/productService.ts` (or relevant frontend API service file)
    - `src/types/product.ts` (or relevant frontend product models)
    - `.env.development` or `src/config/apiConfig.ts`
  - **Description:** Update the React frontend to replace mock data structures and service calls with live HTTP calls to the .NET 8 Web API.
  - **Action Items:**
    - Locate the existing product mock service/data in `src/`.
    - Define TypeScript interfaces matching `ProductResponseDto` and `PaginatedList`.
    - Implement or update `productService.ts` to call `http://localhost:<API_PORT>/api/products` using `fetch` or `axios`.
    - Add environment variable `VITE_API_BASE_URL` or `REACT_APP_API_BASE_URL` pointing to backend API server.
    - Verify product listing, pagination, and details views in the React app display live data from SQLite.
  - **Acceptance / Verification:**
    - Run `npm test` or `npm run build` to verify frontend compiles with zero TypeScript/lint errors.
    - Verifiable data roundtrip: Adding/editing a product reflects directly in the SQLite database and persists across UI refreshes.

---

- [ ] **TASK-11: Repository Hygiene, Solution Verification & PR Readiness**
  - **Target Files:**
    - `.gitignore`
    - `backend/README.md`
  - **Description:** Finalize configuration, update `.gitignore` to prevent database binaries from being checked in, and document backend run steps.
  - **Action Items:**
    - Add `*.db`, `*.db-shm`, `*.db-wal`, `backend/**/bin/`, and `backend/**/obj/` to `.gitignore`.
    - Create `backend/README.md` documenting prerequisite (.NET 8 SDK), how to run migrations, start the backend, and run the test suite.
    - Execute clean solution build: `dotnet build backend/ProductCatalog.sln -c Release`.
    - Execute test runs across backend and frontend:
      - `dotnet test backend/ProductCatalog.sln`
      - `npm run test -- --watchAll=false` (or equivalent FE test script).
  - **Acceptance / Verification:**
    - `git status` shows no untracked SQLite binary files.
    - Full backend and frontend build and test pipelines execute with zero failures.
