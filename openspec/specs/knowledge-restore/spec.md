# knowledge-restore Specification

## Purpose
TBD - created by archiving change add-knowledge-softdelete. Update Purpose after archive.
## Requirements
### Requirement: Restore soft-deleted knowledge entry
The system SHALL allow restoring a previously soft-deleted knowledge entry to active status.

#### Scenario: Admin restores a soft-deleted entry
- **WHEN** RestoreKnowledge command is called with a valid rowKey of a soft-deleted entry
- **THEN** the entry's `isDeleted` flag is set to false
- **AND** the entry is immediately visible in SearchKnowledge and GetKnowledgeById
- **AND** no data loss occurs (content, embedding, metadata preserved)

#### Scenario: Restore fails on non-existent entry
- **WHEN** RestoreKnowledge is called with an invalid rowKey
- **THEN** the system returns an error
- **AND** no changes are made to the database

#### Scenario: Restore on already-active entry is idempotent
- **WHEN** RestoreKnowledge is called on an entry where `isDeleted` is already false
- **THEN** the operation succeeds without error
- **AND** the entry remains unchanged

