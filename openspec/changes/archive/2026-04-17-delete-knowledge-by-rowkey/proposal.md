# Delete Knowledge by RowKey - Proposal

## Problem Statement

Currently, Morla's knowledge management system (`morla.domain` and `morla.infrastructure`) provides CQRS operations for knowledge entities:
- **Create**: `SetKnowledgeCommand` 
- **Read**: `GetKnowledgeByIdQuery`, `SearchKnowledgeQuery`
- **Update**: `UpdateKnowledgeCommand`
- **Delete**: ❌ **Missing**

The system lacks a mechanism to delete knowledge entries, limiting lifecycle management and preventing cleanup of outdated or incorrect knowledge.

## Why This Matters

### Use Cases
1. **Correcting Bad Knowledge**: If incorrect information is stored, there's no way to remove it cleanly
2. **Lifecycle Management**: Completed tasks or obsolete information should be removable
3. **Data Hygiene**: Support for maintaining a clean, verified knowledge base
4. **MCP Completeness**: Full CRUD symmetry aligns with "create, read, update, delete" completeness

### Current Gap
- Users can query and update knowledge, but cannot remove entries
- No UI/API/MCP tool exists to delete by RowKey (the unique external identifier)
- System forces reliance on database-level operations for deletion (not maintainable)

## Solution Overview

Add a **DeleteKnowledgeByRowKey** MCP tool that:
- ✅ Accepts `rowKey` as input parameter
- ✅ Validates the RowKey exists
- ✅ Removes the knowledge entry from the database
- ✅ Returns confirmation with deleted entity ID
- ✅ Integrates into the existing MCP handler registry

## Design Approach

Leverage the existing CQRS pattern:
1. Create `DeleteKnowledgeCommand` in `Application/UseCases/Commands/`
2. Create command handler in `Application/UseCases/Commands/DeleteKnowledge/`
3. Add repository method `DeleteByRowKeyAsync()` in `Infrastructure/Repository/`
4. Register MCP handler in `morla.hosts.mcp/Handlers/`
5. Expose via MCP protocol (as tool)

## Success Criteria

- ✅ Tool accepts `rowKey` parameter
- ✅ Returns success response with deleted entity details
- ✅ Properly validates input and returns error on invalid RowKey
- ✅ Integrates with existing MCP registry
- ✅ No breaking changes to existing tools
- ✅ Documented in MCP schema

## Non-Goals

- Soft deletes (permanent deletion only)
- Deletion by knowledge ID (RowKey is the public identifier)
- Bulk deletion (single entity at a time)
- Domain-level deletion (application-level only)
