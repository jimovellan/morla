using MediatR;
using Morla.Domain.Repository;
using Serilog;

namespace Morla.Application.UseCases.Commands.DeleteKnowledge;

public class DeleteKnowledgeCommandHandler : IRequestHandler<DeleteKnowledgeCommand, DeleteKnowledgeResponse>
{
    private readonly IKnowledgeRepository _repository;

    public DeleteKnowledgeCommandHandler(IKnowledgeRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteKnowledgeResponse> Handle(
        DeleteKnowledgeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var operationType = request.HardDelete ? "hard-delete" : "soft-delete";
            Log.Information("DeleteKnowledgeCommandHandler.Handle: Iniciando {OperationType} de conocimiento con RowKey {RowKey}", operationType, request.RowKey);

            var knowledge = await _repository.GetByIdAsync(request.RowKey);

            if (knowledge == null)
            {
                Log.Warning("DeleteKnowledgeCommandHandler.Handle: Conocimiento no encontrado {RowKey}", request.RowKey);
                return new DeleteKnowledgeResponse(
                    Success: false,
                    Message: $"Knowledge not found with rowKey: {request.RowKey}",
                    Error: "NOT_FOUND");
            }

            if (request.HardDelete)
            {
                // Hard-delete: permanent removal
                await _repository.DeleteAsync(request.RowKey);
                Log.Information("DeleteKnowledgeCommandHandler.Handle: Conocimiento eliminado permanentemente (hard-delete) {RowKey} - Title: {Title} - Project: {Project}", 
                    request.RowKey, knowledge.Title, knowledge.Project ?? "null");
                
                return new DeleteKnowledgeResponse(
                    Success: true,
                    Message: "Knowledge permanently deleted",
                    DeletedRowKey: knowledge.RowId,
                    DeletedId: knowledge.Id.ToString(),
                    IsSoftDelete: false);
            }
            else
            {
                // Soft-delete: mark as deleted (default behavior)
                knowledge.SoftDelete();
                await _repository.UpdateAsync(knowledge, updateEmbeddings: false);  // ✅ Skip embedding regeneration for soft-delete
                
                Log.Information("DeleteKnowledgeCommandHandler.Handle: Conocimiento marcado como eliminado (soft-delete) {RowKey} - Title: {Title} - Project: {Project} - DeletedAt: {DeletedAt}", 
                    request.RowKey, knowledge.Title, knowledge.Project ?? "null", knowledge.DeletedAt);
                
                return new DeleteKnowledgeResponse(
                    Success: true,
                    Message: "Knowledge marked as deleted (soft-delete)",
                    DeletedRowKey: knowledge.RowId,
                    DeletedId: knowledge.Id.ToString(),
                    IsSoftDelete: true);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DeleteKnowledgeCommandHandler.Handle: Error eliminando conocimiento {RowKey}", request.RowKey);
            return new DeleteKnowledgeResponse(
                Success: false,
                Message: "An error occurred while deleting knowledge",
                Error: "INTERNAL_ERROR");
        }
    }
}
