# Feature Specification: Bring UI Data to the Backend (.NET / EF Core / CQRS / SQLite)

## Executive Summary & Boundary

This specification defines the architecture, data structures, and implementation requirements for establishing a dedicated backend service to replace mocked product data in the existing frontend (TypeScript application). 

The backend service will be implemented using **.NET 8**, **Entity Framework Core (EF Core)**, and a **Command Query Responsibility Segregation (CQRS)** pattern, persisting data to an embedded **SQLite** database. The scope encompasses the database schema, EF Core mapping, CQRS handlers, RESTful API endpoints, database seeding to support UI transition, and CORS enablement to facilitate client-side integration.

```
+--------------------------+          HTTP / JSON          +-------------------------------+
|  TypeScript Frontend UI  |  <=========================>  |       ASP.NET Core Web API    |
| (Mocked Data Replaced)   |     (CORS Enabled Origin)     | (Endpoints / Routing / Auth)  |
+--------------------------+                               +---------------+---------------+
                                                                           |
                                                    +----------------------+----------------------+
                                                    | CQRS Pipeline (MediatR / Handlers)          |
                                                    |  - Commands: Create, Update, Delete Product |
                                                    |  - Queries:  GetProducts, GetProductById    |
                                                    +----------------------+----------------------+
                                                                           |
                                                    +----------------------+----------------------+
                                                    | Entity Framework Core (DbContext)           |
                                                    |  - SQLite Provider                          |
                                                    |  - Fluent API Model Configuration           |
                                                    +----------------------+----------------------+
                                                                           |
                                                                   [( SQLite DB )]
```

---

## User Story & Developer Guidance

### User Story
As a developer and system operator,  
I want to move product catalog information from static frontend mock files into a persistent database managed by a .NET backend,  
So that product data can be queried, modified, and persisted dynamically across client sessions.

### Developer Guidance & Technical Constraints
1. **Platform**: .NET 8 Web API.
2. **Persistence**: Entity Framework Core (`Microsoft.EntityFrameworkCore.Sqlite`).
3. **Architecture**: Command Query Responsibility Segregation (CQRS) utilizing `MediatR` (or a dedicated mediator dispatch pattern).
4. **Database Engine**: SQLite stored in a localized, configurable file path (e.g., `products.db`).
5. **Backwards Compatibility / UI Handoff**: The backend MUST expose a JSON structure that either matches or maps predictably to the existing TypeScript mock models (e.g., id, title/name, description, price, category, imageUrl, stock/inventory).

---

## Domain Models & Database Persistence

### 1. Domain Entity: `Product`

```csharp
namespace ProductService.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
```

### 2. Database Schema (SQLite DDL)

```sql
CREATE TABLE IF NOT EXISTS "Products" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Sku" TEXT NOT NULL,
    "Price" TEXT NOT NULL,          -- EF Core SQLite stores decimal as TEXT or REAL
    "Category" TEXT NOT NULL,
    "ImageUrl" TEXT NULL,
    "StockQuantity" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CreatedAtUtc" TEXT NOT NULL,
    "UpdatedAtUtc" TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Products_Sku" ON "Products" ("Sku");
CREATE INDEX IF NOT EXISTS "IX_Products_Category" ON "Products" ("Category");
```

### 3. EF Core Context & Configuration

```csharp
namespace ProductService.Infrastructure.Persistence;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.Sku).IsRequired().HasMaxLength(100);
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Category).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.StockQuantity).IsRequired();
            entity.Property(p => p.CreatedAtUtc).IsRequired();
        });
    }
}
```

---

## Architecture & Logic Boundaries (CQRS)

The application separates read and write pipelines using standard CQRS interfaces via `MediatR`.

```
[ HTTP Controller / Minimal API ]
       |                  |
   (Command)           (Query)
       v                  v
[Command Handler]   [Query Handler]
       |                  |
 (Write Context)     (AsNoTracking Context)
       \                  /
        v                v
         [ SQLite Database ]
```

