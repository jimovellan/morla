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
            Log.Information("DeleteKnowledgeCommandHandler.Handle: Eliminando conocimiento con RowKey {RowKey}", request.RowKey);

            var knowledge = await _repository.GetByIdAsync(request.RowKey);

            if (knowledge == null)
            {
                Log.Warning("DeleteKnowledgeCommandHandler.Handle: Conocimiento no encontrado {RowKey}", request.RowKey);
                return new DeleteKnowledgeResponse(
                    Success: false,
                    Message: $"Knowledge not found with rowKey: {request.RowKey}",
                    Error: "NOT_FOUND");
            }

            await _repository.DeleteAsync(request.RowKey);

            Log.Information("DeleteKnowledgeCommandHandler.Handle: Conocimiento eliminado exitosamente {RowKey}", request.RowKey);

            return new DeleteKnowledgeResponse(
                Success: true,
                Message: "Knowledge deleted successfully",
                DeletedRowKey: knowledge.RowId,
                DeletedId: knowledge.Id.ToString());
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
