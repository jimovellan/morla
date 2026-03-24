using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        await App.RunAsync($"http://localhost:{Port}");
    }

    
}