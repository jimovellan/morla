using MediatR;
using Morla.Domain.Repository;
using Serilog;

namespace Morla.Application.UseCases.Commands.UpdateKnowledge;

public class UpdateKnowledgeCommandHandler : IRequestHandler<UpdateKnowledgeCommand, UpdateKnowledgeDto>
{
    private readonly IKnowledgeRepository _knowledgeRepository;

    public UpdateKnowledgeCommandHandler(IKnowledgeRepository knowledgeRepository)
    {
        _knowledgeRepository = knowledgeRepository;
    }

    public async Task<UpdateKnowledgeDto> Handle(UpdateKnowledgeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("UpdateKnowledgeCommandHandler.Handle: Actualizando entrada con RowId {RowId}", request.RowId);
            Log.Debug("  - Content length: {ContentLength} caracteres", request.Content.Length);
            
            var knowledge = await _knowledgeRepository.GetByIdAsync(request.RowId);
            
            if (knowledge == null)
            {
                Log.Warning("UpdateKnowledgeCommandHandler.Handle: Entrada no encontrada con RowId {RowId}", request.RowId);
                throw new InvalidOperationException($"Knowledge entry with ID '{request.RowId}' not found.");
            }
            knowledge.Summary = request.Resumen;
            knowledge.Content = request.Content;
            knowledge.UpdatedAt = DateTime.UtcNow;

            Log.Information("UpdateKnowledgeCommandHandler.Handle: Guardando cambios...");
            await _knowledgeRepository.UpdateAsync(knowledge);
            
            Log.Information("UpdateKnowledgeCommandHandler.Handle: Entrada actualizada exitosamente");

            return new UpdateKnowledgeDto(
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
            Log.Error(ex, "UpdateKnowledgeCommandHandler.Handle: Error al actualizar entrada con RowId {RowId}", request.RowId);
            throw;
        }
    }
}
