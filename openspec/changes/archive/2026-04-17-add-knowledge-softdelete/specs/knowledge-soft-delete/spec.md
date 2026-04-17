## ADDED Requirements

### Requirement: Soft-delete knowledge entry
The system SHALL mark a knowledge entry as deleted without removing it from the database. The entry SHALL be excluded from all queries by default.

#### Scenario: User soft-deletes a knowledge entry
- **WHEN** DeleteKnowledge command is called with a valid rowKey
- **THEN** the entry's `isDeleted` flag is set to true
- **AND** the entry is no longer returned by SearchKnowledge or GetKnowledgeById
- **AND** the entry's semantic embedding is preserved

#### Scenario: Soft-deleted entry is invisible to standard queries
- **WHEN** SearchKnowledge is executed
- **THEN** soft-deleted entries are automatically excluded from results
- **AND** the search count reflects only active entries

#### Scenario: Soft-delete preserves entry metadata
- **WHEN** an entry is soft-deleted
- **THEN** timestamps (createdAt, updatedAt, deletedAt) are preserved
- **AND** the original content and embedding remain unchanged
