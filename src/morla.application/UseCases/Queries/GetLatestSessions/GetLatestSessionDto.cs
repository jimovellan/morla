namespace Morla.Application.UseCases.Queries.GetLatestSessions;

public record GetLatestSessionDto(
    string RowId,
    string? Topic,
    string Title,
    string? Project,
    string Summary,
    DateTime CreatedAt
);
