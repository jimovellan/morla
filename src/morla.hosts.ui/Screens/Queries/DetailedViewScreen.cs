using MediatR;
using Morla.Application.UseCases.Queries.GetKnowledgeById;
using Morla.Application.UseCases.Queries.GetSessionById;
using Morla.Hosts.UI.Services;

namespace Morla.Hosts.UI.Screens.Queries;

/// <summary>
/// Pantalla genérica para mostrar detalles de Knowledge o Session
/// Recibe el tipo (Knowledge/Session) y RowId para cargar los detalles
/// </summary>
public class DetailedViewScreen : IScreen
{
    private readonly string _rowId;
    private readonly string _type; // "Knowledge" o "Session"

    public DetailedViewScreen(string rowId, string type)
    {
        _rowId = rowId;
        _type = type;
    }

    public string ScreenName => $"{_type} Details";

    public async Task ShowAsync(IMediator mediator, IScreenService screenService, NavigationStack navigationStack)
    {
        await screenService.ClearScreenAsync();

        try
        {
            string details;

            if (_type == "Knowledge")
            {
                var query = new GetKnowledgeByIdQuery(_rowId);
                var knowledge = await mediator.Send(query);

                details = $@"Title: {knowledge.Title}
Topic: {knowledge.Topic ?? "N/A"}
Project: {knowledge.Project ?? "N/A"}
Summary: {knowledge.Summary}
Created: {knowledge.CreatedAt:g}
Updated: {knowledge.UpdatedAt:g}

Content:
{knowledge.Content}";
            }
            else if (_type == "Session")
            {
                var query = new GetSessionByIdQuery(_rowId);
                var session = await mediator.Send(query);

                if (session == null)
                {
                    await screenService.DisplayErrorAsync("Session not found");
                    await screenService.PressKeyToContinueAsync();
                    return;
                }

                details = $@"Title: {session.Title}
Topic: {session.Topic ?? "N/A"}
Project: {session.Project ?? "N/A"}
Summary: {session.Summary}
Created: {session.CreatedAt:g}
Updated: {session.UpdatedAt:g}

Content:
{session.Content}";
            }
            else
            {
                await screenService.DisplayErrorAsync($"Unknown type: {_type}");
                await screenService.PressKeyToContinueAsync();
                return;
            }

            await screenService.DisplayPanelAsync($"{_type} Details", details);
            await screenService.PressKeyToContinueAsync();
        }
        catch (Exception ex)
        {
            await screenService.DisplayErrorAsync($"Error loading {_type}: {ex.Message}");
            await screenService.PressKeyToContinueAsync();
        }
    }
}
