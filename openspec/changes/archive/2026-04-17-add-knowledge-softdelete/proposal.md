## Why

Knowledge entries in Morla are permanently deleted, losing audit trails and making recovery impossible. A soft-delete mechanism preserves data integrity, enables rollback/recovery, and maintains compliance with data retention policies. This supports reliability and operational observability.

## What Changes

- Add `isDeleted` flag to Knowledge entity (default: false)
- Knowledge queries automatically filter out soft-deleted entries
- New soft-delete operation (marks as deleted vs hard delete)
- Optional hard-delete for permanent removal when needed
- DeleteKnowledge endpoint performs soft-delete by default (parameter to force hard-delete)
- All queries/searches exclude soft-deleted entries transparently
- New administrative capability to restore soft-deleted entries

## Capabilities

### New Capabilities
- `knowledge-soft-delete`: Ability to soft-delete knowledge entries without permanent loss
- `knowledge-restore`: Ability to restore previously soft-deleted knowledge entries
- `knowledge-hard-delete`: Force permanent deletion when soft-delete is insufficient

### Modified Capabilities
- `knowledge-search`: Modified to exclude soft-deleted entries from all search results
- `knowledge-query`: Modified to exclude soft-deleted entries from queries

## Impact

- **Entities**: Knowledge table adds `isDeleted` (bool, default: false) column
- **APIs**: DeleteKnowledge, GetKnowledgeById, SearchKnowledge modified; new RestoreKnowledge endpoint
- **Queries**: All Knowledge queries must filter `isDeleted = false`
- **Backward Compatibility**: Transparent - existing code continues to work without changes
- **Database**: Migration to add `isDeleted` column (non-breaking, nullable initially)
