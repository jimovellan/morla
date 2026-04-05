using MediatR;
using Morla.Domain.Repository;
using Serilog;

namespace Morla.Application.UseCases.Queries.GetLatestSessions;

public class GetLatestSessionsQueryHandler : IRequestHandler<GetLatestSessionsQuery, List<GetLatestSessionDto>>
{
    private readonly IKnowledgeRepository _knowledgeRepository;

    public GetLatestSessionsQueryHandler(IKnowledgeRepository knowledgeRepository)
    {
        _knowledgeRepository = knowledgeRepository;
    }

    public async Task<List<GetLatestSessionDto>> Handle(GetLatestSessionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("GetLatestSessionsQueryHandler.Handle: Iniciando búsqueda de últimas sesiones...");
            Log.Debug("  - Limit: {Limit}, Project: {Project}", request.Limit, request.Project ?? "null");
            
            var latestSessions = await _knowledgeRepository.GetLatestSessionsAsync(request.Limit, request.Project);
            
            var dtos = latestSessions
                .Select(session => new GetLatestSessionDto(
                    session.RowId,
                    session.Topic,
                    session.Title,
                    session.Project,
                    session.Summary,
                    session.CreatedAt
                ))
                .ToList();
            
            Log.Information("GetLatestSessionsQueryHandler.Handle: ✅ Sesiones obtenidas. Count: {Count}", dtos.Count);
            return dtos;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetLatestSessionsQueryHandler.Handle: Error al obtener últimas sesiones");
            throw;
        }
    }
}
