using Morla.Domain.Models;

namespace Morla.Domain.Repository
{
    public interface IKnowledgeRepository
    {
        Task AddKnowledgeAsync(Knowledge knowledge);
        Task<List<(Knowledge Knowledge, int Score)>> SearchAsync(string? searchTerm = null, string? topic = null, string? project = null, int limit = 5);
        Task<List<Knowledge>> GetAllAsync();
        Task<Knowledge?> GetByIdAsync(string rowId);  // ✅ Usa RowId (string GUID)
        Task UpdateAsync(Knowledge knowledge);
        Task DeleteAsync(string rowId);  // ✅ Usa RowId
        Task RegenerateAllEmbeddingsAsync();
        
        // ✅ Métodos para obtener sesiones
        Task<Knowledge?> GetLastSessionAsync(string? project = null);
        Task<List<Knowledge>> GetLatestSessionsAsync(int limit = 3, string? project = null);
    }
}