using MediatR;
using Morla.Domain.Models;
using Morla.Domain.Repository;

namespace Morla.Application.UseCases.Commands.CreateKnowledge;

public class CreateKnowledgeCommandHandler : IRequestHandler<CreateKnowledgeCommand, string>
{
    private readonly IKnowledgeRepository _knowledgeRepository;

    public CreateKnowledgeCommandHandler(IKnowledgeRepository knowledgeRepository)
    {
        _knowledgeRepository = knowledgeRepository;
    }

    public async Task<string> Handle(CreateKnowledgeCommand request, CancellationToken cancellationToken)
    {
        var knowledge = new Knowledge
        {
            Id = Guid.NewGuid().ToString(),
            Topic = request.Topic,
            Title = request.Title,
            Project = request.Project,
            Summary = request.Summary,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _knowledgeRepository.AddKnowledgeAsync(knowledge);
        return $"Knowledge entry created successfully with ID: {knowledge.Id}";
    }
}
