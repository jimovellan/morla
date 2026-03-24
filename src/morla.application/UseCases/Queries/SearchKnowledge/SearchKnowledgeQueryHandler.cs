using MediatR;
using Morla.Domain.Repository;

namespace Morla.Application.UseCases.Queries.SearchKnowledge;

public class SearchKnowledgeQueryHandler : IRequestHandler<SearchKnowledgeQuery, List<SearchKnowledgeDto>>
{
    private readonly IKnowledgeRepository _knowledgeRepository;

    public SearchKnowledgeQueryHandler(IKnowledgeRepository knowledgeRepository)
    {
        _knowledgeRepository = knowledgeRepository;
    }

    public async Task<List<SearchKnowledgeDto>> Handle(SearchKnowledgeQuery request, CancellationToken cancellationToken)
    {
        var results = await _knowledgeRepository.SearchAsync(request.SearchTerm);
        return results.Select(k => new SearchKnowledgeDto(k.Id, k.Title, k.Summary)).ToList();
    }
}
