# Morla - Copilot Instructions

**Morla** is a .NET knowledge base system with MCP support, API server, and CLI. It provides tools for managing structured knowledge with semantic search capabilities.

## Build, Test & Run

### Build
```bash
dotnet build
```

### Publish
```bash
dotnet publish -c Release
# Adds executable to PATH for `morla` command
```

### Run Locally
```bash
# MCP server (stdio, port 6277)
morla mcp

# HTTP API (port 5000)
morla server

# Setup configuration
morla setup
```

### Database Migrations (Entity Framework Core)
```bash
# Create migration
dotnet ef migrations add {MigrationName} \
  --project src/morla.infrastructure \
  --startup-project src/morla.hosts.migrations

# Apply migrations
dotnet ef database update \
  --project src/morla.infrastructure \
  --startup-project src/morla.hosts.migrations

# Remove last migration
dotnet ef migrations remove \
  --project src/morla.infrastructure \
  --startup-project src/morla.hosts.migrations

# List migrations
dotnet ef migrations list \
  --project src/morla.infrastructure \
  --startup-project src/morla.hosts.migrations
```

**Note:** `morla.hosts.migrations` is a special EF-only host because the main CLI has multiple commands.

## Architecture

Morla follows **layered architecture** with clean separation of concerns:

```
CLI Host (System.CommandLine)
    ↓
├─ MCP Host (stdio) → Port 6277
├─ HTTP API Host → Port 5000
└─ Console UI Host (TUI/CLI)
    ↓
Application Layer
├─ UseCases (CQRS pattern: Commands/Queries)
├─ ToolExecutor
└─ ToolRegistry
    ↓
Domain Layer
├─ ITool interface
├─ Models (Knowledge, Session, etc.)
└─ Repository interfaces
    ↓
Infrastructure Layer
├─ EF Core repositories
├─ SQLite/SQL database
├─ MCP protocol handlers
└─ HTTP clients
```

### Projects

- **morla.hosts** - Main CLI entry point, command-line interface
- **morla.hosts.mcp** - MCP protocol server
- **morla.hosts.server** - HTTP API server
- **morla.hosts.ui** - Console UI / Terminal UI
- **morla.hosts.migrations** - EF Core migration runner
- **morla.application** - CQRS commands/queries, use cases
- **morla.domain** - Core entities, models, interfaces
- **morla.infrastructure** - EF Core, repositories, DB access

## Key Conventions

### 1. CQRS Pattern
- **Commands** in `Application/UseCases/Commands/{CommandName}/`
  - `{Command}Command.cs` - Request class (implements `IRequest<T>`)
  - `{Command}CommandHandler.cs` - Handler (implements `IRequestHandler<TRequest, TResponse>`)
- **Queries** in `Application/UseCases/Queries/{QueryName}/`
  - `{Query}Query.cs` - Request class
  - `{Query}QueryHandler.cs` - Handler
- Use **MediatR** for dispatching commands/queries

### 2. Dependency Injection
- Configure in `Extensions/{Layer}Extensions.cs`
- Example: `ApplicationExtensions.cs` registers all application services
- Use `builder.Services` and extension methods for fluent setup

### 3. Logging
- Use **Serilog** (already configured in main host)
- Pattern: `Log.Information("Class.Method: Message")`, `Log.Debug()`, `Log.Error()`
- Scope logging: include operation context in messages

### 4. Database & Models
- **Entity**: `Knowledge`, `Session` (in domain layer)
- **RowId**: Unique tracking key (generated as `TrackingKeyHelper.GenerateTrackingKey(topic, project, title)`)
- **Timestamps**: `CreatedAt`, `UpdatedAt` (UTC)
- Repository pattern for data access (interface in domain, implementation in infrastructure)

### 5. Naming
- Projects: `morla.{layer}.{purpose}` or `morla.hosts.{host}`
- Namespaces: `Morla.{Layer}.{Category}` (PascalCase)
- Classes: PascalCase, descriptive
- Async methods: Suffix with `Async`

### 6. NullableReference & ImplicitUsings
All projects enable `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>` in .csproj files.

## Knowledge Base Integration

After `morla setup`, the system is configured at:
- `~/.copilot/memory.instructions.md` (Copilot CLI)
- `~/.vscode/mcp.json` (VS Code Copilot)
- `~/.config/morla/mcp.config.json` (backup)

**Required workflow:**
1. Always search Morla knowledge base first before implementing
2. Save session summaries at end of each session
3. Use `topic` categories: BUG, FEATURE, COMPONENT, ARCHITECTURE, CONFIG, DECISION, LEARNING, TESTING, PERFORMANCE
4. Format content with `## When to Use`, `## Content`, `## Code Examples` sections



## Extensions & Models

- **Extensions folder**: Contains compiled DLL/SO/dylib libraries (AI models, onnx runtime)
- **Models folder**: Model files (copied to output on build/publish)
- Both are bundled in the NuGet package

## Common Tasks

### Adding a new command
1. Create `Application/UseCases/Commands/{CommandName}/{CommandName}Command.cs`
2. Create `Application/UseCases/Commands/{CommandName}/{CommandName}CommandHandler.cs`
3. Register in `Application/Extensions/ApplicationExtensions.cs` (if using MediatR pipeline)
4. Call via: `await mediator.Send(new MyCommand(...))`

### Adding a new query
1. Create `Application/UseCases/Queries/{QueryName}/{QueryName}Query.cs`
2. Create `Application/UseCases/Queries/{QueryName}/{QueryName}QueryHandler.cs`
3. Register in `Application/Extensions/ApplicationExtensions.cs`
4. Call via: `await mediator.Send(new MyQuery(...))`

### Adding database entities
1. Create model in `Domain/Models/`
2. Create repository interface in `Domain/Repository/`
3. Create EF Core DbSet configuration in `Infrastructure/Data/ApplicationDbContext.cs`
4. Implement repository in `Infrastructure/Repository/`
5. Create and apply EF migration

### Mandatory (Obligatory)
- Always Load #~/.copilot/memory.instructions.md
