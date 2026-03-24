using ModelContextProtocol;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Morla.Infrastructure.Extensions;
using morla.infrastructure.tools;

namespace Morla.hosts.MCP.Runtime;


public class McpRuntime
{
    public async Task RunAsync( CancellationToken cancellationToken = default)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.AddCoreServices();
            
        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly(typeof(KnowledgeTools).Assembly);


        var host = builder.Build();
        await host.RunAsync(cancellationToken);
    }
}