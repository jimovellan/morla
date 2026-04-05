using MediatR;

namespace Morla.Application.UseCases.Queries.GetLatestSessions;

/// <summary>
/// Query para obtener las N últimas sesiones
/// </summary>
/// <param name="Limit">Número máximo de sesiones a retornar (default: 3)</param>
/// <param name="Project">Filtro por proyecto (opcional)</param>
public record GetLatestSessionsQuery(int Limit = 3, string? Project = null) : IRequest<List<GetLatestSessionDto>>;
