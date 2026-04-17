## Context

The `RegenerateAllEmbeddings()` tool was added to handle scenarios where embeddings needed to be recalculated across the entire knowledge base. However, in practice:
- Embeddings are automatically recalculated on `SetKnowledge` and `UpdateKnowledgeById` operations
- No client has called this tool in production
- It adds complexity to the MCP interface without practical benefit
- It only served hypothetical maintenance scenarios (embedding model changes)

## Goals / Non-Goals

**Goals:**
- Remove `RegenerateAllEmbeddings()` from the KnowledgeTools MCP interface
- Simplify the tool set by removing unused, hard-to-explain functionality
- Update documentation to reflect automatic embedding updates

**Non-Goals:**
- Change how embeddings are generated or updated
- Modify the embedding storage or retrieval mechanism
- Add new maintenance tools to replace this one

## Decisions

**Decision 1: Complete Removal vs. Deprecation**
- **Choice**: Complete removal (no deprecation period)
- **Why**: The tool is not documented in the CLAUDE.md protocol, not mentioned in any task, and likely not integrated into any workflows. Removing it outright is cleaner than deprecation.
- **Alternative**: Add a deprecation warning and leave as a no-op for 2 releases (rejected: unnecessary complexity)

**Decision 2: Documentation Updates**
- **Choice**: Update GetMcpDocumentation and GetInstructions to remove all references
- **Why**: Clients reading documentation should not see removed tools
- **Alternative**: Leave references as "deprecated" (rejected: could cause confusion)

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| **Backward compatibility break** | Clients calling RegenerateAllEmbeddings will get a "method not found" error. Mitigated by: MCP is internal-only, version bump, and release notes explaining the removal. |
| **Hidden embeddings issue** | If a bug is discovered where embeddings become stale, there's no manual refresh tool. Mitigated by: Automatic updates on write operations ensure consistency. If a bug is discovered, add a dedicated fix for the root cause (not a generic reorganization tool). |

## Implementation Notes

1. Remove the `RegenerateAllEmbeddings()` method from `KnowledgeTools.cs` (lines ~270-286)
2. Remove its McpServerTool attribute and Description
3. Clean up any documentation references
4. No database migration needed (embedding logic unchanged)
5. No configuration changes needed
