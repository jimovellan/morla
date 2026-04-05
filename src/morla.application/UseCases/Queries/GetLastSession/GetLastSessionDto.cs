namespace Morla.Application.UseCases.Queries.GetLastSession;

public record GetLastSessionDto(
    string RowId,
    string? Topic,
    string Title,
    string? Project,
    string Summary,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
