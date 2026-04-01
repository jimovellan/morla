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
}