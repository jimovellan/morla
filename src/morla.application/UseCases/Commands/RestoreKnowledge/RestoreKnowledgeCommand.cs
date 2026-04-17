using MediatR;

namespace Morla.Application.UseCases.Commands.RestoreKnowledge;

public record RestoreKnowledgeCommand(string RowKey) : IRequest<RestoreKnowledgeResponse>;

public record RestoreKnowledgeResponse(
    bool Success,
    string Message,
    string? RestoredRowKey = null,
    string? RestoredId = null,
    string? Error = null);
