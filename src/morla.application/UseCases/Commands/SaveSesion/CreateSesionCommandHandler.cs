using MediatR;
using morla.domain.constants;
using morla.domain.services;
using Morla.Domain.Models;
using Morla.Domain.Repository;
using Serilog;

namespace Morla.Application.UseCases.Commands.SaveSesion;

public class CreateSesionCommandHandler: IRequestHandler<CreateSesionCommand, string>
{
    private readonly IKnowledgeRepository _knowledgeRepository;

    public CreateSesionCommandHandler(IKnowledgeRepository knowledgeRepository)
    {
        _knowledgeRepository = knowledgeRepository;
    }
    public async Task<string> Handle(CreateSesionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("CreateSesionCommandHandler.Handle: Procesando comando...");
            Log.Debug("  - Titulo: {Titulo}, Summary: {Summary}", request.Titulo, request.Summary);
            
            var knowledge = new Knowledge
            {
                
                RowId = TrackingKeyHelper.GenerateTrackingKey(TopicNames.SESSION_TOPIC, request.Project, string.Empty),
                Topic = TopicNames.SESSION_TOPIC,
                Title = request.Titulo,
                Project = request.Project,
                Summary = request.Summary,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _knowledgeRepository.AddKnowledgeAsync(knowledge);
            
            Log.Information("CreateSesionCommandHandler.Handle: Sesión guardada exitosamente.");
            return $"Session Id: {knowledge.RowId}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CreateSesionCommandHandler.Handle: Error al crear sesión");
            throw;
        }
    }
}
