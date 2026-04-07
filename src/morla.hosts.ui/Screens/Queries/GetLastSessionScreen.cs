using MediatR;
using Morla.Application.UseCases.Queries.GetLastSession;
using Morla.Hosts.UI.Services;

namespace Morla.Hosts.UI.Screens.Queries;

/// <summary>
/// Pantalla para mostrar la última sesión más reciente
/// Muestra detalles completos en un panel
/// </summary>
public class GetLastSessionScreen : IScreen
{
    public string ScreenName => "Last Session";

    public async Task ShowAsync(IMediator mediator, IScreenService screenService, NavigationStack navigationStack)
    {
        await screenService.ClearScreenAsync();
        await screenService.DisplayInfoAsync("Loading last session...");

        // Ejecutar query
        var query = new GetLastSessionQuery();
        var session = await mediator.Send(query);

        if (session == null)
        {
            await screenService.DisplayInfoAsync("No sessions found");
            await screenService.PressKeyToContinueAsync();
            return;
        }

        // Mostrar detalles en panel
        var details = $@"Title: {session.Title}
Topic: {session.Topic ?? "N/A"}
Project: {session.Project ?? "N/A"}
Summary: {session.Summary}
Created: {session.CreatedAt:g}
Updated: {session.UpdatedAt:g}

Content:
{session.Content}";

        await screenService.DisplayPanelAsync("Last Session Details", details);
        await screenService.PressKeyToContinueAsync();
    }
}
