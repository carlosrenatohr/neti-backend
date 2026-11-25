using MarcasAutos.Application.Abstractions;
using MarcasAutos.Infrastructure.Persistence;
using MarcasAutos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarcasAutos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // -- Fail fast at startup when the connection string is missing --
        var connectionString = configuration.GetConnectionString("MarcasAutos")
            ?? throw new InvalidOperationException(
                "Connection string 'MarcasAutos' is not configured. Set ConnectionStrings__MarcasAutos.");

        services.AddDbContext<MarcasAutosDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IMarcasAutosRepository, MarcasAutosRepository>();

        return services;
    }
}
