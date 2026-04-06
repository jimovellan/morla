using ElBruno.LocalEmbeddings;
using ElBruno.LocalEmbeddings.Options;
using Microsoft.EntityFrameworkCore;
using morla.domain.services;
using Morla.Domain.Models;
using Morla.Domain.Repository;
using Morla.Infrastructure.Database;
using System.Globalization;
using System.Text;

namespace morla.infrastructure.repositories;

public class KnowledgeRepository : IKnowledgeRepository
{
    private readonly MorlaContext _context;
    private LocalEmbeddingGenerator? _embeddingGenerator;
    private bool _embeddingGeneratorInitialized = false;

    private int split_in_chunks = 100;

    public KnowledgeRepository(MorlaContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lazy initialization del embedding generator
    /// </summary>
    private LocalEmbeddingGenerator GetEmbeddingGenerator()
    {
        if (_embeddingGeneratorInitialized && _embeddingGenerator != null)
            return _embeddingGenerator;

        try
        {
            var folderPath = Path.Combine(AppContext.BaseDirectory, "models");
           
            
            if (!Directory.Exists(folderPath))
            {
                Serilog.Log.Warning("KnowledgeRepository.GetEmbeddingGenerator: Carpeta de modelos no existe en {Path}", folderPath);
            }

            var _options = new LocalEmbeddingsOptions
            {
                ModelPath = folderPath,
                
              
            };
            _embeddingGenerator = new LocalEmbeddingGenerator(_options);
            _embeddingGeneratorInitialized = true;
            
            Serilog.Log.Information("KnowledgeRepository.GetEmbeddingGenerator: LocalEmbeddingGenerator inicializado correctamente");
            return _embeddingGenerator;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "KnowledgeRepository.GetEmbeddingGenerator: Error inicializando LocalEmbeddingGenerator");
            throw;
        }
    }

public async Task AddKnowledgeAsync(Knowledge knowledge)
{
    try
    {
        if (string.IsNullOrEmpty(knowledge.RowId))
            knowledge.RowId = TrackingKeyHelper.GenerateTrackingKey(knowledge.Topic, knowledge.Project, knowledge.Title);

        // Verificar si ya existe con ese RowId (hacer upsert)
        var existingKnowledge = await _context.Knowledges.FirstOrDefaultAsync(k => k.RowId == knowledge.RowId);
        
        if (existingKnowledge != null)
        {
            // Actualizar registro existente
            existingKnowledge.Title = knowledge.Title;
            existingKnowledge.Summary = knowledge.Summary;
            existingKnowledge.Content = knowledge.Content;
            existingKnowledge.Topic = knowledge.Topic;
            existingKnowledge.Project = knowledge.Project;
            existingKnowledge.UpdatedAt = DateTime.UtcNow;
            
            _context.Knowledges.Update(existingKnowledge);
        }
        else
        {
            // Crear nuevo registro
            knowledge.CreatedAt = DateTime.UtcNow;
            knowledge.UpdatedAt = DateTime.UtcNow;
            _context.Knowledges.Add(knowledge);
        }
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) when (
            dbEx.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx &&
            sqliteEx.SqliteErrorCode == 19 && // UNIQUE constraint failure
            dbEx.Message.Contains("RowId"))
        {
            // Condición de carrera: otro proceso insertó el mismo RowId
            // Reintentamos como update
            Serilog.Log.Warning(
                "KnowledgeRepository.AddKnowledgeAsync: Condición de carrera detectada para RowId '{RowId}'. Reintentando como update...", 
                knowledge.RowId);
            
            // Limpiar el DbContext y recargar desde la BD
            _context.ChangeTracker.Clear();
            
            var reloadedKnowledge = await _context.Knowledges.FirstOrDefaultAsync(k => k.RowId == knowledge.RowId);
            if (reloadedKnowledge != null)
            {
                reloadedKnowledge.Title = knowledge.Title;
                reloadedKnowledge.Summary = knowledge.Summary;
                reloadedKnowledge.Content = knowledge.Content;
                reloadedKnowledge.Topic = knowledge.Topic;
                reloadedKnowledge.Project = knowledge.Project;
                reloadedKnowledge.UpdatedAt = DateTime.UtcNow;
                
                _context.Knowledges.Update(reloadedKnowledge);
                await _context.SaveChangesAsync();
            }
        }

        // Obtener el ID del registro (necesario tanto para create como para update)
        var savedKnowledge = await _context.Knowledges.FirstOrDefaultAsync(k => k.RowId == knowledge.RowId);
        if (savedKnowledge == null)
            throw new InvalidOperationException($"No se pudo guardar o encontrar el conocimiento con RowId '{knowledge.RowId}'");

        // 🟢 Limpiar embeddings antiguos si es actualización
        using (var delCmd = _context.Database.GetDbConnection().CreateCommand())
        {
            delCmd.CommandText = "DELETE FROM knowledges_embedding WHERE rowid = $rowid";
            delCmd.Parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("$rowid", savedKnowledge.Id));

            if (delCmd.Connection.State != System.Data.ConnectionState.Open)
                await delCmd.Connection.OpenAsync();

            await delCmd.ExecuteNonQueryAsync();
        }

