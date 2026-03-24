using System.ComponentModel;
using System.Data.Common;
using MediatR;
using ModelContextProtocol.Server;
using Morla.Application.UseCases.Commands.CreateKnowledge;
using Morla.Application.UseCases.Queries.SearchKnowledge;
using Morla.Domain.Models;
using Morla.Domain.Repository;

namespace morla.infrastructure.tools;

[McpServerToolType]
public class KnowledgeTools
{
    private readonly IKnowledgeRepository _knowledgeRepository;
    private readonly ISender _sender;

    public KnowledgeTools(ISender sender)
    {
        _sender = sender;
    }

    [McpServerTool, Description("Tool to create a new knowledge entry")]
    public async Task<string> SetKnowledge(string topic, string title, string project, string summary, string content)
    {
        var knowledge = new Knowledge
        {
            Id = Guid.NewGuid().ToString(),
            Topic = topic,
            Title = title,
            Project = project,
            Summary = summary,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var result = await _sender.Send(new CreateKnowledgeCommand(topic, title, project, summary, content));

        return $"Knowledge entry created successfully with ID: {result}";
    }

    [McpServerTool, Description("Tool to get a knowledge entry by semantic search term")]
    public async Task<List<SearchKnowledgeDto>> GetKnowledge(string semanticSearchTerm)
    {
        var result = await _sender.Send(new SearchKnowledgeQuery(semanticSearchTerm));
        return result;
    }

    [McpServerTool, Description("Tool to get a knowledge entry by ID")]
    public async Task<string> GetKnowledgeById(string id)
    {
        var knowledge = await _knowledgeRepository.GetByIdAsync(id);
        if (knowledge == null)
        {
            return $"Knowledge entry with ID {id} not found.";
        }
        return $"ID: {knowledge.Id}\nTitle: {knowledge.Title}\nSummary: {knowledge.Summary}\nContent: {knowledge.Content}";
    }
}