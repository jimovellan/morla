using MediatR;
using Morla.Domain.Repository;
using Serilog;

namespace Morla.Application.UseCases.Queries.GetKnowledgeById;

public class GetKnowledgeByIdQueryHandler : IRequestHandler<GetKnowledgeByIdQuery, GetKnowledgeByIdDto>
{
    private readonly IKnowledgeRepository _knowledgeRepository;

    public GetKnowledgeByIdQueryHandler(IKnowledgeRepository knowledgeRepository)
    {
        _knowledgeRepository = knowledgeRepository;
    }

    public async Task<GetKnowledgeByIdDto> Handle(GetKnowledgeByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("GetKnowledgeByIdQueryHandler.Handle: Obteniendo entrada con RowId {RowId}", request.RowId);
            
            var knowledge = await _knowledgeRepository.GetByIdAsync(request.RowId);
            
            if (knowledge == null)
            {
                Log.Warning("GetKnowledgeByIdQueryHandler.Handle: Entrada no encontrada con RowId {RowId}", request.RowId);
                throw new InvalidOperationException($"Knowledge entry with ID '{request.RowId}' not found.");
            }

            Log.Information("GetKnowledgeByIdQueryHandler.Handle: Entrada obtenida exitosamente");

            return new GetKnowledgeByIdDto(
                knowledge.RowId,   // ✅ Expone solo RowId, nunca Id interno
                knowledge.Topic,
                knowledge.Title,
                knowledge.Project,
                knowledge.Summary,
                knowledge.Content,
                knowledge.CreatedAt,
                knowledge.UpdatedAt
            );
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetKnowledgeByIdQueryHandler.Handle: Error al obtener entrada con RowId {RowId}", request.RowId);
            throw;
        }
    }
}
