using MediatR;

namespace Morla.Application.UseCases.Commands.UpdateKnowledge;

public record UpdateKnowledgeCommand(
    string RowId,      // ✅ Usa RowId (string GUID), nunca el Id interno
    string Resumen,
    string Content
) : IRequest<UpdateKnowledgeDto>;
