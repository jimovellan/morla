using ModelContextProtocol;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Morla.Infrastructure.Extensions;
using morla.infrastructure.tools;
using Serilog;

namespace Morla.hosts.MCP.Runtime;


public class McpRuntime
{
    public async Task RunAsync( CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ CRÍTICO: Configurar Serilog ANTES de hacer cualquier otra cosa
            // El protocolo MCP usa stdin/stdout para comunicación
            // Los logs van SOLO a archivo y stderr (nunca a stdout que es para MCP)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                // ✅ CRÍTICO: SOLO archivo, NADA a stdout/stderr
                // El protocolo MCP requiere stdout totalmente limpio
                // Logs en ~/Library/Application Support/Morla/ (macOS standard)
                .WriteTo.File(
                    path: Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Morla",
                        "logs",
                        "morla-.txt"
                    ),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 7
                )
                .CreateLogger();

            Log.Information("McpRuntime: Creando host...");
            
            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            {
                // ✅ IMPORTANTE: Desactivar logos y otros outputs del host
                DisableDefaults = false
            });

            // ✅ Limpiar TODOS los providers de logging
            builder.Logging.ClearProviders();
            // ✅ Usar SOLO Serilog, configurado para NO escribir a Console
            builder.Logging.AddSerilog();

            Log.Information("McpRuntime: Agregando servicios de infraestructura...");
            builder.Services.AddCoreServices();

            Log.Information("McpRuntime: Configurando servidor MCP...");
            builder.Services.AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly(typeof(KnowledgeTools).Assembly);

            Log.Information("McpRuntime: Compilando host...");
            var host = builder.Build();
            
            Log.Information("McpRuntime: Iniciando host...");
            await host.RunAsync(cancellationToken);
            
            Log.Information("McpRuntime: Host finalizado correctamente");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "McpRuntime: Error fatal en la ejecución");
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();  // ✅ Asegurar que se escriben todos los logs
        }
    }
}