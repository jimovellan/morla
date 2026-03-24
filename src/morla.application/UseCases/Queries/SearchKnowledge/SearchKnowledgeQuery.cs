using MediatR;

namespace Morla.Application.UseCases.Queries.SearchKnowledge;

public record SearchKnowledgeQuery(string SearchTerm) : IRequest<List<SearchKnowledgeDto>>;
