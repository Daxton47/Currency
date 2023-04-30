using CurrencyApp.EFCore.Context;
using CurrencyApp.EFCore.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyApp.EFCore.Feature;

/// <summary>
/// Add EF Core related services to DI Container
/// </summary>
public static class EntityFrameworkCoreFeature
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddEntityFrameworkSqlite().AddDbContext<CurrencyDbContext>();
        services.AddScoped<CurrencyStore>();

        return services;
    }
}
