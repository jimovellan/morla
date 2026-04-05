using MediatR;

namespace Morla.Application.UseCases.Queries.SearchKnowledge;

public record SearchKnowledgeQuery(
    string? SearchTerm = null, 
    string? Topic = null, 
    string? Project = null,
    int Limit = 5
) : IRequest<List<SearchKnowledgeDto>>;
