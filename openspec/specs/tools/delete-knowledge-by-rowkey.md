# DeleteKnowledgeByRowKey Tool Specification

**Status**: Implemented in v0.0.71  
**Change**: `delete-knowledge-by-rowkey` (archived)  
**Last Updated**: 2026-04-17

## Overview

`DeleteKnowledgeByRowKey` is an MCP tool that permanently removes knowledge entries from the Morla knowledge base. It completes the CRUD lifecycle and enables safe deletion with validation.

## MCP Tool Contract

### Tool Name
```
delete_knowledge_by_rowkey
```

### Input Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `rowKey` | string | ✅ Yes | Unique identifier (RowKey/RowId) of the knowledge entry to delete |

### Output - Success Response

```json
{
  "success": true,
  "message": "Knowledge entry deleted successfully",
  "deletedRowKey": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "deletedId": 42
}
```

### Output - Error Response

**NOT_FOUND**:
```json
{
  "success": false,
  "error": "NOT_FOUND",
  "message": "Knowledge entry with rowKey 'a1b2c3d4-...' not found",
  "rowKey": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

**VALIDATION_ERROR**:
```json
{
  "success": false,
  "error": "VALIDATION_ERROR",
  "message": "RowKey parameter is required, cannot be null or empty",
  "rowKey": null
}
```

---

## Specifications

### Requirement: Deletion by RowKey

**When** user calls `DeleteKnowledgeByRowKey(rowKey: "a1b2...")`:

- **THEN** system validates that rowKey is not empty
- **AND** searches for entry by RowKey
- **IF** entry exists:
  - **THEN** removes it from database
  - **AND** removes associated embeddings
  - **AND** returns success response with deleted IDs
- **IF** entry not found:
  - **THEN** returns NOT_FOUND error
  - **AND** no changes occur
- **AND** operation is logged (title, project, RowKey)

### Requirement: Irreversibility

**When** entry is deleted:
- **THEN** no recovery mechanism is available
- **AND** deletion is permanent
- **AND** user is warned (via documentation) of this consequence

### Requirement: Validation

**When** user calls with invalid input:
- **IF** rowKey is null, empty, or whitespace:
  - **THEN** return VALIDATION_ERROR
  - **AND** no deletion occurs
- **IF** rowKey format is invalid (e.g., not a GUID):
  - **THEN** search returns no results (NOT_FOUND)
  - **AND** graceful error handling

---

## Implementation Details

### Code Location

```
src/morla.infrastructure/tools/KnowledgeTools.cs
```

### MCP Tool Method

```csharp
[McpServerTool]
[Description("ELIMINACIÓN DE CONOCIMIENTO: Elimina permanentemente una entrada de conocimiento...")]
public async Task<object> DeleteKnowledgeByRowKey(
    [Description("RowKey único identificador de la entrada...")] string rowKey)
{
    var command = new DeleteKnowledgeCommand(rowKey);
    var result = await _sender.Send(command);
    
    return new
    {
        success = result.Success,
        message = result.Message,
        deletedRowKey = result.DeletedRowKey,
        deletedId = result.DeletedId,
        error = !result.Success ? result.ErrorCode : null
    };
}
```

### CQRS Command Handler

```
src/morla.application/UseCases/Commands/DeleteKnowledge/DeleteKnowledgeCommandHandler.cs
```

**Handler Flow**:
1. Validate rowKey is not empty
2. Call `repository.DeleteAsync(rowKey)`
3. Log deletion (title, project for context)
4. Return response DTO with results

### Repository Layer

```
src/morla.infrastructure/Repository/KnowledgeRepository.cs
```

**Method**: `DeleteAsync(string rowId)`
- Accepts rowKey parameter
- Removes entry from database
- Returns success/failure status
- Existing implementation (not created for this change)

---

## Usage Scenarios

### Scenario 1: Delete Known Entry

**User Action**:
```
Tool Call: DeleteKnowledgeByRowKey("7f8e9d0c-1b2a-3f4e-5d6c-7b8a9f0e1d2c")
```

**Expected Outcome**:
```json
{
  "success": true,
  "message": "Knowledge entry deleted successfully",
  "deletedRowKey": "7f8e9d0c-1b2a-3f4e-5d6c-7b8a9f0e1d2c",
  "deletedId": 256
}
```

**Effect**: Knowledge entry is removed from database permanently.

### Scenario 2: Delete Non-Existent Entry

**User Action**:
```
Tool Call: DeleteKnowledgeByRowKey("ffffffff-ffff-ffff-ffff-ffffffffffff")
```

**Expected Outcome**:
```json
{
  "success": false,
  "error": "NOT_FOUND",
  "message": "Knowledge entry with rowKey 'ffffffff-...' not found",
  "rowKey": "ffffffff-ffff-ffff-ffff-ffffffffffff"
}
```

**Effect**: No changes; user informed of missing entry.

### Scenario 3: Invalid Input (Empty RowKey)

**User Action**:
```
Tool Call: DeleteKnowledgeByRowKey("")
```

**Expected Outcome**:
```json
{
  "success": false,
  "error": "VALIDATION_ERROR",
  "message": "RowKey parameter is required, cannot be null or empty"
}
```

**Effect**: Validation error; no database access.

---

## Safety & Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Require RowKey (UUID)** | Prevents accidental bulk deletions; UUID format ensures unique identification |
| **No cascade delete trigger** | Embeddings referenced only in Knowledge table; single DELETE removes everything |
| **Return deleted IDs** | Allows client to confirm deletion and maintain audit trail |
| **Log deletion events** | Enable review of deletion history via application logs; required for compliance |
| **No soft-delete** | Clear business requirement: permanent removal, not archive |

---

## Testing

### Unit Tests

Expected test coverage:
- ✅ Delete existing entry → success response
- ✅ Delete non-existent entry → NOT_FOUND error
- ✅ Delete with empty rowKey → VALIDATION_ERROR
- ✅ Delete with null rowKey → VALIDATION_ERROR
- ✅ Handler logging verification
- ✅ Response DTO structure validation

### Integration Tests

Expected scenarios:
- ✅ Full CQRS pipeline: Command → Handler → Repository → Database
- ✅ Database state after deletion (entry removed, no orphaned records)

---

## Related Documentation

- **Parent Specification**: [Morla MCP Tools Spec](spec.md#requirement-deleteknowledgebyrowkey---remove-knowledge-entry)
- **Change Archive**: `openspec/changes/archive/2026-04-17-delete-knowledge-by-rowkey/`
- **Version**: v0.0.71+
- **Release Date**: 2026-04-17

---

## Future Enhancements

- Batch delete by project/topic
- Schedule delayed deletion (soft-delete recovery window)
- Deletion policy configuration (retention periods)
- Deletion audit reports

