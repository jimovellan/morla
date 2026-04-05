using MediatR;

namespace Morla.Application.UseCases.Queries.GetLastSession;

/// <summary>
/// Query para obtener la última sesión (más reciente)
/// </summary>
/// <param name="Project">Filtro por proyecto (opcional)</param>
public record GetLastSessionQuery(string? Project = null) : IRequest<GetLastSessionDto?>;
