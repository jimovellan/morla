namespace Morla.Domain.Models;
public class Knowledge
{
    public string Id { get; set; }
    public string? Topic { get; set; }
    public string Title { get; set; }
    public string? Project { get; set; }
    public string Summary { get; set; }
    public string Content { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}