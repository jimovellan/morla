using MediatR;
using Morla.Domain.Repository;
using Serilog;

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
        try
        {
            Log.Information("SearchKnowledgeQueryHandler.Handle: Iniciando búsqueda...");
            Log.Debug("  - SearchTerm: {SearchTerm}, Topic: {Topic}, Project: {Project}", 
                request.SearchTerm ?? "null", request.Topic ?? "null", request.Project ?? "null");
            
            var results = await _knowledgeRepository.SearchAsync(request.SearchTerm, request.Topic, request.Project);
            
            var dtos = results
                .OrderByDescending(x => x.Score)
                .Select(k => new SearchKnowledgeDto(k.Knowledge.RowId, k.Knowledge.Title, k.Knowledge.Summary, k.Knowledge.Topic, k.Score))  // ✅ Usa RowId
                .ToList();
            
            Log.Information("SearchKnowledgeQueryHandler.Handle: Búsqueda completada. Resultados: {ResultCount}", dtos.Count);
            return dtos;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SearchKnowledgeQueryHandler.Handle: Error en la búsqueda");
            throw;
        }
    }
}
