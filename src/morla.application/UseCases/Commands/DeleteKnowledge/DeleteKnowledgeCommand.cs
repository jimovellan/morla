using MediatR;

namespace Morla.Application.UseCases.Commands.DeleteKnowledge;

public record DeleteKnowledgeCommand(string RowKey) : IRequest<DeleteKnowledgeResponse>;

public record DeleteKnowledgeResponse(
    bool Success,
    string Message,
    string? DeletedRowKey = null,
    string? DeletedId = null,
    string? Error = null);