        // 🟢 USAMOS TEXTO ORIGINAL (BGE prefiere acentos y ñ)
        string fullText = $"{savedKnowledge.Title} {savedKnowledge.Summary} {savedKnowledge.Content}";
        
        // Solo pasamos a minúsculas, NO normalizamos acentos para el vector
        var textForEmbedding = fullText.ToLowerInvariant();

        var chunks = SplitIntoChunks(textForEmbedding, split_in_chunks);

        foreach (var chunk in chunks)
        {
            // 🟢 Generar con el chunk "rico" en info
            var embedding = await GetEmbeddingGenerator().GenerateEmbeddingAsync(chunk);
            
            var vector = embedding.Vector.Span.ToArray();
            
            // Normalización L2 (opcional si el modelo ya lo da, pero no estorba)
            var norm = Math.Sqrt(vector.Sum(x => x * x));
            if (norm == 0) norm = 1;
            var normalized = vector.Select(x => x / (float)norm).ToArray();

            var blob = new byte[normalized.Length * sizeof(float)];
            Buffer.BlockCopy(normalized, 0, blob, 0, blob.Length);

            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "INSERT INTO knowledges_embedding (rowid, embedding) VALUES ($rowid, $embedding)";
            cmd.Parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("$rowid", savedKnowledge.Id));
            cmd.Parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("$embedding", blob));

            if (cmd.Connection.State != System.Data.ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            await cmd.ExecuteNonQueryAsync();
        }
    }
    catch (Exception ex)
    {
        Serilog.Log.Error(ex, "KnowledgeRepository.AddKnowledgeAsync: Error");
        throw;
    }
}

// Método auxiliar
private List<string> SplitIntoChunks(string text, int tokensPerChunk = 50)
{
    var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var chunks = new List<string>();

    for (int i = 0; i < words.Length; i += tokensPerChunk)
    {
        var chunk = string.Join(" ", words.Skip(i).Take(tokensPerChunk));
        if (!string.IsNullOrWhiteSpace(chunk))
            chunks.Add(chunk);
    }
    return chunks;
}

