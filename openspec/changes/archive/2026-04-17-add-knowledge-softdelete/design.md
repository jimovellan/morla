## Context

Currently, Knowledge entries are permanently deleted when `DeleteKnowledgeByRowKey()` is called, preventing any recovery or audit trail. This design introduces a soft-delete pattern to preserve data while marking entries as removed from active use.

The system uses SQLite with EF Core, allowing column-level migrations. Existing code patterns (SearchKnowledge, GetKnowledgeById) must be extended to filter out soft-deleted entries transparently.

## Goals / Non-Goals

**Goals:**
- Enable recovery of accidentally deleted knowledge entries
- Maintain audit trails for compliance
- Preserve semantic embeddings for restored entries
- Provide transparent filtering for existing queries
- Support administrative restore operations
- Allow forced hard-delete when needed

**Non-Goals:**
- Version history / change tracking (only current state)
- Soft-delete cascade behavior (each entry independent)
- UI/admin panel (API-only in this phase)
- Scheduled permanent deletion

## Decisions

### Decision 1: Add `isDeleted` column to Knowledge table
**Choice**: Add nullable boolean column `isDeleted` with default false  
**Rationale**: Simple, backward-compatible migration. Existing entries default to active.  
**Alternatives**: Shadow table (more complex), archival table (harder to restore)

### Decision 2: Soft-delete by default, hard-delete opt-in
**Choice**: DeleteKnowledge performs soft-delete; require explicit parameter for hard-delete  
**Rationale**: Safer default. Prevents accidental permanent loss.  
**Alternatives**: Require explicit soft-delete call (less convenient), always hard-delete (loses recovery)

### Decision 3: Filter at query level, not repository level
**Choice**: Add `WHERE isDeleted = false` to all Knowledge queries in LINQ  
**Rationale**: Transparent to callers, queryable layer handles it consistently.  
**Alternatives**: Repository methods (more boilerplate), stored procedures (less testable)

### Decision 4: Restore via new endpoint, not update
**Choice**: Create `RestoreKnowledgeByRowKey(rowKey)` command  
**Rationale**: Explicit, auditable operation. Separate from regular update.  
**Alternatives**: Allow `isDeleted` parameter in update (ambiguous intent)

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| **Storage bloat**: Soft-deleted entries accumulate over time | Implement scheduled hard-delete policy (future: 90-day retention) |
| **Query performance**: Additional WHERE clause on every query | Minimal impact; add index on `isDeleted` if needed |
| **Restore creates stale embeddings**: Restored entry has old semantic content | Regenerate embeddings on restore if content unchanged |
| **Hard-delete API misuse**: User accidentally hard-deletes critical data | Require confirmation/admin role (future enhancement) |

## Migration Plan

1. **Phase 1 - Schema**: Add `isDeleted` column (nullable boolean, default false)
2. **Phase 2 - Queries**: Wrap all Knowledge queries with `isDeleted = false` filter
3. **Phase 3 - Commands**: Update DeleteKnowledge to soft-delete; add RestoreKnowledge command
4. **Rollback**: Remove `isDeleted` filter from queries (reverts to old behavior)

## Open Questions

- Should soft-deleted entries be queryable via admin API (filtered vs. hidden)?
- Should embeddings be regenerated automatically on restore?
- What is the default retention period before hard-delete? (90 days? Manual only?)
