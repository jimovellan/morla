using MediatR;

namespace Morla.Application.UseCases.Queries.GetSessionById;

/// <summary>
/// Query para obtener una sesión específica por su identificador único (RowId).
/// 
/// Esta query especializa la obtención de knowledge entries para el caso específico de sesiones.
/// Recomendado para escenarios donde sesiones puedan evolucionar independientemente del conocimiento general.
/// </summary>
/// <param name="RowId">Identificador único de la sesión a recuperar (GUID en formato string).</param>
public record GetSessionByIdQuery(string RowId) : IRequest<GetSessionByIdDto>;
