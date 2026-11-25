using MarcasAutos.Application.Abstractions;
using MarcasAutos.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MarcasAutos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMarcasAutosService, MarcasAutosService>();

        return services;
    }
}
