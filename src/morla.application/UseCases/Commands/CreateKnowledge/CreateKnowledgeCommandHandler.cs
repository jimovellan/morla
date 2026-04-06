using MediatR;
using morla.domain.services;
using Morla.Domain.Models;
using Morla.Domain.Repository;
using Serilog;

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
        try
        {
            Log.Information("CreateKnowledgeCommandHandler.Handle: Procesando comando...");
            Log.Debug("  - Topic: {Topic}, Title: {Title}, Project: {Project}", request.Topic, request.Title, request.Project);
            
            var knowledge = new Knowledge
            {
                // Id se genera automáticamente en la BD (AUTOINCREMENT)
                RowId = TrackingKeyHelper.GenerateTrackingKey(request.Topic,request.Project,request.Title),
                Topic = request.Topic,
                Title = request.Title,
                Project = request.Project,
                Summary = request.Summary,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Log.Information("CreateKnowledgeCommandHandler.Handle: Guardando en repositorio...");
            await _knowledgeRepository.AddKnowledgeAsync(knowledge);
            
            Log.Information("CreateKnowledgeCommandHandler.Handle: Entrada creada con RowId: {RowId}", knowledge.RowId);
            return knowledge.RowId;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CreateKnowledgeCommandHandler.Handle: Error al crear entrada");
            throw;
        }
    }
}
