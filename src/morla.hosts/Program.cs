using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Morla.hosts.MCP.Runtime;
using Morla.hosts.Server.Runtime;
using Morla.Hosts.UI.Runtime;
using Morla.hosts.Setup;
using Morla.Infrastructure.Extensions;

// =====================
// SETUP SERVICES WITH HOST BUILDER
// =====================



// =====================
// MCP COMMAND
// =====================
var mcpCommand = new Command("mcp", "Ejecuta el host MCP");

mcpCommand.SetHandler(async () =>
{
    var mcpRuntime = new McpRuntime();
    await mcpRuntime.RunAsync(CancellationToken.None);
    // var mcpRuntime = new MCPRuntime();
    // await mcpRuntime.RunAsync(services, CancellationToken.None);
});

// =====================
// SERVER COMMAND
// =====================
var portOption = new Option<int>(
    "--port",
    getDefaultValue: () => 5000,
    description: "Puerto del servidor HTTP"
);

var serverCommand = new Command("server", "Ejecuta el API HTTP");
serverCommand.AddOption(portOption);

serverCommand.SetHandler(async (int port) =>
{
    await new HttpRuntime().RunAsync(port, CancellationToken.None);
    
}, portOption);

// =====================
// UI COMMAND
// =====================
var uiCommand = new Command("ui", "Ejecuta la UI por consola");

uiCommand.SetHandler(async () =>
{
    var uiRuntime = new UIRuntime();
    await uiRuntime.ShowAsync(CancellationToken.None);
});

// =====================
// SETUP COMMAND
// =====================
var setupCommand = new Command("setup", "Configura archivos globales de Morla");

setupCommand.SetHandler(async () =>
{
    var setupService = new SetupService();
    await setupService.ExecuteAsync();
});

// =====================
// ROOT COMMAND
// =====================
var rootCommand = new RootCommand("MyTool CLI");

rootCommand.AddCommand(mcpCommand);
rootCommand.AddCommand(serverCommand);
rootCommand.AddCommand(uiCommand);
rootCommand.AddCommand(setupCommand);

// HELP automático incluido
await rootCommand.InvokeAsync(args);