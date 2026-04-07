using MediatR;
using Morla.Hosts.UI.Services;

namespace Morla.Hosts.UI.Screens;

/// <summary>
/// Interfaz base para todas las pantallas de la aplicación
/// Cada pantalla implementa esta interfaz para definir su comportamiento
/// </summary>
public interface IScreen
{
    /// <summary>
    /// Ejecuta la pantalla
    /// Recibe IMediator para despachar comandos/queries y IScreenService para UI
    /// </summary>
    Task ShowAsync(IMediator mediator, IScreenService screenService, NavigationStack navigationStack);

    /// <summary>
    /// Nombre de la pantalla (para debug/logging)
    /// </summary>
    string ScreenName { get; }
}
