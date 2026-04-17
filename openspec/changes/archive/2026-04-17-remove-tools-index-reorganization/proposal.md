## Why

The `RegenerateAllEmbeddings` tool is a maintenance operation that rarely adds value. Modern embedding systems update indices automatically on write operations (SetKnowledge, UpdateKnowledgeById), making manual reorganization unnecessary. Removing it simplifies the MCP tool interface and reduces confusion about when to use which tools.

## What Changes

- **Removed**: `RegenerateAllEmbeddings()` tool from KnowledgeTools
  - This tool recalculated embeddings for ALL knowledge entries (a heavy operation)
  - It was intended for rare maintenance scenarios (embedding model changes, bugs)
  - Automatic embedding updates on write operations make it obsolete

## Capabilities

### New Capabilities

<!-- None - this is a removal, not a new capability -->

### Modified Capabilities

- `knowledge-management`: Embedding updates are now fully automatic; no manual reorganization needed

## Impact

- **Code changes**: `src/morla.infrastructure/tools/KnowledgeTools.cs` (remove RegenerateAllEmbeddings method and its Description attribute)
- **API**: MCP clients lose access to the RegenerateAllEmbeddings tool (backward-incompatible removal)
- **Documentation**: Update KnowledgeTools documentation (GetMcpDocumentation, GetInstructions)
- **Testing**: Remove any tests related to embedding regeneration
