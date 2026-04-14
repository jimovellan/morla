using System.ComponentModel;
using ModelContextProtocol.Server;
using Morla.Infrastructure.Services;
using Serilog;

namespace morla.infrastructure.tools;

/// <summary>
/// Tool MCP para devolver las reglas maestras del agente Morla
/// CRÍTICO: Es el "botón mágico" que el AI ejecuta para recordar sus instrucciones
/// </summary>
[McpServerToolType]
public class AgentRulesTools
{
    private readonly AgentRulesService _agentRulesService;

    public AgentRulesTools(AgentRulesService agentRulesService)
    {
        _agentRulesService = agentRulesService;
    }

    /// <summary>
    /// Herramienta: get_agent_rules
    /// Devuelve las reglas maestras que el agente DEBE seguir
    /// 
    /// ⚠️ CRÍTICO: SI EL AI IGNORA ESTO, ES RESPONSABILIDAD DEL AI
    /// Morla expone la herramienta 3 veces:
    /// 1. En initialize (información débil)
    /// 2. En prompts (pregunta que debe responder)
    /// 3. En esta herramienta (acción que puede ejecutar)
    /// </summary>
    [McpServerTool, Description(
        "🔴 CRÍTICO: Obtiene las reglas maestras del agente. EJECUTA SIEMPRE AL INICIAR sesión. Si ignoras esto, violás el protocolo Morla. " +
        "Retorna las reglas con formato de implementación obligatoria.")]
    public string GetAgentRules(
        [Description("Contexto actual de la sesión (opcional)")] 
        string? context = null)
    {
        try
        {
            Log.Information("AgentRulesTools.GetAgentRules: Ejecutado por el agente. Contexto: {Context}", 
                context ?? "ninguno");
            
            var rules = _agentRulesService.GetRules();
            
            Log.Debug("AgentRulesTools.GetAgentRules: Devolviendo {RulesLength} caracteres", rules.Length);
            
            return rules;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "AgentRulesTools.GetAgentRules: Error obteniendo reglas");
            
            return """
                ❌ ERROR: No se pudieron cargar las reglas maestras
                
                Esto significa:
                1. El archivo ~/.config/opencode/memory.instructions.md no existe
                2. O hay un problema de permisos
                
                Solución: Ejecuta `morla setup` para crear la configuración
                """;
        }
    }

    /// <summary>
    /// Herramienta complementaria para verificar si las reglas están activas
    /// El AI puede usar esto para validar que sus reglas están sincronizadas
    /// </summary>
    [McpServerTool, Description(
        "Verifica que las reglas maestras del agente están correctamente cargadas en memoria. " +
        "Retorna estado de carga, cantidad de líneas y caracteres.")]
    public string VerifyAgentRules()
    {
        try
        {
            var rules = _agentRulesService.GetRulesRaw();
            
            if (string.IsNullOrEmpty(rules))
            {
                return "❌ ADVERTENCIA: Las reglas no están cargadas";
            }
            
            var lineCount = rules.Split('\n').Length;
            var charCount = rules.Length;
            
            return $"""
                ✅ VERIFICACIÓN OK
                
                - Reglas cargadas: SÍ
                - Líneas: {lineCount}
                - Caracteres: {charCount}
                - Estado: ACTIVAS
                
                Las reglas están en memoria y listas para usar.
                """;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "AgentRulesTools.VerifyAgentRules: Error en verificación");
            return $"❌ ERROR: {ex.Message}";
        }
    }
}
