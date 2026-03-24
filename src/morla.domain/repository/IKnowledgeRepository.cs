using Morla.Domain.Models;

namespace Morla.Domain.Repository
{
    public interface IKnowledgeRepository
    {
        Task AddKnowledgeAsync(Knowledge knowledge);
        Task<List<Knowledge>> SearchAsync(string searchTerm);
        Task<List<Knowledge>> GetAllAsync();
        Task<Knowledge?> GetByIdAsync(string id);
        Task UpdateAsync(Knowledge knowledge);
        Task DeleteAsync(string id);
    }
}