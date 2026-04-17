using MediatR;
using Morla.Domain.Repository;
using Serilog;

namespace Morla.Application.UseCases.Commands.RestoreKnowledge;

public class RestoreKnowledgeCommandHandler : IRequestHandler<RestoreKnowledgeCommand, RestoreKnowledgeResponse>
{
    private readonly IKnowledgeRepository _repository;

    public RestoreKnowledgeCommandHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    public async Task<RestoreKnowledgeResponse> Handle(
        RestoreKnowledgeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("RestoreKnowledgeCommandHandler.Handle: Restaurando conocimiento con RowKey {RowKey}", request.RowKey);

            // Get the knowledge INCLUDING soft-deleted entries
            var knowledge = await _repository.GetByIdIncludingDeletedAsync(request.RowKey);

            if (knowledge == null)
            {
                Log.Warning("RestoreKnowledgeCommandHandler.Handle: Conocimiento no encontrado {RowKey}", request.RowKey);
                return new RestoreKnowledgeResponse(
                    Success: false,
                    Message: $"Knowledge not found with rowKey: {request.RowKey}",
                    Error: "NOT_FOUND");
            }

            // Check if the entry is actually soft-deleted
            if (!knowledge.IsDeleted)
            {
                Log.Warning("RestoreKnowledgeCommandHandler.Handle: Conocimiento no está marcado como eliminado {RowKey}", request.RowKey);
                return new RestoreKnowledgeResponse(
                    Success: false,
                    Message: $"Knowledge with rowKey {request.RowKey} is not soft-deleted",
                    Error: "NOT_DELETED");
            }

            // Restore the knowledge entry
            knowledge.Restore();
            await _repository.UpdateAsync(knowledge, updateEmbeddings: false);  // ✅ Skip embedding regeneration for restore

            Log.Information("RestoreKnowledgeCommandHandler.Handle: Conocimiento restaurado exitosamente {RowKey} - Title: {Title} - Project: {Project} - WasDeletedAt: {DeletedAt}", 
                request.RowKey, knowledge.Title, knowledge.Project ?? "null", knowledge.DeletedAt);

            return new RestoreKnowledgeResponse(
                Success: true,
                Message: "Knowledge restored successfully",
                RestoredRowKey: knowledge.RowId,
                RestoredId: knowledge.Id.ToString());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "RestoreKnowledgeCommandHandler.Handle: Error restaurando conocimiento {RowKey}", request.RowKey);
            return new RestoreKnowledgeResponse(
                Success: false,
                Message: "An error occurred while restoring knowledge",
                Error: "INTERNAL_ERROR");
        }
    }
}