### Commands (Write Operations)
1. **`CreateProductCommand`**
   - **Payload**: `Name`, `Description`, `Sku`, `Price`, `Category`, `ImageUrl`, `StockQuantity`
   - **Handler Logic**: Validates input, ensures SKU uniqueness, sets `CreatedAtUtc = DateTime.UtcNow`, persists via EF Core, returns created `ProductResponseDto`.
2. **`UpdateProductCommand`**
   - **Payload**: `Id`, `Name`, `Description`, `Sku`, `Price`, `Category`, `ImageUrl`, `StockQuantity`, `IsActive`
   - **Handler Logic**: Fetches product by `Id`, returns `NotFound` if non-existent, validates unique SKU collision with other products, updates fields and `UpdatedAtUtc = DateTime.UtcNow`, saves changes.
3. **`DeleteProductCommand`**
   - **Payload**: `Id`
   - **Handler Logic**: Performs hard deletion or sets `IsActive = false` (soft deletion). Default: hard deletion unless specified. Returns success confirmation.

### Queries (Read Operations)
1. **`GetProductsQuery`**
   - **Parameters**: `string? Category`, `string? SearchTerm`, `int Page = 1`, `int PageSize = 20`
   - **Handler Logic**: Queries `ProductDbContext.Products.AsNoTracking()`, applies filter predicates and pagination, returns `PagedList<ProductResponseDto>`.
2. **`GetProductByIdQuery`**
   - **Parameters**: `Guid Id`
   - **Handler Logic**: Queries `ProductDbContext.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id)`, returns single `ProductResponseDto` or `null`.

---

## API Endpoints & Request/Response Contracts

### Base URL: `/api/v1/products`

| HTTP Method | Endpoint | Description | Status Codes |
|---|---|---|---|
| `GET` | `/api/v1/products` | Retrieve paginated products with filters | `200 OK` |
| `GET` | `/api/v1/products/{id}` | Retrieve a product by ID | `200 OK`, `404 Not Found` |
| `POST` | `/api/v1/products` | Create a new product | `201 Created`, `400 Bad Request`, `409 Conflict` |
| `PUT` | `/api/v1/products/{id}` | Update an existing product | `200 OK`, `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| `DELETE` | `/api/v1/products/{id}` | Delete a product | `204 No Content`, `404 Not Found` |

### JSON Data Contracts

#### 1. `GET /api/v1/products`
**Query Parameters**:
- `category` (optional, string)
- `searchTerm` (optional, string)
- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 20)

**Response `200 OK`**:
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Wireless Ergonomic Mouse",
      "description": "2.4GHz ultra-fast ergonomic optical mouse",
      "sku": "TECH-MOU-001",
      "price": 49.99,
      "category": "Electronics",
      "imageUrl": "https://cdn.example.com/products/mouse.png",
      "stockQuantity": 150,
      "isActive": true,
      "createdAtUtc": "2025-01-15T08:30:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

#### 2. `POST /api/v1/products`
**Request Body**:
```json
{
  "name": "Mechanical Keyboard",
  "description": "RGB mechanical keyboard with red switches",
  "sku": "TECH-KEY-002",
  "price": 89.99,
  "category": "Electronics",
  "imageUrl": "https://cdn.example.com/products/keyboard.png",
  "stockQuantity": 80
}
```

**Response `201 Created`** (Headers: `Location: /api/v1/products/{new-guid}`):
```json
{
  "id": "e814d483-34e8-467f-801f-bf56c07efd93",
  "name": "Mechanical Keyboard",
  "description": "RGB mechanical keyboard with red switches",
  "sku": "TECH-KEY-002",
  "price": 89.99,
  "category": "Electronics",
  "imageUrl": "https://cdn.example.com/products/keyboard.png",
  "stockQuantity": 80,
  "isActive": true,
  "createdAtUtc": "2025-01-15T09:00:00Z",
  "updatedAtUtc": null
}
```

#### 3. Error Contract (`400 Bad Request` / `409 Conflict`)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "Price": ["Price must be greater than zero."],
    "Sku": ["SKU is required."]
  }
}
```

---

## Acceptance Criteria

