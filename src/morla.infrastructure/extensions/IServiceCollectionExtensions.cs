using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Morla.Domain.Repository;
using Morla.Infrastructure.Database;
using morla.infrastructure.repositories;
using Morla.Application.Extensions;
using Serilog;

namespace Morla.Infrastructure.Extensions;


public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        Log.Information("AddCoreServices: Iniciando configuración de servicios core...");
        
        // Crear ruta de base de datos multiplataforma
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var morlaFolder = Path.Combine(appDataFolder, "Morla");
        Directory.CreateDirectory(morlaFolder);
        var dbPath = Path.Combine(morlaFolder, "morla.db");
        
        Log.Information("AddCoreServices: Base de datos en: {DbPath}", dbPath);
        
        services.AddDbContext<MorlaContext>(options =>
        {
            options.UseSqlite($"Data Source={dbPath}");
        });
            
        Log.Information("AddCoreServices: DbContext configurado");

        // =====================
        // REGISTER REPOSITORIES
        // =====================
        services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();
        services.AddApplicationServices();

        Log.Information("AddCoreServices: Repositorios registrados");

        //mirar si hay migraciones pendientes y aplicarlas
        using (var scope = services.BuildServiceProvider().CreateScope())
        {
            try
            {
                Log.Information("AddCoreServices: Iniciando migraciones de base de datos...");
                var context = scope.ServiceProvider.GetRequiredService<MorlaContext>();
                context.Database.Migrate();
                Log.Information("AddCoreServices: Migraciones completadas exitosamente");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "AddCoreServices: Error durante las migraciones");
                throw;
            }
        }
        
        Log.Information("AddCoreServices: Servicios core configurados correctamente");
        return services;
    }
}