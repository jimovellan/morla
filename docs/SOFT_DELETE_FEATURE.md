# Soft-Delete Feature Documentation

## Overview

The Knowledge entity now supports soft-delete functionality, allowing entries to be marked as deleted without permanent removal. This preserves data for compliance, audit trails, and enables recovery of accidentally deleted entries.

## Behavior

### Default: Soft-Delete
When `DELETE /knowledge/{rowKey}` is called, the entry is marked as deleted (IsDeleted=true, DeletedAt=timestamp recorded) but remains in the database. The entry is automatically excluded from all search results and queries.

### Hard-Delete (Optional)
Append `?hardDelete=true` or use the `hardDelete` parameter to permanently remove an entry. This is irreversible.

### Restore
Soft-deleted entries can be restored using `POST /knowledge/{rowKey}/restore`, which resets IsDeleted=false and clears the DeletedAt timestamp.

## Database Schema Changes

- `IsDeleted` (bool, default: false) - Soft-delete flag
- `DeletedAt` (datetime?, nullable) - Timestamp when entry was soft-deleted
- Index on `IsDeleted` for query performance

## Query Filtering

All Knowledge queries automatically exclude soft-deleted entries using the `.WhereNotDeleted()` extension method:
- SearchKnowledge
- GetKnowledgeById
- GetAllAsync
- GetLastSessionAsync
- GetLatestSessionsAsync

To include deleted entries (for admin operations), use `GetByIdIncludingDeletedAsync()`.

## API Endpoints

### DELETE /knowledge/{rowKey}
- **Default**: Soft-delete (preserves data)
- **Query params**: `?hardDelete=true` for permanent deletion
- **Response**: { success, message, deletedRowKey, isSoftDelete }

### POST /knowledge/{rowKey}/restore
- **Purpose**: Restore soft-deleted entry
- **Response**: { success, message, restoredRowKey }

## MCP Tools

### DeleteKnowledgeByRowKey(rowKey, hardDelete=false)
Updated to support soft-delete by default. Pass `hardDelete=true` for permanent deletion.

### RestoreKnowledgeByRowKey(rowKey)
New tool to restore soft-deleted entries.

## Audit Logging

All soft-delete and hard-delete operations are logged with:
- Operation type (soft-delete / hard-delete)
- Entry RowKey, Title, Project
- DeletedAt timestamp (for soft-delete)

## Rollback Strategy

If issues occur after deployment:
1. Remove `.WhereNotDeleted()` filters from queries (reverts to returning all entries)
2. Drop the IsDeleted index
3. Drop the IsDeleted and DeletedAt columns (requires migration)

## Future Enhancements

- Scheduled hard-delete policy (90-day retention)
- Admin UI to view/manage deleted entries
- Bulk delete/restore operations
- Soft-delete audit reports
