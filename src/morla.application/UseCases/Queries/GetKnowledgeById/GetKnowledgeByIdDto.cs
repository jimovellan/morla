namespace Morla.Application.UseCases.Queries.GetKnowledgeById;

public record GetKnowledgeByIdDto(
    string RowId,      // ✅ Expone solo el RowId (GUID), nunca el Id interno (long)
    string? Topic,
    string Title,
    string? Project,
    string Summary,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
