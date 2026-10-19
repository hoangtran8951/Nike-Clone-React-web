using ProductCatalog.Api.Middleware;
using ProductCatalog.Application;
using ProductCatalog.Infrastructure;
using ProductCatalog.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register Core Application and Infrastructure services (Clean Architecture & CQRS)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add API Controllers and JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure CORS for React frontend (Vite & Create-React-App default ports)
const string corsPolicyName = "ReactAppCorsPolicy";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://127.0.0.1:3000", "http://127.0.0.1:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Swagger / OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Product Catalog API",
        Version = "v1",
        Description = ".NET 8 Web API with CQRS and SQLite Entity Framework Core for React Frontend"
    });
});

var app = builder.Build();

// Run Database Migrations and Seeding on startup
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ProductDbInitializer>();
    await initializer.InitializeAsync();
}

// Global Exception Handler Middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Catalog API v1");
    });
}

app.UseCors(corsPolicyName);

app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible to integration test WebApplicationFactory
public partial class Program { }