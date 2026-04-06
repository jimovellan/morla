using System.ComponentModel;
using System.Data.Common;
using MediatR;
using ModelContextProtocol.Server;
using Morla.Application.UseCases.Commands.CreateKnowledge;
using Morla.Application.UseCases.Commands.UpdateKnowledge;
using Morla.Application.UseCases.Queries.SearchKnowledge;
using Morla.Application.UseCases.Queries.GetKnowledgeById;
using Morla.Application.UseCases.Queries.GetLastSession;
using Morla.Application.UseCases.Queries.GetLatestSessions;
using Morla.Domain.Models;
using Morla.Domain.Repository;
using Serilog;
using Morla.Application.UseCases.Commands.SaveSesion;
using System.Linq.Expressions;

namespace morla.infrastructure.tools;

[McpServerToolType]
public class KnowledgeTools
{
    private readonly IKnowledgeRepository _knowledgeRepository;
    private readonly ISender _sender;

    public KnowledgeTools(ISender sender, IKnowledgeRepository knowledgeRepository)
    {
        _sender = sender;
        _knowledgeRepository = knowledgeRepository;
    }

    [McpServerTool, Description("Tool to create a new knowledge entry with topic, title, project, summary, and content")]
    public async Task<string> SetKnowledge(string topic, string title, string project, string summary, string content)
    {
        try
        {
            Log.Information("KnowledgeTools.SetKnowledge: Iniciando creación de entrada...");
            Log.Debug("  - Topic: {Topic}, Title: {Title}, Project: {Project}", topic, title, project);
                        
            Log.Information("KnowledgeTools.SetKnowledge: Enviando comando CreateKnowledge...");
            var result = await _sender.Send(new CreateKnowledgeCommand(topic, title, project, summary, content));

            Log.Information("KnowledgeTools.SetKnowledge: Entrada creada exitosamente con RowId {RowId}", result);
            return $"Knowledge entry created successfully with ID: {result}, RowId: {result}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.SetKnowledge: Error al crear entrada");
            throw;
        }
    }

    [McpServerTool, Description("Tool to search knowledge entries with flexible filtering by search term, topic, and/or project")]
    public async Task<List<SearchKnowledgeDto>> SearchKnowledge(string? searchTerm = null, string? topic = null, string? project = null, int limit = 5)
    {
        try
        {
            Log.Information("KnowledgeTools.SearchKnowledge: Iniciando búsqueda...");
            Log.Debug("  - SearchTerm: {SearchTerm}, Topic: {Topic}, Project: {Project}, Limit: {Limit}", searchTerm ?? "null", topic ?? "null", project ?? "null", limit);
            
            var result = await _sender.Send(new SearchKnowledgeQuery(searchTerm, topic, project, limit));
            
            Log.Information("KnowledgeTools.SearchKnowledge: Búsqueda completada. Resultados encontrados: {ResultCount}", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.SearchKnowledge: Error en la búsqueda");
            throw;
        }
    }

    [McpServerTool, Description("Tool to get a knowledge entry by ID")]
    public async Task<GetKnowledgeByIdDto> GetKnowledgeById(string id)
    {
        try
        {
            Log.Information("KnowledgeTools.GetKnowledgeById: Obteniendo entrada con ID {KnowledgeId}", id);
            
            var result = await _sender.Send(new GetKnowledgeByIdQuery(id));
            
            Log.Information("KnowledgeTools.GetKnowledgeById: Entrada obtenida exitosamente");
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.GetKnowledgeById: Error al obtener entrada con ID {KnowledgeId}", id);
            throw;
        }
    }

    [McpServerTool, Description("Tool to update knowledge content by ID")]
    public async Task<UpdateKnowledgeDto> UpdateKnowledgeById(string id, string resumen, string content)
    {
        try
        {
            Log.Information("KnowledgeTools.UpdateKnowledgeById: Actualizando entrada con ID {KnowledgeId}", id);
            Log.Debug("  - Resumen length: {ResumenLength} caracteres", resumen.Length);
            Log.Debug("  - Content length: {ContentLength} caracteres", content.Length);
            
            var result = await _sender.Send(new UpdateKnowledgeCommand(id, resumen, content));
            
            Log.Information("KnowledgeTools.UpdateKnowledgeById: Entrada actualizada exitosamente");
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.UpdateKnowledgeById: Error al actualizar entrada con ID {KnowledgeId}", id);
            throw;
        }
    }

    [McpServerTool, Description("Tool to regenerate all embeddings for knowledge entries (useful when changing chunk size or embedding parameters)")]
    public async Task<string> RegenerateAllEmbeddings()
    {
        try
        {
            Log.Information("KnowledgeTools.RegenerateAllEmbeddings: Iniciando regeneración de todos los embeddings...");
            
            await _knowledgeRepository.RegenerateAllEmbeddingsAsync();
            
            Log.Information("KnowledgeTools.RegenerateAllEmbeddings: ✅ Regeneración completada exitosamente");
            return "All embeddings have been regenerated successfully!";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.RegenerateAllEmbeddings: Error al regenerar embeddings");
            throw;
        }
    }

    [McpServerTool, Description("Tool to get the last (most recent) session")]
    public async Task<GetLastSessionDto?> GetLastSession(string? project = null)
    {
        try
        {
            Log.Information("KnowledgeTools.GetLastSession: Obteniendo última sesión...");
            Log.Debug("  - Project: {Project}", project ?? "null");
            
            var result = await _sender.Send(new GetLastSessionQuery(project));
            
            if (result == null)
            {
                Log.Information("KnowledgeTools.GetLastSession: No se encontraron sesiones");
                return null;
            }
            
            Log.Information("KnowledgeTools.GetLastSession: ✅ Sesión obtenida. RowId: {RowId}", result.RowId);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.GetLastSession: Error al obtener última sesión");
            throw;
        }
    }

    [McpServerTool, Description("Tool to get the latest sessions ordered by creation date")]
    public async Task<List<GetLatestSessionDto>> GetLatestSessions(int limit = 3, string? project = null)
    {
        try
        {
            Log.Information("KnowledgeTools.GetLatestSessions: Obteniendo últimas sesiones...");
            Log.Debug("  - Limit: {Limit}, Project: {Project}", limit, project ?? "null");
            
            var result = await _sender.Send(new GetLatestSessionsQuery(limit, project));
            
            Log.Information("KnowledgeTools.GetLatestSessions: ✅ Sesiones obtenidas. Count: {Count}", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.GetLatestSessions: Error al obtener últimas sesiones");
            throw;
        }
    }

    [McpServerTool, Description("Tool to save a current session")]
    public async Task<string> SaveSession(string project, string topic, string title, string summary, string content)
    {
        try
        {
            Log.Information("KnowledgeTools.SaveSession: Guardando sesión...");
            Log.Debug("  - Project: {Project}, Topic: {Topic}, Title: {Title}", project, topic, title);
            
     
            
           var result =await _sender.Send(new CreateSesionCommand(content,summary,project,content));
            
            Log.Information("KnowledgeTools.SaveSession: ✅ Sesión guardada exitosamente. RowId: {RowId}", result);
            return $"Session saved successfully with ID: {result}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "KnowledgeTools.SaveSession: Error al guardar sesión");
            throw;
        }
    }
}