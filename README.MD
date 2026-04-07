

<div style="background-color:#0d1117;  
display:block;
padding:20px; border-radius:10px; display: flex; justify-content: center">
  <img src="assets/logo_black.png" width="200"/>
</div>


## 🚀 Inicio Rápido

### 1. Instalar Morla
```bash
dotnet build
dotnet publish -c Release
# Luego agregar el ejecutable a tu PATH
```

### 2. Configurar MCP (Obligatorio)
```bash
morla setup
```

Este comando configura automáticamente:
- ✅ **Copilot CLI**: Lee instrucciones en `~/.copilot/memory.instructions.md`
- ✅ **GitHub Copilot (VSCode)**: MCP disponible en `~/.vscode/mcp.json`
- ✅ **Configuración centralizada**: Backup en `~/.config/morla/`

**Después de ejecutar `morla setup`:**
1. Reinicia VSCode para cargar el MCP
2. Inicia una nueva sesión de Copilot CLI
3. Ejecuta el servidor MCP: `morla mcp`

### 3. Usar Morla
```bash
morla mcp        # Inicia servidor MCP (puerto 6277)
morla server     # Inicia API HTTP (puerto 5000)
morla setup      # Configura archivos globales
morla help       # Ver ayuda
```

---

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