using System.Runtime.InteropServices;
using Serilog;

namespace morla.infrastructure.services;

/// <summary>
/// Resuelve la ruta correcta de modelos según el SO y contexto de ejecución
/// </summary>
public static class ModelPathResolver
{
    public enum OperatingSystem
    {
        Windows,
        Linux,
        macOS,
        Unknown
    }

    /// <summary>
    /// Detecta el SO actual
    /// </summary>
    public static OperatingSystem GetOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OperatingSystem.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OperatingSystem.Linux;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return OperatingSystem.macOS;
        return OperatingSystem.Unknown;
    }

    /// <summary>
    /// Obtiene el nombre del SO para logging
    /// </summary>
    public static string GetOSName() =>
        GetOperatingSystem() switch
        {
            OperatingSystem.Windows => "Windows",
            OperatingSystem.Linux => "Linux",
            OperatingSystem.macOS => "macOS",
            _ => "Unknown"
        };

    /// <summary>
    /// Resuelve la ruta de los modelos
    /// </summary>
    public static string ResolveModelsPath()
    {
        var os = GetOperatingSystem();
        var baseDir = AppContext.BaseDirectory;

        Log.Information("ModelPathResolver: SO detectado = {OS}", GetOSName());
        Log.Debug("ModelPathResolver: AppContext.BaseDirectory = {BaseDir}", baseDir);

        // Estrategia 1: Usar ubicación del Assembly (más robusto)
        var assemblyDir = GetAssemblyDirectory();
        Log.Debug("ModelPathResolver: Assembly directory = {AssemblyDir}", assemblyDir);

        // Rutas a intentar en orden de preferencia
        var searchPaths = BuildSearchPaths(baseDir, assemblyDir, os);

        foreach (var path in searchPaths)
        {
            Log.Debug("ModelPathResolver: Buscando modelos en {Path}", path);
            if (Directory.Exists(path))
            {
                Log.Information("ModelPathResolver: ✅ Modelos encontrados en {Path}", path);
                ValidateModelsFolder(path);
                return path;
            }
        }

        // Si no encuentra nada, reporta y usa el primero (permitirá error más descriptivo)
        var fallbackPath = searchPaths.First();
        Log.Error("ModelPathResolver: ❌ Carpeta de modelos NO encontrada en ninguna ubicación. " +
                  "Buscadas: {SearchPaths}. Usando fallback: {FallbackPath}",
                  string.Join(", ", searchPaths), fallbackPath);
        return fallbackPath;
    }

    /// <summary>
    /// Obtiene el directorio del Assembly (más confiable que AppContext.BaseDirectory)
    /// </summary>
    private static string GetAssemblyDirectory()
    {
        try
        {
            var assembly = typeof(ModelPathResolver).Assembly;
            var location = assembly.Location;
            return Path.GetDirectoryName(location) ?? AppContext.BaseDirectory;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "ModelPathResolver: Error obteniendo Assembly directory, usando AppContext.BaseDirectory");
            return AppContext.BaseDirectory;
        }
    }

    /// <summary>
    /// Construye lista de rutas a buscar según SO y contexto
    /// </summary>
    private static List<string> BuildSearchPaths(string baseDir, string assemblyDir, OperatingSystem os)
    {
        var paths = new List<string>();

        // Estrategia 1: Directorio del Assembly (instalación de herramienta)
        paths.Add(Path.Combine(assemblyDir, "models"));

        // Estrategia 2: BaseDirectory (desarrollo local o instalación alternativa)
        paths.Add(Path.Combine(baseDir, "models"));

        // Estrategia 3: Ruta dentro de tools/ (instalación como tool)
        if (baseDir.Contains("tools") && baseDir.Contains("net10.0"))
        {
            paths.Add(Path.Combine(baseDir, "models"));
        }
        else
        {
            // Si no está en tools/, buscar subiendo directorios
            paths.Add(Path.Combine(baseDir, "tools", "net10.0", "any", "models"));
        }

        // Estrategia 4: Rutas relativas (desfasadas útiles en algunos contextos)
        paths.Add(Path.Combine(Path.GetDirectoryName(baseDir) ?? "", "models"));
        paths.Add(Path.Combine(baseDir, "..", "models"));
        paths.Add(Path.Combine(baseDir, "..", "..", "models"));
        paths.Add(Path.Combine(baseDir, "..", "..", "..", "models"));

        // Estrategia 5: Rutas absolutas comunes según SO
        switch (os)
        {
            case OperatingSystem.Windows:
                paths.Add(@"C:\Program Files\morla\models");
                paths.Add(@"C:\Program Files (x86)\morla\models");
                break;
            case OperatingSystem.Linux:
                paths.Add("/opt/morla/models");
                paths.Add("/usr/local/share/morla/models");
                break;
            case OperatingSystem.macOS:
                paths.Add("/usr/local/opt/morla/models");
                paths.Add("/opt/homebrew/opt/morla/models");
                break;
        }

        return paths.Distinct().ToList();
    }

    /// <summary>
    /// Valida que la carpeta de modelos contenga archivos críticos
    /// </summary>
    private static void ValidateModelsFolder(string modelsPath)
    {
        try
        {
            var files = Directory.GetFiles(modelsPath);
            Log.Debug("ModelPathResolver: Archivos en {ModelsPath}: {Count} archivos", modelsPath, files.Length);

            var onnxFiles = files.Where(f => f.EndsWith(".onnx", StringComparison.OrdinalIgnoreCase)).ToList();
            var jsonFiles = files.Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).ToList();

            if (onnxFiles.Any())
                Log.Information("ModelPathResolver: ✅ Encontrados archivos ONNX: {Files}", string.Join(", ", onnxFiles.Select(Path.GetFileName)));
            else
                Log.Warning("ModelPathResolver: ⚠️ No se encontraron archivos .onnx en {ModelsPath}", modelsPath);

            if (jsonFiles.Any())
                Log.Information("ModelPathResolver: ✅ Encontrados archivos JSON: {Files}", string.Join(", ", jsonFiles.Select(Path.GetFileName)));
            else
                Log.Warning("ModelPathResolver: ⚠️ No se encontraron archivos .json en {ModelsPath}", modelsPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "ModelPathResolver: Error validando contenido de {ModelsPath}", modelsPath);
        }
    }
}
