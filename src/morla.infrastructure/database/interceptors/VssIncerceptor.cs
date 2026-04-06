using System.Data.Common;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

namespace Morla.Infrastructure.Database.Interceptors;


public class VssInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        if(connection is SqliteConnection sqlite)
        {
            try
            {
                Log.Information("VssInterceptor: Abriendo conexión SQLite, intentando cargar vec0...");
                
                //comprobar que puedo aceder al fichero 
                var extensionPath = GetExtensionToLoad();
                if (!System.IO.File.Exists(extensionPath))
                {
                    Log.Warning($"VssInterceptor: Archivo vec0 no encontrado en {extensionPath}");
                    throw new Exception($"Extension file not found: {extensionPath}");
                }
                Log.Information($"VssInterceptor: Archivo vec0 encontrado en {extensionPath}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "VssInterceptor: Error al verificar la ruta de vec0, continuando sin vectores");
            }

            
            try
            {
                Log.Information("VssInterceptor: Habilitando extensiones SQLite...");
                sqlite.EnableExtensions(true);
                
                Log.Information("VssInterceptor: Cargando extensión vec0...");
                sqlite.LoadExtension(GetExtensionToLoad());
                
                Log.Information("VssInterceptor: Extensión vec0 cargada exitosamente");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "VssInterceptor: No se pudo cargar vec0, continuando operación");
            }
        }
    }

    /// <summary>
    /// Obtiene la ruta absoluta de la extensión vec0 desde el directorio de ejecución
    /// </summary>
    private static string GetExtensionPath()
    {
        var baseDir = AppContext.BaseDirectory;
        var extensionName = GetExtensionFileName();
        var fullPath = Path.Combine(baseDir, extensionName);
        
        Log.Debug("VssInterceptor: Buscando vec0 en {ExtensionPath}", fullPath);
        
        return fullPath;
    }

    private static string GetExtensionFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "vec0.dll";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "vec0.so";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "vec0.dylib";
        throw new PlatformNotSupportedException("Unsupported OS platform");
    }

    public static string GetExtensionToLoad()
    {
        return GetExtensionPath();
    }
}