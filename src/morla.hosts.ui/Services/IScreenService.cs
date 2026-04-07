using Spectre.Console;

namespace Morla.Hosts.UI.Services;

/// <summary>
/// Interfaz para servicios de interfaz de usuario basados en Spectre.Console
/// </summary>
public interface IScreenService
{
    /// <summary>
    /// Muestra un menú interactivo y retorna el elemento seleccionado o null si se presiona ESC
    /// </summary>
    Task<T?> DisplayMenuAsync<T>(string title, IEnumerable<T> items, Func<T, string> displayFormatter);

    /// <summary>
    /// Muestra una tabla con datos
    /// </summary>
    Task DisplayTableAsync<T>(IEnumerable<T> data, Action<Table> configureTable);

    /// <summary>
    /// Muestra un panel con título y contenido
    /// </summary>
    Task DisplayPanelAsync(string title, string content);

    /// <summary>
    /// Solicita entrada de texto con label y validación opcional
    /// </summary>
    Task<string> PromptTextInputAsync(string label, Func<string, bool>? validator = null);

    /// <summary>
    /// Solicita entrada de texto multiline (presionar Ctrl+D para terminar)
    /// </summary>
    Task<string> PromptMultilineInputAsync(string label);

    /// <summary>
    /// Solicita confirmación (sí/no)
    /// </summary>
    Task<bool> ConfirmActionAsync(string message);

    /// <summary>
    /// Muestra mensaje de error
    /// </summary>
    Task DisplayErrorAsync(string message);

    /// <summary>
    /// Muestra mensaje de éxito
    /// </summary>
    Task DisplaySuccessAsync(string message);

    /// <summary>
    /// Muestra mensaje informativo
    /// </summary>
    Task DisplayInfoAsync(string message);

    /// <summary>
    /// Limpia la pantalla
    /// </summary>
    Task ClearScreenAsync();

    /// <summary>
    /// Muestra una tabla seleccionable numerada y retorna el item seleccionado
    /// rowFormatter convierte cada item a un array de strings (uno por columna)
    /// </summary>
    Task<T?> DisplaySelectableTableAsync<T>(string title, string[] columnNames, IEnumerable<T> items, Func<T, string[]> rowFormatter);

    /// <summary>
    /// Pausa awaiting a key press
    /// </summary>
    Task PressKeyToContinueAsync();

    /// <summary>
    /// Muestra un título ASCII centrado con color
    /// </summary>
    Task DisplayAsciiTitleAsync(string asciiArt, string? subtitle = null);
}
