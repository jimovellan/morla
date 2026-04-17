# Delta: Tools - Remove Manual Embedding Regeneration

## REMOVED Requirements

### Requirement: RegenerateAllEmbeddings - Rebuild Search Index

**Reason**: Embeddings are automatically regenerated on SetKnowledge and UpdateKnowledgeById operations, making manual reorganization unnecessary. This tool was rarely used and adds unnecessary complexity to the MCP interface. Removing it simplifies the tools contract.

**Migration**: No migration needed. The automatic embedding regeneration triggered by write operations provides the same guarantees without requiring manual intervention. If embeddings become stale (which should not happen), the root cause should be fixed rather than relying on a manual rebuild tool.

---

## Summary

This delta removes the `RegenerateAllEmbeddings` tool from the MCP interface. All existing tools (SetKnowledge, SearchKnowledge, GetKnowledgeById, UpdateKnowledgeById, DeleteKnowledgeByRowKey) continue to work unchanged. Embedding management is now fully automatic and transparent.
