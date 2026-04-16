# Delete Knowledge by RowKey - Design

## Architecture Overview

```
MCP Handler Layer (morla.hosts.mcp)
    ↓
DeleteKnowledgeRowKeyHandler (MCP Tool)
    ↓
Application Layer (morla.application)
    ├─ DeleteKnowledgeCommand
    └─ DeleteKnowledgeCommandHandler
    ↓
Repository Layer (morla.infrastructure)
    ├─ IKnowledgeRepository.DeleteByRowKeyAsync()
    └─ KnowledgeRepository (EF Core impl)
    ↓
Domain Layer (morla.domain)
    └─ Knowledge entity
    ↓
Database (SQLite/SQL)
```

## MCP Tool Contract

**Tool Name**: `delete_knowledge_by_rowkey`

**Input**:
```json
{
  "rowKey": "string (required) - External unique identifier (e.g., 'topic:project:timestamp')"
}
```

**Output Success** (200):
```json
{
  "success": true,
  "message": "Knowledge deleted successfully",
  "deletedId": "guid",
  "deletedRowKey": "string"
}
```

**Output Error** (400/404):
```json
{
  "success": false,
  "error": "Knowledge not found with rowKey: {rowKey}",
  "errorCode": "NOT_FOUND"
}
```

## Implementation Details

### 1. Command Layer (`Application/UseCases/Commands/DeleteKnowledge/`)

**DeleteKnowledgeCommand.cs**:
```csharp
public record DeleteKnowledgeCommand(string RowKey) : IRequest<DeleteKnowledgeResponse>;
```

**DeleteKnowledgeCommandHandler.cs**:
- Inject `IKnowledgeRepository`
- Call `repository.DeleteByRowKeyAsync(rowKey)`
- Handle `RowKeyNotFoundException` → translate to business exception
- Return response with deleted entity details

### 2. Repository Layer (`Infrastructure/Repository/`)

**IKnowledgeRepository.cs** - Add method:
```csharp
Task<Knowledge?> DeleteByRowKeyAsync(string rowKey, CancellationToken cancellationToken = default);
```

**KnowledgeRepository.cs** - Implementation:
- Query: `.FirstOrDefaultAsync(k => k.RowId == rowKey)`
- If null: return null (handler checks and throws)
- If found: `dbContext.Remove(knowledge)` → `SaveChangesAsync()`
- Return deleted entity

### 3. MCP Handler (`morla.hosts.mcp/Handlers/`)

**DeleteKnowledgeRowKeyHandler.cs**:
- Type: `ToolHandler`
- Input schema: `{ rowKey: string }`
- Uses: `IMediator.Send(new DeleteKnowledgeCommand(rowKey))`
- Response mapping: command result → MCP response

Registration in handler registry:
```csharp
registry.Register("delete_knowledge_by_rowkey", new DeleteKnowledgeRowKeyHandler(...));
```

## Behavior Specifications

### Happy Path
1. User calls MCP tool with valid `rowKey`
2. Command handler queries knowledge by rowKey
3. Entity found → deleted from DB
4. Response returned with success and deleted entity ID
5. MCP client receives confirmation

### Error Handling

| Scenario | Error | HTTP | Handling |
|----------|-------|------|----------|
| RowKey not found | `NOT_FOUND` | 404 | Return error response |
| Invalid RowKey format | `INVALID_INPUT` | 400 | Validate before query |
| DB transaction fails | `INTERNAL_ERROR` | 500 | Log and return generic error |

## Data Consistency

- **Transaction**: Single write operation (atomic)
- **Cascading**: Foreign keys configured with ON DELETE CASCADE if needed
- **Audit**: CreatedAt, UpdatedAt timestamps remain (delete records history)
- **Idempotency**: Calling twice with same rowKey → 2nd call returns 404

## Integration Points

### Existing Code Impact
- **No breaking changes**: Additive only
- **No modifications** to existing commands/handlers
- **Repository interface** extends (new method, not override)

### Dependencies
- `MediatR` (already in play)
- `Entity Framework Core` (already used)
- `IKnowledgeRepository` (existing abstraction)

## Metrics & Observability

- **Logging**: `Log.Information("DeleteKnowledge.Handler: Deleted knowledge {RowKey}")` 
- **Metrics**: Count successful/failed deletions (optional histogram)
- **Tracing**: Include operation in CorrelationId flow (if applicable)

## Testing Strategy

- **Unit Tests**: Mock repository → verify command handler logic
- **Integration Tests**: Real DB context → deleteandverify query returns 404
- **Handler Tests**: Verify MCP input/output serialization
- **Error Cases**: Invalid rowKey, DB constraints, concurrency

## Deployment Notes

- **EF Migration**: No schema changes (only adds operation on existing entity)
- **Backwards Compatibility**: ✅ Fully compatible
- **Rollback**: Remove handler registration; existing data untouched
