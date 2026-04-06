using System.Runtime.InteropServices;
using Serilog;

namespace Morla.hosts.Setup;

/// <summary>
/// Servicio para configurar archivos globales de Morla en diferentes plataformas
/// Copia archivos a ubicaciones estándar para Copilot CLI, GitHub Copilot (VSCode) y CLI centralizado
/// </summary>
public class SetupService
{
    private record SetupLocation(string Path, string Description, string? SourceFile = null, string? TargetFileName = null);

    /// <summary>
    /// Detecta el sistema operativo
    /// </summary>
    private string GetOperatingSystem() => 
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macOS" :
        "Unknown";

    /// <summary>
    /// Obtiene la carpeta de origen de los archivos de setup
    /// </summary>
    private string GetSetupFilesDirectory()
    {
        var appDir = AppContext.BaseDirectory;
        var setupDir = Path.Combine(appDir, "setup-files");
        return setupDir;
    }

    /// <summary>
    /// Crea directorio si no existe
    /// </summary>
    private void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Log.Information("SetupService: Directorio creado: {Directory}", directory);
        }
    }

    /// <summary>
    /// Copia archivo de origen a destino con validación
    /// </summary>
    private bool CopyFileIfExists(string sourceFile, string targetFile, string description)
    {
        if (!File.Exists(sourceFile))
        {
            Log.Warning("SetupService: Archivo no encontrado: {SourceFile}", sourceFile);
            return false;
        }

        try
        {
            File.Copy(sourceFile, targetFile, overwrite: true);
            Log.Information("SetupService: ✅ {Description}: {TargetPath}", description, targetFile);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SetupService: Error copiando {SourceFile} a {TargetFile}", sourceFile, targetFile);
            return false;
        }
    }

    /// <summary>
    /// Obtiene las ubicaciones de configuración según la plataforma
    /// Solo copia archivos de instrucciones. El MCP se registra con "copilot mcp add"
    /// </summary>
    private List<(string Path, string Description, string SourceFile, string TargetFileName)> GetTargetLocations()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        return new List<(string, string, string, string)>
        {
            // Copilot CLI (lee automáticamente en cada sesión)
            (
                Path.Combine(userProfile, ".copilot"),
                "Copilot CLI (instrucciones automáticas)",
                "morla.protocol.md",
                "memory.instructions.md"
            ),
            
            // Backup centralizado / CLI portability
            (
                Path.Combine(userProfile, ".config", "morla"),
                "Configuración centralizada (.config/morla)",
                "morla.protocol.md",
                "memory.instructions.md"
            ),
        };
    }

    /// <summary>
    /// Ejecuta el setup copiando archivos a ubicaciones globales
    /// </summary>
    public async Task ExecuteAsync()
    {
        try
        {
            var os = GetOperatingSystem();
            Log.Information("SetupService.ExecuteAsync: Detectado SO: {OS}", os);

            var sourceDir = GetSetupFilesDirectory();
            var locations = GetTargetLocations();

            Console.WriteLine("\n🚀 INICIANDO CONFIGURACIÓN DE MORLA MCP");
            Console.WriteLine($"🖥️  Sistema Operativo: {os}\n");

            var successCount = 0;
            var results = new List<(string Location, string FileName, bool Success)>();

            foreach (var (targetDir, description, sourceFile, targetFileName) in locations)
            {
                try
                {
                    EnsureDirectoryExists(targetDir);
                    
                    var sourceFilePath = Path.Combine(sourceDir, sourceFile);
                    var targetFilePath = Path.Combine(targetDir, targetFileName);

                    if (CopyFileIfExists(sourceFilePath, targetFilePath, description))
                    {
                        Console.WriteLine($"✅ {description}");
                        Console.WriteLine($"   → {targetFilePath}\n");
                        successCount++;
                        results.Add((targetDir, targetFileName, true));
                    }
                    else
                    {
                        Console.WriteLine($"⚠️  {description} (archivo no encontrado)\n");
                        results.Add((targetDir, targetFileName, false));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "SetupService: Error procesando {Description}", description);
                    Console.WriteLine($"❌ {description}: {ex.Message}\n");
                    results.Add((targetDir, targetFileName, false));
                }
            }

            // Resumen final
            Console.WriteLine("─────────────────────────────────────────");
            if (successCount == locations.Count)
            {
                Console.WriteLine("✅ SETUP COMPLETADO EXITOSAMENTE");
                Log.Information("SetupService.ExecuteAsync: ✅ SETUP COMPLETADO");
            }
            else
            {
                Console.WriteLine($"⚠️  SETUP PARCIALMENTE COMPLETADO ({successCount}/{locations.Count})");
                Log.Warning("SetupService.ExecuteAsync: ⚠️ SETUP PARCIAL ({SuccessCount}/{Total})", successCount, locations.Count);
            }

            Console.WriteLine("\n📝 CONFIGURACIÓN COMPLETADA:");
            Console.WriteLine("─────────────────────────────────────────");
            Console.WriteLine("✓ Copilot CLI: Instrucciones cargadas en ~/.copilot/");
            Console.WriteLine("✓ Configuración centralizada: ~/.config/morla/");
            Console.WriteLine("\n🔗 PRÓXIMOS PASOS:");
            Console.WriteLine("─────────────────────────────────────────");
            Console.WriteLine("\n1️⃣  REGISTRA EL MCP CON COPILOT:");
            Console.WriteLine("   $ copilot mcp add morla \"morla mcp\"");
            Console.WriteLine("\n2️⃣  REINICIA VS CODE para cargar el MCP:");
            Console.WriteLine("   Archivo → Recargar ventana (o Cmd+Shift+P → Reload Window)");
            Console.WriteLine("\n3️⃣  VERIFICA QUE EL MCP ESTÁ REGISTRADO:");
            Console.WriteLine("   $ copilot mcp list");
            Console.WriteLine("\n4️⃣  INICIA COPILOT Y MORLA ESTARÁ DISPONIBLE");
            Console.WriteLine("─────────────────────────────────────────\n");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SetupService.ExecuteAsync: Error crítico durante setup");
            Console.WriteLine($"\n❌ ERROR CRÍTICO: {ex.Message}");
            throw;
        }
    }
}
