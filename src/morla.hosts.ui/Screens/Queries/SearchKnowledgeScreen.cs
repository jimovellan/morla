using MediatR;
using Morla.Application.UseCases.Queries.SearchKnowledge;
using Morla.Hosts.UI.Config;
using Morla.Hosts.UI.Services;
using Spectre.Console;

namespace Morla.Hosts.UI.Screens.Queries;

/// <summary>
/// Pantalla para buscar conocimiento con filtros opcionales
/// Muestra resultados en tabla seleccionable
/// </summary>
public class SearchKnowledgeScreen : IScreen
{
    public string ScreenName => "Search Knowledge";

    public async Task ShowAsync(IMediator mediator, IScreenService screenService, NavigationStack navigationStack)
    {
        // Solicitar parámetros de búsqueda
        await screenService.ClearScreenAsync();
        await screenService.DisplayInfoAsync("Search Knowledge (Press ESC to cancel)");

        var searchTerm = await screenService.PromptTextInputAsync("Search term (leave empty for all):");
        if (searchTerm == null)
        {
            // ESC presionado - volver al menú
            return;
        }

        var topic = await screenService.PromptTextInputAsync("Topic filter (optional, press ENTER to skip):");
        if (topic == null)
        {
            // ESC presionado - volver al menú
            return;
        }

        var project = await screenService.PromptTextInputAsync("Project filter (optional, press ENTER to skip):");
        if (project == null)
        {
            // ESC presionado - volver al menú
            return;
        }

        // Ejecutar query
        var query = new SearchKnowledgeQuery(
            SearchTerm: string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
            Topic: string.IsNullOrWhiteSpace(topic) ? null : topic,
            Project: string.IsNullOrWhiteSpace(project) ? null : project,
            Limit: UIConfig.Defaults.SearchResultsLimit
        );

        var results = await mediator.Send(query);

        if (!results.Any())
        {
            await screenService.DisplayInfoAsync(UIConfig.Messages.NoResults);
            await screenService.PressKeyToContinueAsync();
            return;
        }

        // Mostrar lista seleccionable de resultados
        var selected = await screenService.DisplaySelectableTableAsync(
            "Search Results",
            new[] { "Summary", "Topic" },
            results,
            result => new[]
            {
                result.Summary.Length > 80 ? result.Summary.Substring(0, 80) + "..." : result.Summary,
                result.Topic ?? "—"
            }
        );

        if (selected != null)
        {
            // Navegar a DetailedViewScreen
            navigationStack.Push(new DetailedViewScreen(selected.RowId, "Knowledge"));
            await navigationStack.Current!.ShowAsync(mediator, screenService, navigationStack);
            navigationStack.GoBack();
        }
    }
}

