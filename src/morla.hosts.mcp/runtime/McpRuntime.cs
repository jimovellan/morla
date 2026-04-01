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
            // ✅ Configurar Serilog ANTES de usarlo
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning) // ✅ Excluir logs SQL
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/morla-.txt",                  // ✅ Carpeta logs, nombre morla-{date}.txt
                    rollingInterval: RollingInterval.Day,     // ✅ Rotar diariamente
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 7                 // ✅ Mantener últimos 7 archivos
                )
                .CreateLogger();

            Log.Information("McpRuntime: Creando host...");
            var builder = Host.CreateApplicationBuilder();

            // Configurar Serilog en el Host
            builder.Logging.ClearProviders();
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