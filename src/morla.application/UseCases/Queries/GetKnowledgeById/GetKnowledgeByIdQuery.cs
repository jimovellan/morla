using MediatR;

namespace Morla.Application.UseCases.Queries.GetKnowledgeById;

public record GetKnowledgeByIdQuery(string RowId) : IRequest<GetKnowledgeByIdDto>;  // ✅ Usa RowId (string GUID)
