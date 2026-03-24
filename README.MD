

#Arquitectura
```
                          ┌─────────────────────────┐
                          │   MyTool.Host (CLI)     │
                          │  Entry Point            │
                          │  System.CommandLine     │
                          └────────────┬────────────┘
                                       │
                   ┌───────────────────┼───────────────────┐
                   │                   │                   │
                   v                   v                   v
        ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
        │   MCP HOST       │ │ HTTP API HOST    │ │ CONSOLE UI HOST  │
        │   (stdio)        │ │ (Web API)        │ │ (TUI / CLI)      │
        └────────┬─────────┘ └────────┬─────────┘ └────────┬─────────┘
                 │                    │                    │
                 └────────────────────┼────────────────────┘
                                      │
                    ┌─────────────────v──────────────────┐
                    │   APPLICATION LAYER                │
                    │  • ToolExecutor                    │
                    │  • ToolRegistry                    │
                    │  • UseCases (ExecuteTool)          │
                    └─────────────────┬──────────────────┘
                                      │
                    ┌─────────────────v──────────────────┐
                    │   DOMAIN LAYER                     │
                    │  • ITool                           │
                    │  • ToolResult                      │
                    │  • ToolDefinition                  │
                    └─────────────────┬──────────────────┘
                                      │
                    ┌─────────────────v──────────────────┐
                    │   INFRASTRUCTURE LAYER             │
                    │  • EchoTool                        │
                    │  • FileTool                        │
                    │  • DbTool (EF Core)                │
                    │  • Http Clients                    │
                    └────────────────────────────────────┘
```

## Migraciones de Base de Datos

Para crear y aplicar migraciones de Entity Framework Core, usa los siguientes comandos:

### Crear una nueva migración

```bash
dotnet ef migrations add {NombreMigracion} --project src/morla.infrastructure --startup-project src/morla.hosts.migrations
```

**Ejemplo:**
```bash
dotnet ef migrations add InitialCreate --project src/morla.infrastructure --startup-project src/morla.hosts.migrations
```

**Nota:** El proyecto `morla.hosts.migrations` es un host especial configurado solo para las herramientas de Entity Framework, ya que el `morla.hosts` principal es una CLI con múltiples comandos.

### Aplicar migraciones a la base de datos

```bash
dotnet ef database update --project src/morla.infrastructure --startup-project src/morla.hosts.migrations
```

### Revertir una migración

```bash
dotnet ef migrations remove --project src/morla.infrastructure --startup-project src/morla.hosts.migrations
```

### Ver el estado de las migraciones

```bash
dotnet ef migrations list --project src/morla.infrastructure --startup-project src/morla.hosts.migrations
```