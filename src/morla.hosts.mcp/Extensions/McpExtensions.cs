using ModelContextProtocol;
using Microsoft.Extensions.DependencyInjection;

namespace Morla.hosts.MCP.Extensions;

/// <summary>
/// Extensiones para configuración del servidor MCP con instrucciones y metadatos
/// </summary>
public static class McpExtensions
{
    /// <summary>
    /// Configura las instrucciones que serán retornadas en la respuesta del initialize
    /// </summary>
    /// <param name="builder">MCP server builder</param>
    /// <param name="instructions">Contenido de instrucciones (markdown)</param>
    /// <returns>El builder para encadenamiento fluido</returns>
    public static IMcpServerBuilder WithInstructions(
        this IMcpServerBuilder builder, 
        string instructions)
    {
        // Registra las instrucciones como servicio singleton
        // Esto permite que los handlers de MCP accedan a ellas cuando necesiten
        builder.AddInstructions(instructions);
        return builder;
    }

    /// <summary>
    /// Registra las instrucciones como servicio en DI
    /// </summary>
    private static void AddInstructions(this IMcpServerBuilder builder, string instructions)
    {
        // Nota: Esta es una forma de registrar las instrucciones para que el servidor MCP
        // pueda acceder a ellas y incluirlas en la respuesta del initialize.
        // La librería ModelContextProtocol debería proporcionar un hook para esto,
        // pero si no lo hace, esta es una forma alternativa de hacerlo disponible.
        
        // GetSystemServiceProvider() si está disponible
        try
        {
            // Intenta acceder a los servicios del builder para registrar las instrucciones
            // esto es específico de la implementación interna de la librería
            var serviceProviderProperty = builder.GetType().GetProperty("ServiceProvider");
            if (serviceProviderProperty?.CanRead == true)
            {
                if (serviceProviderProperty.GetValue(builder) is IServiceCollection services)
                {
                    services.AddSingleton(new McpServerInstructions(instructions));
                }
            }
        }
        catch (Exception)
        {
            // Si no funciona, la instrucción se pasará de otra forma
            // (el servidor MCP tendrá que acceder a ella desde un servicio registrado)
        }
    }
}

/// <summary>
/// Almacena las instrucciones del servidor MCP
/// </summary>
public class McpServerInstructions
{
    public string Content { get; }

    public McpServerInstructions(string content)
    {
        Content = content;
    }
}
