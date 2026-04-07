using MediatR;
using Morla.Hosts.UI.Config;
using Morla.Hosts.UI.Services;
using Spectre.Console;

namespace Morla.Hosts.UI.Screens.Queries;

/// <summary>
/// Pantalla principal del menú
/// Permite navegar a todas las funcionalidades de la aplicación
/// </summary>
public class MainMenuScreen : IScreen
{
    public string ScreenName => "Main Menu";

    public async Task ShowAsync(IMediator mediator, IScreenService screenService, NavigationStack navigationStack)
    {
        while (true)
        {
            await screenService.ClearScreenAsync();
            
            // Mostrar ASCII art de Morla con versión
            var asciiArt = UIConfig.AsciiArtAssets.Morla.Trim();
            var version = "v0.0.51";
            await screenService.DisplayAsciiTitleAsync(asciiArt, version);
            
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            
            var options = new[]
            {
                ("🔍 Search Knowledge", "search"),
                ("📋 Latest Sessions", "latest_sessions"),
                ("⏰ Last Session", "last_session"),
                ("➕ Create Knowledge", "create_knowledge"),
                ("➕ Create Session", "create_session"),
                ("✏️  Update Knowledge", "update_knowledge"),
                ("❌ Exit", "exit")
            };

            var selected = await screenService.DisplayMenuAsync(
                UIConfig.Titles.MainMenu,
                options,
                opt => opt.Item1
            );

            if (selected == default)
                continue; // ESC pressed

            var action = selected.Item2;

            switch (action)
            {
                case "search":
                    navigationStack.Push(new SearchKnowledgeScreen());
                    await navigationStack.Current!.ShowAsync(mediator, screenService, navigationStack);
                    navigationStack.GoBack();
                    break;

                case "latest_sessions":
                    navigationStack.Push(new GetLatestSessionsScreen());
                    await navigationStack.Current!.ShowAsync(mediator, screenService, navigationStack);
                    navigationStack.GoBack();
                    break;

                case "last_session":
                    navigationStack.Push(new GetLastSessionScreen());
                    await navigationStack.Current!.ShowAsync(mediator, screenService, navigationStack);
                    navigationStack.GoBack();
                    break;

                case "create_knowledge":
                    await screenService.DisplayInfoAsync("(TODO) Create Knowledge - Phase 3");
                    await screenService.PressKeyToContinueAsync();
                    break;

                case "create_session":
                    await screenService.DisplayInfoAsync("(TODO) Create Session - Phase 3");
                    await screenService.PressKeyToContinueAsync();
                    break;

                case "update_knowledge":
                    await screenService.DisplayInfoAsync("(TODO) Update Knowledge - Phase 3");
                    await screenService.PressKeyToContinueAsync();
                    break;

                case "exit":
                    await screenService.DisplaySuccessAsync("Goodbye!");
                    return;
            }
        }
    }
}
