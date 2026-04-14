using Serilog;

namespace Morla.Infrastructure.Services;

/// <summary>
/// Servicio para cargar y gestionar las reglas maestras del agente Morla
/// Carga las instrucciones desde el archivo de configuración al iniciar
/// </summary>
public class AgentRulesService
{
    private string? _rules;
    private readonly string _rulesFilePath;

    public AgentRulesService()
    {
        // Ruta donde está el archivo de reglas
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _rulesFilePath = Path.Combine(userProfile, ".config", "opencode", "memory.instructions.md");
    }

    /// <summary>
    /// Carga las reglas desde el archivo al iniciar la aplicación
    /// CRÍTICO: Se ejecuta en el constructor para garantizar que las reglas están en memoria antes de cualquier operación MCP
    /// </summary>
    public void LoadRulesAtStartup()
    {
        try
        {
            if (File.Exists(_rulesFilePath))
            {
                _rules = File.ReadAllText(_rulesFilePath);
                Log.Information("AgentRulesService: Reglas cargadas desde {RulesPath}", _rulesFilePath);
            }
            else
            {
                // Fallback: Reglas por defecto
                _rules = GetDefaultRules();
                Log.Warning("AgentRulesService: Archivo de reglas no encontrado en {RulesPath}. Usando reglas por defecto.", _rulesFilePath);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "AgentRulesService: Error cargando reglas desde {RulesPath}", _rulesFilePath);
            _rules = GetDefaultRules();
        }
    }

    /// <summary>
    /// Devuelve las reglas cargadas en memoria
    /// Esta es la herramienta que el AI llamará para obtener sus reglas
    /// </summary>
    public string GetRules()
    {
        if (string.IsNullOrEmpty(_rules))
        {
            throw new InvalidOperationException("Las reglas no han sido cargadas. Llamar a LoadRulesAtStartup() primero.");
        }

        return $"""
            # REGLAS MAESTRAS DEL AGENTE MORLA
            
            ⚠️ ESTAS REGLAS SON INAMOVIBLES - IMPLEMENTA TODAS
            
            {_rules}
            
            ═════════════════════════════════════════════════════════════
            FIN DE REGLAS - Confirma que has leído e implementado todas
            ═════════════════════════════════════════════════════════════
            """;
    }

    /// <summary>
    /// Devuelve solo las reglas sin el encabezado (para uso interno)
    /// </summary>
    public string GetRulesRaw()
    {
        return _rules ?? GetDefaultRules();
    }

    /// <summary>
    /// Reglas por defecto si el archivo no existe
    /// </summary>
    private static string GetDefaultRules()
    {
        return """
            ## WORKFLOW OBLIGATORIO

            ### 1. BÚSQUEDA (SearchKnowledge)
            - SIEMPRE busca primero en Morla antes de responder
            - Parámetro OBLIGATORIO: `project: "morla"`
            - Si no está en Morla → busca externamente → GUARDA EN MORLA

            ### 2. CREACIÓN (SetKnowledge)
            - Cuando descubres algo nuevo → guardar en Morla
            - Opciones: BUG, FEATURE, COMPONENT, ARCHITECTURE, CONFIG, DECISION, LEARNING, TESTING, PERFORMANCE
            - Incluir ejemplos de código

            ### 3. ACTUALIZACIÓN (UpdateKnowledgeById)
            - Si existe → ACTUALIZAR, NO DUPLICAR
            - Devuelve ID para futuras referencias

            ### 4. SESIONES (SaveSession)
            - Al terminar: guardar resumen de la sesión
            - Format: ✅ COMPLETADO / 📝 PENDIENTES

            ### 5. PROTOCOLOS MCP
            - Ejecuta `morla.get_agent_rules()` al iniciar
            - Lee todos los prompts de Morla
            - Respeta el objeto `initialize` del servidor MCP

            ---
            Autor: Morla Protocol
            Vigencia: Indefinida
            Última actualización: 2026-04-14
            """;
    }
}
