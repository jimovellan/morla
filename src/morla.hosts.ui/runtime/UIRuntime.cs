using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Morla.Hosts.UI.Extensions;
using Morla.Hosts.UI.Services;
using Morla.Hosts.UI.Screens.Queries;
using Serilog;

namespace Morla.Hosts.UI.Runtime;

/// <summary>
/// Runtime de la interfaz UI de Morla
/// Se carga desde el CLI principal (morla.hosts)
/// </summary>
public class UIRuntime
{
    private readonly IServiceProvider? _serviceProvider;

    public UIRuntime()
    {
        // Configurar Serilog para UI
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        // Configurar DI
        var services = new ServiceCollection();
        services.AddUIServices();
        services.AddLogging(builder =>
        {
            builder.AddSerilog();
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Inicia la interfaz UI interactiva
    /// </summary>
    public async Task ShowAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Servicios no configurados");

            // Obtener servicios
            var mediator = _serviceProvider.GetRequiredService<IMediator>();
            var screenService = _serviceProvider.GetRequiredService<IScreenService>();
            var navigationStack = _serviceProvider.GetRequiredService<NavigationStack>();

            // Inicializar stack de navegación con MainMenuScreen
            navigationStack.Push(new MainMenuScreen());

            // Ejecutar la aplicación
            while (navigationStack.Current != null)
            {
                try
                {
                    await navigationStack.Current.ShowAsync(mediator, screenService, navigationStack);

                    if (!navigationStack.GoBack())
                        break;
                }
                catch (OperationCanceledException)
                {
                    await screenService.DisplayInfoAsync("Cancelled");
                    if (!navigationStack.GoBack())
                        break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error en pantalla {Screen}", navigationStack.Current?.ScreenName);
                    await screenService.DisplayErrorAsync($"Error: {ex.Message}");
                    await screenService.PressKeyToContinueAsync();
                    if (!navigationStack.GoBack())
                        break;
                }
            }

            await screenService.DisplayInfoAsync("Goodbye!");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error in UI Runtime");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
