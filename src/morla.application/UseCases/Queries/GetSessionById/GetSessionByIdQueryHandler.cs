using MediatR;
using morla.domain.constants;
using Morla.Domain.Repository;
using Serilog;

namespace Morla.Application.UseCases.Queries.GetSessionById;

/// <summary>
/// Handler para la query GetSessionByIdQuery.
/// Obtiene una sesión específica del repositorio y la proyecta al DTO correspondiente.
/// Valida que el elemento recuperado sea efectivamente una sesión (Topic == "session").
/// </summary>
public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, GetSessionByIdDto>
{
    private readonly IKnowledgeRepository _knowledgeRepository;

    /// <summary>
    /// Constructor que inyecta las dependencias necesarias.
    /// </summary>
    /// <param name="knowledgeRepository">Repositorio de acceso a las entradas de conocimiento/sesiones.</param>
    public GetSessionByIdQueryHandler(IKnowledgeRepository knowledgeRepository)
    {
        _knowledgeRepository = knowledgeRepository;
    }

    /// <summary>
    /// Ejecuta la lógica de obtención de sesión por RowId.
    /// </summary>
    /// <param name="request">Query con el RowId de la sesión a recuperar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>DTO con los datos de la sesión.</returns>
    /// <exception cref="InvalidOperationException">Se lanza si:
    /// - La sesión no existe.
    /// - El element existe pero no es una sesión (Topic != "session").
    /// </exception>
    public async Task<GetSessionByIdDto> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("GetSessionByIdQueryHandler.Handle: Obteniendo sesión con RowId {RowId}", request.RowId);
            
            var knowledge = await _knowledgeRepository.GetByIdAsync(request.RowId);
            
            if (knowledge == null)
            {
                Log.Warning("GetSessionByIdQueryHandler.Handle: Sesión no encontrada con RowId {RowId}", request.RowId);
                throw new InvalidOperationException($"Session with ID '{request.RowId}' not found.");
            }

            // Validar que el elemento es efectivamente una sesión
            if (knowledge.Topic != TopicNames.SESSION_TOPIC)
            {
                Log.Warning(
                    "GetSessionByIdQueryHandler.Handle: El elemento con RowId {RowId} no es una sesión. Topic: {Topic}",
                    request.RowId,
                    knowledge.Topic);
                throw new InvalidOperationException(
                    $"Element with ID '{request.RowId}' is not a session (Topic: '{knowledge.Topic}'). Expected Topic: '{TopicNames.SESSION_TOPIC}'.");
            }

            Log.Information("GetSessionByIdQueryHandler.Handle: Sesión obtenida exitosamente. RowId: {RowId}", request.RowId);

            return new GetSessionByIdDto(
                knowledge.RowId,
                knowledge.Topic,
                knowledge.Title,
                knowledge.Project,
                knowledge.Summary,
                knowledge.Content,
                knowledge.CreatedAt,
                knowledge.UpdatedAt
            );
        }
        catch (Exception ex)
        {
            Log.Error(ex, "GetSessionByIdQueryHandler.Handle: Error al obtener sesión con RowId {RowId}", request.RowId);
            throw;
        }
    }
}
