using ModelContextProtocol;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Morla.Infrastructure.Extensions;
using morla.infrastructure.tools;
using Morla.hosts.MCP.Extensions;
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
            
            // ✅ Cargar instrucciones desde archivo
            string instructions = await LoadInstructionsAsync();
   
            builder.Services.AddMcpServer((a)=>{
                a.ServerInfo = new ModelContextProtocol.Protocol.Implementation
                {
                    Name = "Morla MCP Server",
                    Description = "Servidor MCP para gestión de base de conocimiento con búsqueda semántica y seguimiento de sesiones.",
                    Version = "1.0.0",
                    
                };
                a.ServerInstructions = instructions;
                

            })
                .WithStdioServerTransport()
                .WithToolsFromAssembly(typeof(KnowledgeTools).Assembly)
                .WithResourcesFromAssembly(typeof(KnowledgeTools).Assembly)
                
                .WithInstructions(instructions);

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
    
    private async Task<string> LoadInstructionsAsync()
    {
        try
        {
            // Rutas posibles para instructions.md
            var possiblePaths = new List<string>
            {
                // Ruta relativa desde el dll (para dotnet run)
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "morla.hosts", "setup-files", "instructions.md"),
                // Ruta absoluta para instalación global
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Morla", "setup-files", "instructions.md"),
                // Ruta para desarrollador
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "Morla", "instructions.md"),
                // Ruta en el binario publicado
                Path.Combine(AppContext.BaseDirectory, "setup-files", "instructions.md")
            };
            
            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                Log.Debug("LoadInstructionsAsync: Intentando cargar desde: {Path}", fullPath);
                
                if (File.Exists(fullPath))
                {
                    var content = await File.ReadAllTextAsync(fullPath);
                    Log.Information("LoadInstructionsAsync: Instrucciones cargadas desde {Path}", fullPath);
                    return content;
                }
            }
            
            // Fallback: retornar instrucciones mínimas
            Log.Warning("LoadInstructionsAsync: No se encontró instructions.md, usando fallback");
            return """
                # Morla - Knowledge Base Manager
                
                Morla provides a persistent knowledge base with semantic search.
                
                **Available Tools:**
                - `SetKnowledge` - Create knowledge entries
                - `SearchKnowledge` - Search with keyword/topic filters
                - `GetKnowledgeById` - Retrieve full entry by ID
                - `UpdateKnowledgeById` - Modify existing entries  
                - `RegenerateAllEmbeddings` - Regenerate semantic index
                
                **Getting Started:**
                1. Use SearchKnowledge first to check if knowledge exists
                2. Use SetKnowledge to save new discoveries
                3. Use UpdateKnowledgeById to keep knowledge current
                """;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "LoadInstructionsAsync: Error cargando instrucciones");
            return "Morla MCP Server - Knowledge base management with semantic search and session tracking.";
        }
    }
}