/// <summary>
/// Normaliza texto removiendo acentos y caracteres especiales
/// </summary>
private static string NormalizeText(string text)
{
    if (string.IsNullOrEmpty(text))
        return text;

    // Normalizar a NFD (descomposición) para separar acentos
    string nfdForm = text.Normalize(NormalizationForm.FormD);
    var sb = new StringBuilder();

    foreach (char c in nfdForm)
    {
        UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
        
        // Mantener solo caracteres ASCII, números, espacios y guiones
        if (unicodeCategory != UnicodeCategory.NonSpacingMark &&
            (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_' || c == '.'))
        {
            sb.Append(c);
        }
    }

    return sb.ToString().Normalize(NormalizationForm.FormC);
}

    public async Task<Knowledge?> GetByIdAsync(string rowId)
    {
        return await _context.Knowledges.FirstOrDefaultAsync(k => k.RowId == rowId);  // ✅ Busca por RowId
    }

    public async Task<List<Knowledge>> GetAllAsync()
    {
        return await _context.Knowledges.ToListAsync();
    }

public async Task UpdateAsync(Knowledge knowledge)
{
    try
    {
        var existingKnowledge = await _context.Knowledges
            .FirstOrDefaultAsync(k => k.RowId == knowledge.RowId);

        if (existingKnowledge == null)
            throw new InvalidOperationException($"Knowledge with RowId '{knowledge.RowId}' not found");

        // 1. Actualizar los campos en la tabla principal
        existingKnowledge.Summary = knowledge.Summary;
        existingKnowledge.Content = knowledge.Content;
        existingKnowledge.Title = knowledge.Title;
        existingKnowledge.Topic = knowledge.Topic;
        existingKnowledge.Project = knowledge.Project;

        _context.Knowledges.Update(existingKnowledge);
        await _context.SaveChangesAsync();

        // 2. Preparar el texto para los nuevos Embeddings
        // 🟢 CAMBIO: Usamos el texto original. BGE prefiere acentos y mayúsculas/minúsculas naturales.
        string fullText = $"{existingKnowledge.Title} {existingKnowledge.Summary} {existingKnowledge.Content}";
        
        // Solo pasamos a minúsculas por estandarización, pero NO quitamos acentos.
        var textForEmbedding = fullText.ToLowerInvariant();

        // 3. Dividir en chunks
        var chunks = SplitIntoChunks(textForEmbedding, split_in_chunks);

        // 4. Limpiar embeddings antiguos (esto está perfecto en tu código)
        using (var delCmd = _context.Database.GetDbConnection().CreateCommand())
        {
            delCmd.CommandText = "DELETE FROM knowledges_embedding WHERE rowid = $rowid";
            delCmd.Parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("$rowid", existingKnowledge.Id));
            
            if (delCmd.Connection.State != System.Data.ConnectionState.Open)
                await delCmd.Connection.OpenAsync();
            
            await delCmd.ExecuteNonQueryAsync();
        }

        // 5. Insertar los nuevos vectores de BGE
        foreach (var chunk in chunks)
        {
            // Generar el embedding con el nuevo modelo cargado en el constructor
            var embedding = await GetEmbeddingGenerator().GenerateEmbeddingAsync(chunk);
            
            var vector = embedding.Vector.Span.ToArray();
            
            // Normalización L2 (asegura que el vector tenga longitud 1)
            var norm = Math.Sqrt(vector.Sum(x => x * x));
            if (norm == 0) norm = 1;
            var normalized = vector.Select(x => x / (float)norm).ToArray();

            var blob = new byte[normalized.Length * sizeof(float)];
            Buffer.BlockCopy(normalized, 0, blob, 0, blob.Length);

            using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "INSERT INTO knowledges_embedding (rowid, embedding) VALUES ($rowid, $embedding)";
            
            cmd.Parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("$rowid", existingKnowledge.Id));
            cmd.Parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("$embedding", blob));

            if (cmd.Connection.State != System.Data.ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            await cmd.ExecuteNonQueryAsync();
        }

        Serilog.Log.Information("KnowledgeRepository.UpdateAsync: Knowledge {RowId} actualizado con BGE-Small", knowledge.RowId);
    }
    catch (Exception ex)
    {
        Serilog.Log.Error(ex, "KnowledgeRepository.UpdateAsync: Error al actualizar {RowId}", knowledge.RowId);
        throw;
    }
}

    public async Task DeleteAsync(string rowId)
    {
        var knowledge = await GetByIdAsync(rowId);
        if (knowledge != null)
        {
            _context.Knowledges.Remove(knowledge);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Regenera todos los embeddings existentes (útil cuando cambias tamaño de chunks)
    /// </summary>
    public async Task RegenerateAllEmbeddingsAsync()
    {
        try
        {
            Serilog.Log.Information("KnowledgeRepository.RegenerateAllEmbeddingsAsync: Iniciando regeneración de todos los embeddings...");
            
            var allKnowledges = await _context.Knowledges.ToListAsync();
            Serilog.Log.Information("KnowledgeRepository.RegenerateAllEmbeddingsAsync: Encontrados {Count} documentos", allKnowledges.Count);

            // ✅ Borrar todos los embeddings
            using (var delCmd = _context.Database.GetDbConnection().CreateCommand())
            {
                delCmd.CommandText = "DELETE FROM knowledges_embedding";
                
                if (delCmd.Connection.State != System.Data.ConnectionState.Open)
                    await delCmd.Connection.OpenAsync();
                
                await delCmd.ExecuteNonQueryAsync();
            }
            Serilog.Log.Information("KnowledgeRepository.RegenerateAllEmbeddingsAsync: Embeddings borrados");

            var generator = GetEmbeddingGenerator();
            int processedCount = 0;

            // ✅ Regenerar embeddings para cada documento
            foreach (var knowledge in allKnowledges)
            {
                try
                {
                    string fullText = $"{knowledge.Title} {knowledge.Summary} {knowledge.Content}";
                    var textForEmbedding = fullText.ToLowerInvariant();

                    var chunks = SplitIntoChunks(textForEmbedding, split_in_chunks);

                    // Insertar embeddings por chunk
                    foreach (var chunk in chunks)
                    {
                        var embedding = await generator.GenerateEmbeddingAsync(chunk);
                        
                        var vector = embedding.Vector.Span.ToArray();
                        var norm = Math.Sqrt(vector.Sum(x => x * x));
                        if (norm == 0) norm = 1;
                        var normalized = vector.Select(x => x / (float)norm).ToArray();

                        var blob = new byte[normalized.Length * sizeof(float)];
                        Buffer.BlockCopy(normalized, 0, blob, 0, blob.Length);

                        using var cmd = _context.Database.GetDbConnection().CreateCommand();
                        cmd.CommandText = "INSERT INTO knowledges_embedding (rowid, embedding) VALUES ($rowid, $embedding)";
                        
                        cmd.Parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("$rowid", knowledge.Id));
                        cmd.Parameters.Add(new Microsoft.Data.Sqlite.SqliteParameter("$embedding", blob));

                        if (cmd.Connection.State != System.Data.ConnectionState.Open)
                            await cmd.Connection.OpenAsync();

                        await cmd.ExecuteNonQueryAsync();
                    }

                    processedCount++;
                    if (processedCount % 10 == 0)
                        Serilog.Log.Information("KnowledgeRepository.RegenerateAllEmbeddingsAsync: Procesados {Count}/{Total}", processedCount, allKnowledges.Count);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "KnowledgeRepository.RegenerateAllEmbeddingsAsync: Error regenerando embeddings para {RowId}", knowledge.RowId);
                }
            }

            Serilog.Log.Information("KnowledgeRepository.RegenerateAllEmbeddingsAsync: ✅ Regeneración completada. Procesados {Count}/{Total}", processedCount, allKnowledges.Count);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "KnowledgeRepository.RegenerateAllEmbeddingsAsync: Error fatal");
            throw;
        }
    }

      
    /// <summary>
    /// Extrae palabras clave de búsqueda, removiendo stop words comunes
    /// </summary>
    private static List<string> ExtractSearchWords(string searchTerm)
    {
        // ✅ Normalizar término de búsqueda y convertir a minúsculas
        searchTerm = NormalizeText(searchTerm).ToLowerInvariant();
        
        var stopWords = new[] { "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "de", "la", "el" };
        
        return searchTerm
            .ToLowerInvariant()
            .Split(new[] { ' ', ',', '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 2 && !stopWords.Contains(word))  // Palabras > 2 caracteres
            .Distinct()
            .ToList();
    }


async Task<List<(Knowledge Knowledge, int Score)>> IKnowledgeRepository.SearchAsync(string? searchTerm, string? topic, string? project, int limit = 5)
{
    try
    {
        Serilog.Log.Information("KnowledgeRepository.SearchAsync: Iniciando búsqueda híbrida (BGE-Small + Keywords)...");
        Serilog.Log.Debug("  - Limit: {Limit}", limit);

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            var simpleResults = await _context.Knowledges
                .Where(k => (string.IsNullOrEmpty(topic) || k.Topic == topic) &&
                            (string.IsNullOrEmpty(project) || k.Project == project))
                .ToListAsync();
            return simpleResults.Select(k => (k, 100)).ToList();
        }

        var allKnowledges = await _context.Knowledges.ToListAsync();
        var results = new List<(Knowledge, int)>();

        // 1️⃣ BÚSQUEDA VECTORIAL (BGE-Small-v1.5)
        Serilog.Log.Information("KnowledgeRepository.SearchAsync: Ejecutando búsqueda vectorial...");
        try
        {
            // ✅ CAMBIO: BGE requiere este prefijo exacto para las consultas
            var queryPrefix = "represent query for retrieval: ";
            
            // ✅ CAMBIO: No usamos NormalizeText para el vector, el modelo entiende acentos.
            // Solo pasamos a minúsculas para consistencia.
            var textToEmbed = queryPrefix + searchTerm.ToLowerInvariant();
            
            var searchEmbedding = await GetEmbeddingGenerator().GenerateEmbeddingAsync(textToEmbed);
            var searchVector = searchEmbedding.Vector.Span.ToArray();
            
            // Normalizar vector de búsqueda
            var norm = Math.Sqrt(searchVector.Sum(x => x * x));
            if (norm > 0)
                searchVector = searchVector.Select(x => x / (float)norm).ToArray();

            var vectorScores = new Dictionary<long, int>();

            using var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            // id es el PK de la tabla de embeddings, rowid es el FK hacia Knowledge.Id
            command.CommandText = "SELECT rowid, embedding FROM knowledges_embedding";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var knowledgeId = reader.GetInt64(0); 
                var embeddingBlob = (byte[])reader.GetValue(1);

                var embeddingFloats = new float[embeddingBlob.Length / sizeof(float)];
                Buffer.BlockCopy(embeddingBlob, 0, embeddingFloats, 0, embeddingBlob.Length);

                // Similitud coseno
                float similarity = 0;
                if (searchVector.Length == embeddingFloats.Length)
                {
                    for (int i = 0; i < searchVector.Length; i++)
                        similarity += searchVector[i] * embeddingFloats[i];
                }

                int vectorScore = (int)(similarity * 100);

                if (!vectorScores.ContainsKey(knowledgeId) || vectorScores[knowledgeId] < vectorScore)
                    vectorScores[knowledgeId] = vectorScore;
            }

            // ✅ CAMBIO: Umbral ajustado a 65 para BGE (55 era demasiado restrictivo para modelos locales)
            const int MIN_VECTOR_SCORE = 65; 
            
            var topVectorMatches = vectorScores
                .Where(kvp => kvp.Value >= MIN_VECTOR_SCORE)
                .OrderByDescending(kvp => kvp.Value)
                .Take(limit) // Limitamos a los N mejores matches semánticos
                .ToList();

            if (topVectorMatches.Any())
            {
                results = topVectorMatches
                    .Select(kvp => (allKnowledges.FirstOrDefault(k => k.Id == kvp.Key), kvp.Value))
                    .Where(x => x.Item1 != null &&
                               (string.IsNullOrEmpty(topic) || x.Item1.Topic == topic) &&
                               (string.IsNullOrEmpty(project) || x.Item1.Project == project))
                    .Select(x => (x.Item1!, x.Item2))
                    .ToList();

                if (results.Any())
                {
                    Serilog.Log.Information("KnowledgeRepository.SearchAsync: ✅ BÚSQUEDA VECTORIAL EXITOSA ({Count} resultados)", results.Count);
                    return results;
                }
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Warning(ex, "KnowledgeRepository.SearchAsync: Falló búsqueda vectorial, intentando keywords...");
        }

        // 2️⃣ FALLBACK - BÚSQUEDA POR KEYWORDS (Se mantiene tu lógica original de normalización)
        Serilog.Log.Information("KnowledgeRepository.SearchAsync: Ejecutando fallback de palabras clave...");
        var searchWords = ExtractSearchWords(searchTerm).ToList();

        if (searchWords.Any())
        {
            foreach (var knowledge in allKnowledges)
            {
                if (!(string.IsNullOrEmpty(topic) || knowledge.Topic == topic) ||
                    !(string.IsNullOrEmpty(project) || knowledge.Project == project))
                    continue;

                int score = 0;
                int wordsFound = 0;
                
                var normalizedTitle = NormalizeText(knowledge.Title).ToLowerInvariant();
                var normalizedSummary = NormalizeText(knowledge.Summary).ToLowerInvariant();
                var normalizedContent = NormalizeText(knowledge.Content).ToLowerInvariant();

                foreach (var word in searchWords)
                {
                    bool foundInDoc = false;
                    if (normalizedTitle.Contains(word)) { score += 50; foundInDoc = true; }
                    if (normalizedSummary.Contains(word)) { score += 30; foundInDoc = true; }
                    if (normalizedContent.Contains(word)) { score += 15; foundInDoc = true; }
                    
                    if (foundInDoc) wordsFound++;
                }
                
                bool hasWordInTitleOrSummary = searchWords.Any(w => normalizedTitle.Contains(w) || normalizedSummary.Contains(w));
                int minWordsRequired = searchWords.Count >= 3 ? (int)Math.Ceiling(searchWords.Count * 0.6) : 1;
                
                if (hasWordInTitleOrSummary && wordsFound >= minWordsRequired)
                    results.Add((knowledge, score));
            }
        }

        return results.OrderByDescending(x => x.Item2).Take(limit).ToList();
    }
    catch (Exception ex)
    {
        Serilog.Log.Error(ex, "KnowledgeRepository.SearchAsync: Error crítico");
        throw;
    }
}

    /// <summary>
    /// Obtiene la última sesión (más reciente por CreatedAt)
    /// </summary>
    public async Task<Knowledge?> GetLastSessionAsync(string? project = null)
    {
        try
        {
            Serilog.Log.Information("KnowledgeRepository.GetLastSessionAsync: Obteniendo última sesión...");
            Serilog.Log.Debug("  - Project: {Project}", project ?? "null");

            var query = _context.Knowledges
                .Where(k => k.Topic == morla.domain.constants.TopicNames.SESSION_TOPIC);

            if (!string.IsNullOrEmpty(project))
                query = query.Where(k => k.Project == project);

            var lastSession = await query
                .OrderByDescending(k => k.CreatedAt)
                .FirstOrDefaultAsync();

            Serilog.Log.Information("KnowledgeRepository.GetLastSessionAsync: ✅ Sesión obtenida. RowId: {RowId}", 
                lastSession?.RowId ?? "null");
            
            return lastSession;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "KnowledgeRepository.GetLastSessionAsync: Error");
            throw;
        }
    }

    /// <summary>
    /// Obtiene las N últimas sesiones ordenadas por CreatedAt DESC
    /// </summary>
    public async Task<List<Knowledge>> GetLatestSessionsAsync(int limit = 3, string? project = null)
    {
        try
        {
            Serilog.Log.Information("KnowledgeRepository.GetLatestSessionsAsync: Obteniendo últimas {Limit} sesiones...", limit);
            Serilog.Log.Debug("  - Project: {Project}, Limit: {Limit}", project ?? "null", limit);

            var query = _context.Knowledges
                .Where(k => k.Topic == morla.domain.constants.TopicNames.SESSION_TOPIC);

            if (!string.IsNullOrEmpty(project))
                query = query.Where(k => k.Project == project);

            var latestSessions = await query
                .OrderByDescending(k => k.CreatedAt)
                .Take(limit)
                .ToListAsync();

            Serilog.Log.Information("KnowledgeRepository.GetLatestSessionsAsync: ✅ Obtenidas {Count} sesiones", 
                latestSessions.Count);
            
            return latestSessions;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "KnowledgeRepository.GetLatestSessionsAsync: Error");
            throw;
        }
    }
}
