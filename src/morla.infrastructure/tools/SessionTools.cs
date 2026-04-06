using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Server;
using Morla.Application.UseCases.Commands.SaveSesion;
using Morla.Application.UseCases.Queries.GetSessionById;
using Morla.Application.UseCases.Queries.GetLastSession;
using Morla.Application.UseCases.Queries.GetLatestSessions;
using Morla.Domain.Repository;
using Serilog;

namespace morla.infrastructure.tools;

[McpServerToolType]
public class SessionTools
{
    private readonly ISender _sender;
    private readonly IKnowledgeRepository _knowledgeRepository;

    public SessionTools(ISender sender, IKnowledgeRepository knowledgeRepository)
    {
        _sender = sender;
        _knowledgeRepository = knowledgeRepository;
    }

    [McpServerTool, Description("Tool to get the last (most recent) session")]
    public async Task<GetLastSessionDto?> GetLastSession(string? project = null)
    {
        try
        {
            Log.Information("SessionTools.GetLastSession: Obteniendo última sesión...");
            Log.Debug("  - Project: {Project}", project ?? "null");
            
            var result = await _sender.Send(new GetLastSessionQuery(project));
            
            if (result == null)
            {
                Log.Information("SessionTools.GetLastSession: No se encontraron sesiones");
                return null;
            }
            
            Log.Information("SessionTools.GetLastSession: ✅ Sesión obtenida. RowId: {RowId}", result.RowId);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SessionTools.GetLastSession: Error al obtener última sesión");
            throw;
        }
    }

    [McpServerTool, Description("Tool to get the latest sessions ordered by creation date")]
    public async Task<List<GetLatestSessionDto>> GetLatestSessions(int limit = 3, string? project = null)
    {
        try
        {
            Log.Information("SessionTools.GetLatestSessions: Obteniendo últimas sesiones...");
            Log.Debug("  - Limit: {Limit}, Project: {Project}", limit, project ?? "null");
            
            var result = await _sender.Send(new GetLatestSessionsQuery(limit, project));
            
            Log.Information("SessionTools.GetLatestSessions: ✅ Sesiones obtenidas. Count: {Count}", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SessionTools.GetLatestSessions: Error al obtener últimas sesiones");
            throw;
        }
    }

    /// <summary>
    /// Obtiene una sesión específica por su identificador único (RowId).
    /// </summary>
    /// <remarks>
    /// Wrapper que especializa GetKnowledgeByIdQuery para sesiones, validando que el elemento recuperado
    /// sea efectivamente una sesión (Topic == "session"). Permite evolucionar la lógica de sesiones
    /// independientemente de la gestión general de conocimiento.
    /// </remarks>
    [McpServerTool, Description("Tool to get a session by RowId")]
    public async Task<GetSessionByIdDto> GetSessionByRowId(string rowId)
    {
        try
        {
            Log.Information("SessionTools.GetSessionByRowId: Obteniendo sesión con RowId {RowId}", rowId);
            
            var result = await _sender.Send(new GetSessionByIdQuery(rowId));
            
            Log.Information("SessionTools.GetSessionByRowId: ✅ Sesión obtenida exitosamente. RowId: {RowId}", rowId);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SessionTools.GetSessionByRowId: Error al obtener sesión con RowId {RowId}", rowId);
            throw;
        }
    }

    [McpServerTool, Description("Tool to save a current session")]
    public async Task<string> SaveSession(string project, string topic, string title, string summary, string content)
    {
        try
        {
            Log.Information("SessionTools.SaveSession: Guardando sesión...");
            Log.Debug("  - Project: {Project}, Topic: {Topic}, Title: {Title}", project, topic, title);
            
            var result = await _sender.Send(new CreateSesionCommand(content, summary, project, content));
            
            Log.Information("SessionTools.SaveSession: ✅ Sesión guardada exitosamente. RowId: {RowId}", result);
            return $"Session saved successfully with ID: {result}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SessionTools.SaveSession: Error al guardar sesión");
            throw;
        }
    }
}
