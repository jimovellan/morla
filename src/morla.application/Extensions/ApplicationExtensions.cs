using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Morla.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // =====================
        // REGISTER MEDIATR
        // =====================
        // Escanea el assembly de Application y registra todos los handlers automáticamente
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationExtensions).Assembly));

        return services;
    }
}
