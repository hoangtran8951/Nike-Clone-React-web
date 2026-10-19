using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Infrastructure.Persistence;

namespace ProductCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=products.db";

        services.AddDbContext<ProductDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IProductDbContext>(provider =>
            provider.GetRequiredService<ProductDbContext>());

        services.AddScoped<ProductDbInitializer>();

        return services;
    }
}