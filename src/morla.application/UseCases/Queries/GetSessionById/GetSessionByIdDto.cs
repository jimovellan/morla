namespace Morla.Application.UseCases.Queries.GetSessionById;

/// <summary>
/// DTO para obtener una sesión específica por su identificador único (RowId).
/// Especialización de GetKnowledgeByIdDto para sesiones, manteniendo la estructura completa de datos.
/// </summary>
public record GetSessionByIdDto(
    /// <summary>Identificador único de la sesión (GUID).</summary>
    string RowId,
    
    /// <summary>Tipo de tema asociado a la sesión (e.g. "session").</summary>
    string? Topic,
    
    /// <summary>Título o nombre de la sesión.</summary>
    string Title,
    
    /// <summary>Proyecto o contexto al que pertenece la sesión.</summary>
    string? Project,
    
    /// <summary>Resumen breve del contenido de la sesión.</summary>
    string Summary,
    
    /// <summary>Contenido completo de la sesión.</summary>
    string Content,
    
    /// <summary>Fecha y hora de creación de la sesión.</summary>
    DateTime CreatedAt,
    
    /// <summary>Fecha y hora de la última actualización de la sesión.</summary>
    DateTime UpdatedAt
);
