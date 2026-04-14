

<div style="background-color:#0d1117;  
display:block;
padding:20px; border-radius:10px; display: flex; justify-content: center">
  <img src="assets/logo_black.png" width="200"/>
</div>



## 🚀 Início Rápido

### 1. Instalar Morla (como herramienta global)
```bash
dotnet tool install -g morla
```

### 2. Configurar OpenCode (Obligatorio)
Después de instalar, ejecuta:
```bash
morla setup
```

Este comando configura automáticamente:
- ✅ **OpenCode Copilot**: MCP disponible en `~/.vscode/mcp.json`
- ✅ **Configuración centralizada**: Backup en `~/.config/morla/`
- ✅ **Instrucciones del sistema**: Se cargan en `~/.copilot/memory.instructions.md`

**Después de ejecutar `morla setup`:**
1. Reinicia VSCode/OpenCode para cargar el MCP en Puerto 6277
2. Inicia una nueva sesión de Copilot
3. Morla MCP estará listo para usar automáticamente

### 3. Usar Morla
```bash
morla mcp        # Inicia servidor MCP (puerto 6277) - OPERATIVO
morla server     # API HTTP (puerto 5000) - ⏳ PENDIENTE DE DESARROLLO
morla setup      # Configura archivos globales
morla help       # Ver ayuda
```

### 📝 Nota sobre la IU
- **MCP Server** ✅ Operativo - Integrado con OpenCode Copilot
- **API HTTP Server** ⏳ Pendiente de desarrollo
- **Console UI / TUI** ⏳ Pendiente de desarrollo

## 📋 Logs

Los logs de Morla se guardan en:
```
~/.config/morla/logs/
./logs/  (en el directorio del proyecto durante desarrollo)
```

Cada ejecución de `morla` genera un archivo de log con timestamp:
```
morla-20260414.txt
```
Los logs incluyen información de depuración, errores y operaciones del sistema.

---

## 🏛️ Arquitectura

**Sistema Morla - OpenCode Copilot MCP Server**

```
                          ┌─────────────────────────┐
                          │   Morla Host (CLI)      │
                          │  Entry Point            │
                          │  System.CommandLine     │
                          └────────────┬────────────┘
                                       │
                   ┌───────────────────┼───────────────────┐
                   │                   │                   │
                   v                   v                   v
        ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
        │   MCP HOST ✅    │ │ HTTP API HOST    │ │ CONSOLE UI HOST  │
        │   (stdio)        │ │ (Web API) ⏳     │ │ (TUI) ⏳          │
        │   OPERATIVO      │ │ PENDIENTE        │ │ PENDIENTE        │
        └────────┬─────────┘ └────────┬─────────┘ └────────┬─────────┘
                 │                    │                    │
                 └────────────────────┼────────────────────┘
                                      │
                    ┌─────────────────v──────────────────┐
                    │   APPLICATION LAYER                │
                    │  • ToolExecutor                    │
                    │  • ToolRegistry                    │
                    │  • UseCases (Commands/Queries)     │
                    └─────────────────┬──────────────────┘
                                      │
                    ┌─────────────────v──────────────────┐
                    │   DOMAIN LAYER                     │
                    │  • Knowledge Entity                │
                    │  • Session Entity                  │
                    │  • Repository Interfaces           │
                    └─────────────────┬──────────────────┘
                                      │
                    ┌─────────────────v──────────────────┐
                    │   INFRASTRUCTURE LAYER             │
                    │  • EF Core (SQLite)                │
                    │  • Knowledge Repository            │
                    │  • Session Repository              │
                    │  • MCP Protocol Handlers           │
                    │  • ONNX Embeddings                 │
                    └────────────────────────────────────┘
```

**Estado:** OpenCode Copilot MCP Server operativo y listo para producción ✅

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