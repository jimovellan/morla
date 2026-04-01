namespace Morla.Application.UseCases.Queries.SearchKnowledge;

public record SearchKnowledgeDto(string RowId, string Title, string Summary, string? Topic, int RelevanceScore = 0);  // ✅ Topic nullable
