using Microsoft.EntityFrameworkCore;
using Morla.Domain.Models;
using Morla.Domain.Repository;
using Morla.Infrastructure.Database;

namespace morla.infrastructure.repositories;

public class KnowledgeRepository : IKnowledgeRepository
{
    private readonly MorlaContext _context;

    public KnowledgeRepository(MorlaContext context)
    {
        _context = context;
    }

    public async Task AddKnowledgeAsync(Knowledge knowledge)
    {
        _context.Knowledges.Add(knowledge);
        await _context.SaveChangesAsync();
    }

    public async Task<Knowledge?> GetByIdAsync(string id)
    {
        return await _context.Knowledges.FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<List<Knowledge>> GetAllAsync()
    {
        return await _context.Knowledges.ToListAsync();
    }

    public async Task UpdateAsync(Knowledge knowledge)
    {
        _context.Knowledges.Update(knowledge);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var knowledge = await GetByIdAsync(id);
        if (knowledge != null)
        {
            _context.Knowledges.Remove(knowledge);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Búsqueda semántica usando FTS5 (Full Text Search)
    /// Busca en los campos: Title, Summary, Content, Topic
    /// Si FTS5 no está disponible, cae a búsqueda regular LIKE
    /// </summary>
    /// <param name="searchTerm">Término de búsqueda (soporta operadores FTS5: AND, OR, "frase exacta", etc.)</param>
    /// <returns>Lista de Knowledge que coinciden con la búsqueda</returns>
    public async Task<List<Knowledge>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        try
        {
            // Sanitizar el término de búsqueda para FTS5
            var sanitizedTerm = SanitizeFtsQuery(searchTerm);

            // Usar FormattableString para que EF Core interprete correctamente la sentencia SQL
            // Especificar explícitamente todas las columnas de Knowledges
            var results = await _context.Knowledges
                .FromSqlInterpolated($@"
                    SELECT k.Id, k.Topic, k.Title, k.Project, k.Summary, k.Content, k.UpdatedAt, k.CreatedAt
                    FROM Knowledges k 
                    INNER JOIN Knowledges_fts f ON k.rowid = f.rowid 
                    WHERE f MATCH {sanitizedTerm}
                    ORDER BY rank")
                .ToListAsync();

            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FTS5 search failed: {ex.Message}. Falling back to LIKE search.");
            
            // Fallback: Búsqueda regular con LIKE si FTS5 falla
            return await FallbackSearchAsync(searchTerm);
        }
    }

    /// <summary>
    /// Búsqueda alternativa usando LIKE en caso de que FTS5 no esté disponible
    /// </summary>
    private async Task<List<Knowledge>> FallbackSearchAsync(string searchTerm)
    {
        var likePattern = $"%{searchTerm}%";
        
        return await _context.Knowledges
            .Where(k => EF.Functions.Like(k.Title, likePattern) ||
                        EF.Functions.Like(k.Summary, likePattern) ||
                        EF.Functions.Like(k.Content, likePattern) ||
                        EF.Functions.Like(k.Topic, likePattern))
            .ToListAsync();
    }

    /// <summary>
    /// Sanitiza un término de búsqueda para FTS5
    /// Envuelve cada palabra en comillas para evitar errores con caracteres especiales
    /// </summary>
    private static string SanitizeFtsQuery(string query)
    {
        // Dividir por espacios y envolver cada palabra en comillas
        var terms = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var sanitized = string.Join(" AND ", terms.Select(t => $"\"{t}\""));
        return sanitized;
    }
}