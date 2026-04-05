using MediatR;

namespace Morla.Application.UseCases.Commands.SaveSesion;

/// <summary>
/// Comando para crear una nueva sesión. Contiene el título, resumen y contenido de la sesión a guardar.
/// </summary>
/// <param name="Titulo"></param>
/// <param name="Summary"></param>
/// <param name="Project"></param>
/// <param name="Content"></param>
public record CreateSesionCommand(string Titulo, string Summary, string Project, string Content): IRequest<string>;
    
        
    
