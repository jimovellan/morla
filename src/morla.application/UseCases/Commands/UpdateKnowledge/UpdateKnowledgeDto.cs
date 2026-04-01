namespace Morla.Application.UseCases.Commands.UpdateKnowledge;

public record UpdateKnowledgeDto(
    string RowId,      // ✅ Expone solo RowId (GUID), nunca Id interno (long)
    string? Topic,
    string Title,
    string? Project,
    string Summary,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
