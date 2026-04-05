using MediatR;
using Morla.Domain.Repository;
using Serilog;

namespace Morla.Application.UseCases.Queries.GetLastSession;

public class GetLastSessionQueryHandler : IRequestHandler<GetLastSessionQuery, GetLastSessionDto?>
{
    private readonly IKnowledgeRepository _knowledgeRepository;

    public GetLastSessionQueryHandler(IKnowledgeRepository knowledgeRepository)
    {
        _knowledgeRepository = knowledgeRepository;
    }

    public async Task<GetLastSessionDto?> Handle(GetLastSessionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("GetLastSessionQueryHandler.Handle: Iniciando búsqueda de última sesión...");
            Log.Debug("  - Project: {Project}", request.Project ?? "null");
            
            var lastSession = await _knowledgeRepository.GetLastSessionAsync(request.Project);
            
            if (lastSession == null)
            {
                Log.Information("GetLastSessionQueryHandler.Handle: No se encontraron sesiones");
                return null;
            }

            var dto = new GetLastSessionDto(
                lastSession.RowId,
                lastSession.Topic,
                lastSession.Title,
                lastSession.Project,
                lastSession.Summary,
                lastSession.Content,
                lastSession.CreatedAt,
                lastSession.UpdatedAt
            );
            
            Log.Information("GetLastSessionQueryHandler.Handle: ✅ Sesión obtenida. RowId: {RowId}", dto.RowId);
            return dto;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetLastSessionQueryHandler.Handle: Error al obtener última sesión");
            throw;
        }
    }
}
