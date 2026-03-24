using MediatR;
using Morla.Domain.Models;

namespace Morla.Application.UseCases.Commands.CreateKnowledge;

public record CreateKnowledgeCommand(
    string Topic,
    string Title,
    string Project,
    string Summary,
    string Content
) : IRequest<string>;
