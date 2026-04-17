namespace Morla.hosts.Server.Models;

/// <summary>
/// Request model for DELETE /knowledge/{rowKey} endpoint
/// </summary>
public class DeleteKnowledgeRequest
{
    /// <summary>
    /// Force permanent hard-delete instead of soft-delete (default: false)
    /// </summary>
    public bool? HardDelete { get; set; }
}
