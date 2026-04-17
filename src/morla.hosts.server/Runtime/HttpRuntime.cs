using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediatR;
using Morla.Application.UseCases.Commands.DeleteKnowledge;
using Morla.Application.UseCases.Commands.RestoreKnowledge;
using Morla.hosts.Server.Models;
using Morla.Infrastructure.Extensions;

namespace Morla.hosts.Server.Runtime;


public class HttpRuntime
{
    private static WebApplication?  App;
    private static int Port;

    
    public async Task RunAsync(int port,  CancellationToken cancellationToken = default)
    {
        
        Port = port;
        Console.WriteLine($"Arrancando servidor HTTP en el puerto {Port}...");
        // ===== REGISTRAR SERVICIOS ESPECÍFICOS DEL SERVER HTTP =====
        
        // sharedServices.AddScoped<IMiRepositorio, MiRepositorio>();
        // sharedServices.AddScoped<IMiServicio, MiServicio>();
        
        // ===== CREAR BUILDER =====
        var builder = WebApplication.CreateBuilder();
        
        
        builder.Services.AddCoreServices();

        App = builder.Build();
        

        
        // Configure the HTTP request pipeline.
        if (App.Environment.IsDevelopment())
        {
            App.MapOpenApi();
        }

        App.UseHttpsRedirection();

        

        // ===== INYECTAR SERVICIOS EN HANDLERS =====
        App.MapGet("/test", () =>
        {
            // myService está inyectado automáticamente
            return Results.Ok("¡Hola desde el servidor HTTP!");
        })
        .WithName("GetTest");

        // ===== KNOWLEDGE ENDPOINTS =====
        
        // DELETE /knowledge/{rowKey} - Soft-delete by default, hard-delete with query param
        App.MapDelete("/knowledge/{rowKey}", async (string rowKey, [AsParameters] DeleteKnowledgeRequest request, IMediator mediator) =>
        {
            if (string.IsNullOrWhiteSpace(rowKey))
                return Results.BadRequest(new { error = "INVALID_INPUT", message = "rowKey parameter is required" });

            var command = new DeleteKnowledgeCommand(rowKey, request.HardDelete ?? false);
            var result = await mediator.Send(command);

            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("DeleteKnowledge")
        .WithOpenApi()
        .Produces(200, typeof(DeleteKnowledgeResponse))
        .Produces(404, typeof(DeleteKnowledgeResponse));

        // POST /knowledge/{rowKey}/restore - Restore soft-deleted entry
        App.MapPost("/knowledge/{rowKey}/restore", async (string rowKey, IMediator mediator) =>
        {
            if (string.IsNullOrWhiteSpace(rowKey))
                return Results.BadRequest(new { error = "INVALID_INPUT", message = "rowKey parameter is required" });

            var command = new RestoreKnowledgeCommand(rowKey);
            var result = await mediator.Send(command);

            return result.Success
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("RestoreKnowledge")
        .WithOpenApi()
        .Produces(200, typeof(RestoreKnowledgeResponse))
        .Produces(404, typeof(RestoreKnowledgeResponse));

        await App.RunAsync($"http://localhost:{Port}");
    }

    
}