### Scenario 1: Initial Database Initialization and Seeding
- **Given** an empty SQLite environment upon initial service startup,
- **When** the backend application starts up,
- **Then** EF Core must execute pending migrations or run `EnsureCreated()`, creating the SQLite database file and tables without manual intervention,
- **And** it must seed standard fallback catalog items if the `Products` table is empty.

### Scenario 2: Querying Products from Frontend (Happy Path)
- **Given** the database contains 5 products across "Electronics" and "Apparel" categories,
- **When** the frontend executes `GET /api/v1/products?category=Electronics`,
- **Then** the API responds with HTTP 200,
- **And** the JSON response returns only products where `category == "Electronics"`,
- **And** the response includes standard pagination metadata (`page`, `pageSize`, `totalCount`).

### Scenario 3: Creating a Product via CQRS Command
- **Given** a valid product payload with a unique SKU `"OFFICE-CHR-010"`,
- **When** a client sends a `POST /api/v1/products` request,
- **Then** the `CreateProductCommandHandler` persists the product into SQLite,
- **And** returns HTTP 201 with the created product payload (including generated `id` and `createdAtUtc`),
- **And** includes a `Location` header pointing to `/api/v1/products/{id}`.

### Scenario 4: Validation Failure on Create/Update
- **Given** a product payload with a negative price (`-12.50`) or missing `name`,
- **When** the client submits `POST /api/v1/products`,
- **Then** the command pipeline rejects the request before database commit,
- **And** returns HTTP 400 Bad Request with field-level validation messages.

### Scenario 5: Duplicate SKU Conflict
- **Given** an existing product in the database with SKU `"UNIQUE-SKU-001"`,
- **When** a client submits a new product with the same SKU `"UNIQUE-SKU-001"`,
- **Then** the API rejects the request with HTTP 409 Conflict,
- **And** does not persist any duplicate record into SQLite.

### Scenario 6: Product Retrieval by Non-Existent ID
- **Given** a product ID `00000000-0000-0000-0000-000000000000` that does not exist,
- **When** a client calls `GET /api/v1/products/00000000-0000-0000-0000-000000000000`,
- **Then** the query handler returns `null`,
- **And** the endpoint responds with HTTP 404 Not Found.

### Scenario 7: Cross-Origin Resource Sharing (CORS) for UI Integration
- **Given** the frontend client originates from `http://localhost:3000` or `http://localhost:5173`,
- **When** an `OPTIONS` preflight or `GET` request is sent to `/api/v1/products`,
- **Then** the backend must respond with the appropriate `Access-Control-Allow-Origin` and `Access-Control-Allow-Methods` headers,
- **And** allow the browser to successfully read the response without CORS violations.

---

## Quality Gate & Verification Boundaries

### 1. Verification Strategy
- **Unit Testing**:
  - Test CQRS Handlers (`CreateProductCommandHandler`, `GetProductsQueryHandler`, etc.) using an in-memory SQLite provider or EF Core In-Memory mock.
  - Verify validation logic (e.g., negative prices, empty titles, invalid SKU format).
- **Integration Testing**:
  - Test ASP.NET Core endpoints via `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>`.
  - Assert that real SQLite queries and constraints (e.g., unique index on `Sku`) are respected.
- **Frontend Contract Compatibility**:
  - Verify that the serialized JSON payload matches the TypeScript interface expected by the UI:
    ```typescript
    export interface Product {
      id: string;
      name: string;
      description: string;
      sku: string;
      price: number;
      category: string;
      imageUrl?: string;
      stockQuantity: number;
      isActive: boolean;
    }
    ```

### 2. Definition of Done (DoD)
- [ ] Backend solution created targeting .NET 8.
- [ ] `ProductDbContext` configured with `Microsoft.EntityFrameworkCore.Sqlite`.
- [ ] CQRS structure established (Queries, Commands, Handlers).
- [ ] RESTful API endpoints implemented and operational.
- [ ] CORS policies configured in `Program.cs` for local development frontend ports.
- [ ] SQLite database file creation verified on clean application boot.
- [ ] Automated integration test suite passes cleanly.