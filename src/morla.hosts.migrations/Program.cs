using Microsoft.Extensions.Hosting;
using Morla.Infrastructure.Extensions;

// =====================
// MIGRATIONS HOST
// =====================
// Este proyecto es únicamente para ejecutar migraciones de Entity Framework
// Uso: dotnet ef migrations add {MigrationName} --startup-project src/morla.hosts.migrations

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((_, services) =>
{
    services.AddArchitectureServices();
});

var host = builder.Build();
await host.RunAsync();
