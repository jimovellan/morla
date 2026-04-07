using Spectre.Console;

namespace Morla.Hosts.UI.Config;

/// <summary>
/// Configuración centralizada de UI Spectre
/// Define colores, estilos y constantes para la interfaz
/// </summary>
public static class UIConfig
{
    // Colores (strings para Spectre.Console markup)
    public static class Colors
    {
        public const string Primary = "cyan";
        public const string Success = "green";
        public const string Error = "red";
        public const string Warning = "yellow";
        public const string Secondary = "blue";
        public const string Tertiary = "magenta";
    }

    // Estilos de tabla
    public static class TableStyles
    {
        public static TableBorder DefaultBorder => TableBorder.Rounded;
        public static string DefaultBorderColor => Colors.Primary;
    }

    // Mensajes
    public static class Messages
    {
        public const string PressEscToGoBack = "[grey](Press [bold]ESC[/] to go back)[/]";
        public const string PressKeyToContinue = "[grey](Press any key to continue...)[/]";
        public const string NoResults = "No results found";
        public const string LoadingText = "Loading...";
        public const string ConfirmDelete = "Are you sure?";
    }

    // Límites por defecto
    public static class Defaults
    {
        public const int SearchResultsLimit = 10;
        public const int SessionsListLimit = 10;
        public const int PageSize = 15;
    }

    // Títulos de pantallas
    public static class Titles
    {
        public const string MainMenu = "MORLA - Main Menu";
        public const string SearchKnowledge = "Search Knowledge";
        public const string LatestSessions = "Latest Sessions";
        public const string LastSession = "Last Session";
        public const string DetailedView = "Details";
        public const string CreateKnowledge = "Create Knowledge";
        public const string CreateSession = "Create Session";
        public const string UpdateKnowledge = "Update Knowledge";
    }

    // ASCII Art
    public static class AsciiArtAssets
    {
        public const string Morla = @"
  ███╗   ███╗ ██████╗ ██████╗ ██╗     █████╗ 
  ████╗ ████║██╔═══██╗██╔══██╗██║    ██╔══██╗
  ██╔████╔██║██║   ██║██████╔╝██║    ███████║
  ██║╚██╔╝██║██║   ██║██╔══██╗██║    ██╔══██║
  ██║ ╚═╝ ██║╚██████╔╝██║  ██║███████╗██║  ██║
  ╚═╝     ╚═╝ ╚═════╝ ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝";
    }
}
