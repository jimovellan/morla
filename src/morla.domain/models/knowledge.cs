namespace Morla.Domain.Models;
public class Knowledge
{
    public long Id { get; set; }  // PRIMARY KEY: auto-incremental, vinculado con vec0 rowid
    public required string RowId { get; set; }  // GUID/unique identifier para búsquedas y referencias externas
    public string? Topic { get; set; }
    public required string Title { get; set; }
    public string? Project { get; set; }
    public required string Summary { get; set; }
    public required string Content { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Soft-delete properties
    public bool IsDeleted { get; set; } = false;  // Default: not deleted
    public DateTime? DeletedAt { get; set; }  // Timestamp when soft-deleted (nullable)
    
    /// <summary>
    /// Perform soft-delete: mark as deleted and record timestamp
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Perform hard-delete: remove IsDeleted flag and clear timestamp (used before physical deletion)
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}