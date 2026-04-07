using Spectre.Console;
using Morla.Hosts.UI.Config;

namespace Morla.Hosts.UI.Services;

/// <summary>
/// Implementación de servicios UI usando Spectre.Console
/// </summary>
public class ScreenService : IScreenService
{
    public async Task<T?> DisplayMenuAsync<T>(string title, IEnumerable<T> items, Func<T, string> displayFormatter)
    {
        var itemList = items.ToList();
        if (!itemList.Any())
        {
            await DisplayErrorAsync("No items to display");
            return default;
        }

        // Escapar caracteres especiales de Spectre en los choices
        var choices = itemList.Select(item => Markup.Escape(displayFormatter(item))).ToList();
        
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{UIConfig.Colors.Primary}]{Markup.Escape(title)}[/]")
                .PageSize(10)
                .MoreChoicesText($"[grey](Use arrow keys to navigate)[/]")
                .AddChoices(choices)
        );

        var index = choices.IndexOf(selection);
        return itemList[index];
    }

    public async Task DisplayTableAsync<T>(IEnumerable<T> data, Action<Table> configureTable)
    {
        var table = new Table();
        configureTable(table);
        AnsiConsole.Write(table);
        await Task.CompletedTask;
    }

    public async Task DisplayPanelAsync(string title, string content)
    {
        var panel = new Panel(content)
        {
            Header = new PanelHeader(title),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1, 1, 1)
        };
        AnsiConsole.Write(panel);
        await Task.CompletedTask;
    }

    public async Task<string> PromptTextInputAsync(string label, Func<string, bool>? validator = null)
    {
        while (true)
        {
            try
            {
                // Usar TextPrompt en lugar de Ask para capturar ESC
                var prompt = new TextPrompt<string>($"[{UIConfig.Colors.Primary}]{label}[/]")
                    .AllowEmpty(); // Permite entrada vacía (para campos opcionales)

                var input = AnsiConsole.Prompt(prompt);

                if (validator != null && !validator(input))
                {
                    await DisplayErrorAsync("Invalid input. Please try again.");
                    continue;
                }

                return input;
            }
            catch (OperationCanceledException)
            {
                // ESC presionado - devolver null para indicar cancelación
                return null!;
            }
        }
    }

    public async Task<string> PromptMultilineInputAsync(string label)
    {
        AnsiConsole.MarkupLine($"[{UIConfig.Colors.Primary}]{label}[/] (Press Ctrl+D to finish):");
        
        var lines = new List<string>();
        string? line;
        
        while ((line = Console.ReadLine()) != null)
        {
            lines.Add(line);
        }

        return string.Join(Environment.NewLine, lines);
    }

    public async Task<bool> ConfirmActionAsync(string message)
    {
        return AnsiConsole.Confirm($"[{UIConfig.Colors.Warning}]{message}[/]");
    }

    public async Task DisplayErrorAsync(string message)
    {
        AnsiConsole.MarkupLine($"[{UIConfig.Colors.Error}]✗ {message}[/]");
        await Task.CompletedTask;
    }

    public async Task DisplaySuccessAsync(string message)
    {
        AnsiConsole.MarkupLine($"[{UIConfig.Colors.Success}]✓ {message}[/]");
        await Task.CompletedTask;
    }

    public async Task DisplayInfoAsync(string message)
    {
        AnsiConsole.MarkupLine($"[{UIConfig.Colors.Primary}]ℹ {message}[/]");
        await Task.CompletedTask;
    }

    public async Task ClearScreenAsync()
    {
        AnsiConsole.Clear();
        await Task.CompletedTask;
    }

    public async Task PressKeyToContinueAsync()
    {
        AnsiConsole.MarkupLine(UIConfig.Messages.PressKeyToContinue);
        Console.ReadKey(true);
        await Task.CompletedTask;
    }

    public async Task<T?> DisplaySelectableTableAsync<T>(string title, string[] columnNames, IEnumerable<T> items, Func<T, string[]> rowFormatter)
    {
        var itemList = items.ToList();
        if (!itemList.Any())
        {
            await DisplayErrorAsync("No items to display");
            return default;
        }

        int selectedIndex = 0;
        int pageSize = 15;
        int startIndex = 0;

        while (true)
        {
            await ClearScreenAsync();
            
            // Mostrar título
            AnsiConsole.MarkupLine($"[bold {UIConfig.Colors.Primary}]{title}[/]");
            AnsiConsole.WriteLine();

            // Determinar rango visible
            int endIndex = Math.Min(startIndex + pageSize, itemList.Count);

            // Mostrar items como lista seleccionable compacta
            for (int i = startIndex; i < endIndex; i++)
            {
                var row = rowFormatter(itemList[i]);
                var rowText = string.Join(" • ", row);
                
                // Marcar fila seleccionada
                if (i == selectedIndex)
                {
                    AnsiConsole.MarkupLine($"[{UIConfig.Colors.Success}]► {(i + 1).ToString().PadLeft(3)}. {Markup.Escape(rowText)}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"  {(i + 1).ToString().PadLeft(3)}. {Markup.Escape(rowText)}");
                }
            }

            // Mostrar controles
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[{UIConfig.Colors.Primary}]↑/↓[/] Navigate | [{UIConfig.Colors.Primary}]ENTER[/] Select | [{UIConfig.Colors.Primary}]ESC[/] Back");
            AnsiConsole.MarkupLine($"[grey]({startIndex + 1}-{endIndex} of {itemList.Count})[/]");

            // Capturar entrada del teclado
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    if (selectedIndex > 0)
                    {
                        selectedIndex--;
                        if (selectedIndex < startIndex)
                        {
                            startIndex = Math.Max(0, startIndex - pageSize);
                        }
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (selectedIndex < itemList.Count - 1)
                    {
                        selectedIndex++;
                        if (selectedIndex >= startIndex + pageSize)
                        {
                            startIndex = Math.Min(selectedIndex - pageSize + 1, itemList.Count - pageSize);
                        }
                    }
                    break;

                case ConsoleKey.Enter:
                    return itemList[selectedIndex];

                case ConsoleKey.Escape:
                    return default;

                default:
                    break;
            }
        }
    }

    public async Task DisplayAsciiTitleAsync(string asciiArt, string? subtitle = null)
    {
        var lines = asciiArt.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var maxLineLength = lines.Max(line => line.Length);

        // Centrar y mostrar el ASCII art
        foreach (var line in lines)
        {
            var paddingLeft = (Console.WindowWidth - line.Length) / 2;
            var padding = paddingLeft > 0 ? new string(' ', paddingLeft) : "";
            AnsiConsole.MarkupLine($"{padding}[{UIConfig.Colors.Primary}]{Markup.Escape(line)}[/]");
        }

        // Mostrar subtitle centrado debajo
        if (!string.IsNullOrEmpty(subtitle))
        {
            AnsiConsole.WriteLine();
            var subtitlePadding = (Console.WindowWidth - subtitle.Length) / 2;
            var subtitlePad = subtitlePadding > 0 ? new string(' ', subtitlePadding) : "";
            AnsiConsole.MarkupLine($"{subtitlePad}[{UIConfig.Colors.Primary}]{Markup.Escape(subtitle)}[/]");
        }

        await Task.CompletedTask;
    }
}
