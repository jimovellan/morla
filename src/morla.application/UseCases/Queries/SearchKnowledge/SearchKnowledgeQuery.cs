using MediatR;

namespace Morla.Application.UseCases.Queries.SearchKnowledge;

public record SearchKnowledgeQuery(
    string? SearchTerm = null, 
    string? Topic = null, 
    string? Project = null
) : IRequest<List<SearchKnowledgeDto>>;
