using MediatR;
using Morla.Application.UseCases.Queries.GetLatestSessions;
using Morla.Hosts.UI.Config;
using Morla.Hosts.UI.Services;

namespace Morla.Hosts.UI.Screens.Queries;

/// <summary>
/// Pantalla para mostrar las últimas sesiones registradas en una tabla seleccionable
/// </summary>
public class GetLatestSessionsScreen : IScreen
{
    public string ScreenName => "Latest Sessions";

    public async Task ShowAsync(IMediator mediator, IScreenService screenService, NavigationStack navigationStack)
    {
        await screenService.DisplayInfoAsync("Loading latest sessions...");

        // Ejecutar query
        var query = new GetLatestSessionsQuery(Limit: UIConfig.Defaults.SessionsListLimit);
        var sessions = await mediator.Send(query);

        if (!sessions.Any())
        {
            await screenService.DisplayInfoAsync(UIConfig.Messages.NoResults);
            await screenService.PressKeyToContinueAsync();
            return;
        }

        // Mostrar lista seleccionable
        var selected = await screenService.DisplaySelectableTableAsync(
            "Latest Sessions",
            new[] { "Summary", "Project", "Topic", "Created" },
            sessions,
            session => new[]
            {
                session.Summary?.Length > 50 ? session.Summary.Substring(0, 50) + "..." : (session.Summary ?? "—"),
                session.Project ?? "—",
                session.Topic ?? "—",
                session.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            }
        );

        if (selected != null)
        {
            // Navegar a DetailedViewScreen
            navigationStack.Push(new DetailedViewScreen(selected.RowId, "Session"));
            await navigationStack.Current!.ShowAsync(mediator, screenService, navigationStack);
            navigationStack.GoBack();
        }
    }
}

