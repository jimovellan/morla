# Morla MCP Tools Specification Index

This directory contains formal specifications for Morla MCP tools and their contracts.

## Overview

Morla provides 5 core MCP tools for knowledge base management:

| Tool | Version | Purpose | Input | Output |
|------|---------|---------|-------|--------|
| **SetKnowledge** | v0.0.71+ | Create/update knowledge entry | topic, title, project, summary, content | { id, rowId, success } |
| **SearchKnowledge** | v0.0.71+ | Query with filters & ranking | searchTerm?, topic?, project?, limit? | List<SearchResult> |
| **GetKnowledgeById** | v0.0.71+ | Retrieve full entry | id | KnowledgeEntry \| null |
| **UpdateKnowledgeById** | v0.0.71+ | Update summary/content | id, resumen, content | { success, updatedId } |
| **DeleteKnowledgeByRowKey** | v0.0.71+ | Permanent deletion | rowKey | { success, message, deletedRowKey } |

## File Structure

```
openspec/specs/tools/
├── README.md              ← This index (tool overview, versions, usage)
├── spec.md                ← Master specification (requirements, scenarios, API contracts)
└── examples/              ← Example usage patterns (optional)
    ├── csharp-usage.md
    ├── python-usage.md
    └── cli-usage.md
```

## Related Resources

### Archive
- **Change**: `2026-04-17-delete-knowledge-by-rowkey` - Implementation of DeleteKnowledgeByRowKey tool (v0.0.71)
  - Location: `openspec/changes/archive/`
  - Artifacts: proposal.md, design.md, tasks.md

### Implementation
- **Repository Layer**: `src/morla.infrastructure/tools/KnowledgeTools.cs` - MCP tool implementations
- **CQRS Commands**: `src/morla.application/UseCases/Commands/*/` - Command handlers
- **Domain Models**: `src/morla.domain/models/Knowledge.cs` - Data model

## Version History

### v0.0.71 (Current)
- ✅ SetKnowledge - Create/upsert knowledge entries
- ✅ SearchKnowledge - Keyword + semantic search
- ✅ GetKnowledgeById - Retrieve full entry
- ✅ UpdateKnowledgeById - Update summary/content
- ✅ **DeleteKnowledgeByRowKey** - NEW - Permanent deletion
- ✅ RegenerateAllEmbeddings - Rebuild embeddings

### Earlier Versions
- Planned enhancements: Batch operations, GraphQL subscriptions, knowledge graph analysis

## Specification Details

See `spec.md` for complete requirements, scenarios, error handling, and API contracts.

### Quick Links
- [SetKnowledge Requirements](spec.md#requirement-setknowledge---create-knowledge-entry)
- [SearchKnowledge Requirements](spec.md#requirement-searchknowledge---query-knowledge-base)
- [GetKnowledgeById Requirements](spec.md#requirement-getknowledgebyid---retrieve-full-entry)
- [UpdateKnowledgeById Requirements](spec.md#requirement-updateknowledgebyid---modify-existing-entry)
- [DeleteKnowledgeByRowKey Requirements](spec.md#requirement-deleteknowledgebyrowkey---remove-knowledge-entry)
- [RegenerateAllEmbeddings Requirements](spec.md#requirement-regenerateallembeddings---rebuild-search-index)

---

**Last Updated**: 2026-04-17  
**Release**: v0.0.71  
**Status**: Stable (6 tools, full CRUD + search)
