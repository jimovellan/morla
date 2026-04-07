using Morla.Hosts.UI.Screens;

namespace Morla.Hosts.UI.Services;

/// <summary>
/// Gestiona la navegación entre pantallas usando un patrón de stack
/// Permite navegar adelante y atrás entre pantallas
/// </summary>
public class NavigationStack
{
    private readonly Stack<IScreen> _stack = new();

    /// <summary>
    /// Obtiene la pantalla actual en el top del stack
    /// </summary>
    public IScreen? Current => _stack.Count > 0 ? _stack.Peek() : null;

    /// <summary>
    /// Agrega una nueva pantalla al stack
    /// </summary>
    public void Push(IScreen screen)
    {
        _stack.Push(screen);
    }

    /// <summary>
    /// Regresa a la pantalla anterior
    /// Retorna true si hay pantalla anterior, false si stack está vacío
    /// </summary>
    public bool GoBack()
    {
        if (_stack.Count > 1)
        {
            _stack.Pop();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Reemplaza la pantalla actual con una nueva
    /// </summary>
    public void Replace(IScreen screen)
    {
        if (_stack.Count > 0)
            _stack.Pop();
        
        _stack.Push(screen);
    }

    /// <summary>
    /// Obtiene el número de pantallas en el stack
    /// </summary>
    public int Count => _stack.Count;

    /// <summary>
    /// Limpia el stack completamente
    /// </summary>
    public void Clear()
    {
        _stack.Clear();
    }

    /// <summary>
    /// Verifica si hay más de una pantalla en el stack (para permitir GoBack)
    /// </summary>
    public bool CanGoBack => _stack.Count > 1;
}
