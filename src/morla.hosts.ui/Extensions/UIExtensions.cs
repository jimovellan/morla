using Microsoft.Extensions.DependencyInjection;
using Morla.Application.Extensions;
using Morla.Infrastructure.Extensions;
using Morla.Hosts.UI.Services;

namespace Morla.Hosts.UI.Extensions;

/// <summary>
/// Extensiones para configuración de DI de la UI
/// </summary>
public static class UIExtensions
{
    /// <summary>
    /// Agrega servicios de UI al contenedor de DI
    /// </summary>
    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        // Agregar capas de aplicación e infraestructura
        services.AddApplicationServices();
        services.AddCoreServices();

        // Agregar servicios de UI
        services.AddSingleton<IScreenService, ScreenService>();
        services.AddSingleton<NavigationStack>();

        return services;
    }
}
