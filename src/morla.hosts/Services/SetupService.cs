using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
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
    private List<(string Path, string Description, string SourceFile, string TargetFileName)> 
    GetTargetLocations()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        return new List<(string, string, string, string)>
        {
            // Copilot CLI (lee automáticamente en cada sesión)
            (
                Path.Combine(userProfile, ".config","opencode"),
                "Copilot CLI (instrucciones automáticas)",
                "morla.protocol.md",
                "morla-memory.instructions.md"
            ),
            
            // Backup centralizado / CLI portability
            (
                Path.Combine(userProfile, ".config", "morla"),
                "Configuración centralizada (.config/morla)",
                "morla.protocol.md",
                "morla-memory.instructions.md"
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
             var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // comprobar que existe el AGENTS.MD en el directorio destino
            var text = """
                # Morla
                ALWAYS READ memory-morla.instructions.md
            """;
            if(!File.Exists(Path.Combine(userProfile,".config","opencode", "AGENTS.md")))
            {
                Console.WriteLine("⚠️  AGENTS.md no encontrado en el directorio de origen, se creará uno nuevo con instrucciones básicas");
                //comprobar que tiene la el contenido #morla-mcp
                File.WriteAllText(Path.Combine(userProfile,".config","opencode", "AGENTS.md"), text);

                Console.WriteLine("✅ AGENTS.md creado en el directorio de origen con instrucciones básicas");
            }
            else
            {
                Console.WriteLine("✅ AGENTS.md encontrado en el directorio de origen");
                if(!File.ReadAllText(Path.Combine(userProfile,".config","opencode", "AGENTS.md")).Contains("# Morla"))
                {
                    Console.WriteLine("⚠️  AGENTS.md encontrado pero no contiene instrucciones de Morla, se agregarán instrucciones básicas");
                    File.AppendAllText(Path.Combine(userProfile,".config","opencode", "AGENTS.md"), text);
                    Console.WriteLine("✅ Instrucciones básicas agregadas a AGENTS.md en el directorio de origen");
                }
            }

            

            Console.WriteLine("\n🚀 INICIANDO CONFIGURACIÓN DE MORLA MCP para OpenCode");
            Console.WriteLine($"🖥️  Sistema Operativo: {os}\n");

            

            
            //leer el archivo .config
            var modificacion = false;
            JsonNode configJson = null;
           
            var configPath = Path.Combine(userProfile, ".config", "opencode", "opencode.json");
            
            if(!File.Exists(configPath))
            {
                configJson = new JsonObject();
                configJson["$schema"] = "https://opencode.ai/config.json";
                Console.WriteLine("⚠️  Configuración de MCP no encontrada, se creará una nueva configuración en ~/.config/opencode/opencode.json");
   
            }else{
                //leer el JSON existente
                var jsonText = File.ReadAllText(configPath);
                configJson = JsonNode.Parse(jsonText) as JsonObject;
                Console.WriteLine("✅ Configuración de MCP encontrada, se verificará que la sección de Morla esté correctamente configurada");
            }
            
            
            if(configJson["mcp"] == null)
            {
                configJson["mcp"] = new JsonObject();
                modificacion = true;
                Console.WriteLine("⚠️  Sección 'mcp' no encontrada en la configuración, se agregará la sección 'mcp'");
            }else{
                Console.WriteLine("✅ Sección 'mcp' encontrada en la configuración");
            }

            var mcpSection = configJson["mcp"];

            

            if(mcpSection["morla"] == null)
            {
                var morla = new JsonObject();
                var cmd = new JsonArray();
                cmd.Add("morla");
                cmd.Add("mcp");
                morla["command"] = cmd;
                morla["enabled"] = true;
                morla["type"] = "local";
                mcpSection["morla"] = morla;
                modificacion = true;
                Console.WriteLine("⚠️  MCP 'morla' no encontrado en la configuración, se agregará el MCP 'morla' con comando 'morla mcp'");
            }else{
                Console.WriteLine("✅ MCP 'morla' encontrado en la configuración");
            }
            
            if(modificacion){
                //guardar el nuevo JSON en la ubicación de configuración
                var json = configJson.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                EnsureDirectoryExists(Path.GetDirectoryName(configPath)!);
                File.WriteAllText(configPath, json);
                Console.WriteLine("✅ Configuración de MCP actualizada en: " + configPath);
            }

            
                // Guardar el nuevo JSON en la ubicación de configuración
                
            
            
            
            

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
            Console.WriteLine("✓ opencode: Instrucciones cargadas en ~/.config/opencode/");
            Console.WriteLine("✓ Configuración centralizada: ~/.config/opencode/");
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

    /// <summary>
    /// Descarga el modelo ONNX_data desde GitHub Releases (demasiado grande para incluir en el paquete)
    /// </summary>
    public async Task DownloadModelAsync()
    {
        try
        {
            const string currentVersion = "0.0.36";
            const string downloadUrl = "https://github.com/jimovellan/morla/releases/download/v{0}/model.onnx_data";
            const long minFileSize = 100_000_000; // 100 MB

            var modelPath = Path.Combine(AppContext.BaseDirectory, "models");
            var modelFile = Path.Combine(modelPath, "model.onnx_data");

            // Si existe y tiene tamaño correcto, skip
            if (File.Exists(modelFile))
            {
                var fileInfo = new FileInfo(modelFile);
                if (fileInfo.Length > minFileSize)
                {
                    Log.Information("SetupService.DownloadModelAsync: ✅ Modelo ONNX ya existe ({Size} MB)", 
                        fileInfo.Length / 1_000_000);
                    return;
                }
            }

            // Crear directorio si no existe
            EnsureDirectoryExists(modelPath);

            Console.WriteLine("\n⏳ Descargando modelo ONNX (este proceso puede tardar varios minutos)...");
            Log.Information("SetupService.DownloadModelAsync: Iniciando descarga desde GitHub");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Morla-CLI");
            httpClient.Timeout = TimeSpan.FromMinutes(15);

            var url = string.Format(downloadUrl, currentVersion);
            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("SetupService.DownloadModelAsync: HTTP {StatusCode} - {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                Console.WriteLine($"⚠️  No se pudo descargar el modelo automáticamente");
                Console.WriteLine($"   Descárgalo manualmente desde:");
                Console.WriteLine($"   {url}");
                return;
            }

            // Descargar con progreso simple
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1;

            using (var contentStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(modelFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true))
            {
                var totalRead = 0L;
                var buffer = new byte[8192];
                int read;

                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    totalRead += read;

                    if (canReportProgress)
                    {
                        var percentage = (totalRead * 100) / totalBytes;
                        Console.Write($"\r   Progreso: {percentage}% ({totalRead / 1_000_000}/{totalBytes / 1_000_000} MB)");
                    }
                }
            }

            Console.WriteLine($"\n✅ Modelo ONNX descargado exitosamente");
            Log.Information("SetupService.DownloadModelAsync: ✅ Descarga completada");
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "SetupService.DownloadModelAsync: Error de conexión descargando modelo");
            Console.WriteLine($"❌ Error de conexión: {ex.Message}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SetupService.DownloadModelAsync: Error descargando modelo");
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
