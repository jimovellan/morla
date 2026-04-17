# knowledge-hard-delete Specification

## Purpose
TBD - created by archiving change add-knowledge-softdelete. Update Purpose after archive.
## Requirements
### Requirement: Permanently hard-delete knowledge entry
The system SHALL provide an administrative operation to permanently remove a knowledge entry from the database, optionally including its semantic embedding.

#### Scenario: Admin permanently deletes a soft-deleted entry
- **WHEN** DeleteKnowledgeByRowKey is called with `hardDelete: true` parameter
- **THEN** the entry is completely removed from the database
- **AND** the associated embedding is removed from the vector store
- **AND** this operation cannot be undone

#### Scenario: Attempting hard-delete on non-existent entry
- **WHEN** DeleteKnowledgeByRowKey is called with `hardDelete: true` on a non-existent rowKey
- **THEN** the system returns an error
- **AND** no changes are made

#### Scenario: Hard-delete requires explicit confirmation
- **WHEN** hard-delete is triggered
- **THEN** an audit log entry is created recording the deletion
- **AND** the operation timestamp and user (if available) are recorded

