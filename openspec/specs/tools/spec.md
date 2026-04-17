# Morla MCP Tools Specification

## Purpose

Morla provides a set of MCP (Model Context Protocol) tools for managing a semantic knowledge base with full CRUD operations, search capabilities, and lifecycle management. These tools enable AI assistants to persist, retrieve, and organize knowledge persistently.

## Requirements

### Requirement: SetKnowledge - Create Knowledge Entry

The system SHALL allow users to create new knowledge entries with categorization and metadata.

**Description:** Creates a new knowledge entry or updates existing entry (upsert) with topic, project, summary, and content. Automatically generates embeddings for semantic search.

#### Scenario: Create new entry with full metadata
- **WHEN** user calls `SetKnowledge(topic, title, project, summary, content)`
- **THEN** system creates entry with unique RowId and returns ID
- **AND** embeddings are generated for semantic search

#### Scenario: Entry already exists
- **WHEN** user calls `SetKnowledge()` with topic/project/title matching existing entry
- **THEN** system updates the existing entry (upsert behavior)
- **AND** embeddings are regenerated

#### Scenario: Validate input parameters
- **WHEN** required fields (topic, title, project, summary, content) are missing
- **THEN** system returns validation error
- **AND** entry is NOT created

---

### Requirement: SearchKnowledge - Query Knowledge Base

The system SHALL allow users to search for knowledge entries using keywords, topic filters, and project filters with semantic ranking.

**Description:** Searches knowledge entries using keyword matching, topic filtering, and semantic similarity. Returns ranked results with relevance scores.

#### Scenario: Keyword-only search
- **WHEN** user calls `SearchKnowledge(searchTerm: "authentication")`
- **THEN** system returns entries matching term in title, summary, or content
- **AND** results are ranked by relevance

#### Scenario: Filter by topic
- **WHEN** user calls `SearchKnowledge(topic: "architecture")`
- **THEN** system returns only entries with topic == architecture
- **AND** results are within current project

#### Scenario: Multi-filter search
- **WHEN** user calls `SearchKnowledge(searchTerm: "JWT", topic: "architecture", project: "morla")`
- **THEN** system applies all filters
- **AND** returns combined results ranked by relevance

#### Scenario: No results found
- **WHEN** search term matches no entries
- **THEN** system returns empty list
- **AND** no error is raised

---

### Requirement: GetKnowledgeById - Retrieve Full Entry

The system SHALL allow users to retrieve complete knowledge entry details including content and metadata.

**Description:** Retrieves the full knowledge entry by RowId. Returns all fields including creation/update timestamps and embedding metadata.

#### Scenario: Valid entry ID
- **WHEN** user calls `GetKnowledgeById(id)`
- **THEN** system returns complete entry object
- **AND** includes title, summary, content, topic, project, timestamps

#### Scenario: Entry not found
- **WHEN** user calls `GetKnowledgeById()` with non-existent ID
- **THEN** system returns null/error
- **AND** no partial data is returned

---

### Requirement: UpdateKnowledgeById - Modify Existing Entry

The system SHALL allow users to update summary and content of existing knowledge entries.

**Description:** Updates summary and content of existing entry identified by RowId. Regenerates embeddings after update.

#### Scenario: Update summary and content
- **WHEN** user calls `UpdateKnowledgeById(id, newSummary, newContent)`
- **THEN** system updates entry
- **AND** UpdatedAt timestamp is refreshed
- **AND** embeddings are regenerated

#### Scenario: Entry not found
- **WHEN** user calls `UpdateKnowledgeById()` with non-existent ID
- **THEN** system returns error
- **AND** no changes are persisted

#### Scenario: Preserve metadata
- **WHEN** updating entry
- **THEN** topic, project, title remain unchanged
- **AND** CreatedAt is preserved

---

### Requirement: DeleteKnowledgeByRowKey - Remove Knowledge Entry

The system SHALL allow users to permanently delete knowledge entries by RowKey identifier.

**Description:** Permanently removes a knowledge entry and associated embeddings from the knowledge base. Operation is irreversible.

#### Scenario: Delete existing entry
- **WHEN** user calls `DeleteKnowledgeByRowKey(rowKey)`
- **THEN** system removes entry from database
- **AND** associated embeddings are deleted
- **AND** success response includes deleted RowKey

#### Scenario: Entry not found
- **WHEN** user calls `DeleteKnowledgeByRowKey()` with non-existent rowKey
- **THEN** system returns NOT_FOUND error
- **AND** no changes occur

#### Scenario: Permanent deletion
- **WHEN** entry is deleted
- **THEN** no recovery is possible
- **AND** user is responsible for data loss

---

## API Interactions

### Entry Lifecycle

```
SetKnowledge() → entry created with embeddings
    ↓
SearchKnowledge() → find entry via keyword/filter
    ↓
GetKnowledgeById() → retrieve full entry
    ↓
UpdateKnowledgeById() → modify summary/content (embeddings auto-regenerate)
    ↓
DeleteKnowledgeByRowKey() → remove entry (irreversible)
```

### Search Flow

```
SearchKnowledge(searchTerm, topic, project)
    → Keyword matching + semantic similarity
    → Topic filter (if provided)
    → Project filter (if provided)
    → Ranked results by relevance score
```

---

## Data Model

### Knowledge Entry

```
{
  id: number,                 // Internal auto-increment ID
  rowId: string,              // External unique identifier (GUID)
  topic: string,              // Category: architecture, bug, fix, convention, etc.
  title: string,              // Descriptive title (max 6 words)
  project: string,            // Project name for filtering
  summary: string,            // 2-3 line summary
  content: string,            // Full markdown content
  createdAt: datetime,        // UTC timestamp
  updatedAt: datetime,        // UTC timestamp
  embedding: float[]          // Semantic vector (auto-generated)
}
```

---

## Error Handling

| Error | HTTP Code | Scenario |
|-------|-----------|----------|
| `NOT_FOUND` | 404 | GetKnowledgeById or DeleteKnowledgeByRowKey with non-existent ID |
| `INVALID_INPUT` | 400 | Missing required fields, invalid topic, invalid project |
| `INTERNAL_ERROR` | 500 | Database transaction failure, embedding generation failure |

---

## Performance & Constraints

- **Search**: O(n) semantic similarity, indexed by topic/project
- **Embeddings**: Generated automatically on SetKnowledge/UpdateKnowledgeById
- **Result Limit**: Default 5 results, configurable via `limit` parameter (max 50)

---

## Usage Notes

- **Idempotency**: SetKnowledge is idempotent (upsert by topic:project:title)
- **Timestamps**: All timestamps are UTC and auto-managed by system
- **Embeddings**: Automatically updated on SetKnowledge and UpdateKnowledgeById operations
- **Project Filtering**: Recommended to always specify `project` in SearchKnowledge for relevant results
- **Deletion**: DeleteKnowledgeByRowKey is permanent with no recovery option

