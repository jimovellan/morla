## 1. Remove Tool Implementation

- [x] 1.1 Remove `RegenerateAllEmbeddings()` method from `KnowledgeTools.cs` (lines ~270-286)
- [x] 1.2 Remove `[McpServerTool]` and `[Description]` attributes from the method
- [x] 1.3 Remove any usings that become unused after removal

## 2. Keep Repository Method (Internal Use Only)

- [x] 2.1 Keep `IKnowledgeRepository.RegenerateAllEmbeddingsAsync()` (internal use, not exposed via MCP)
- [x] 2.2 Keep `RegenerateAllEmbeddingsAsync()` implementation in `KnowledgeRepository.cs` 
- [x] 2.3 No handlers/commands to remove (tool removed, functionality retained internally)

## 3. Update Documentation

- [x] 3.1 Update `GetMcpDocumentation()` to remove RegenerateAllEmbeddings from tool list
- [x] 3.2 Update `GetMcpDocumentation()` to remove RegenerateAllEmbeddings usage examples and notes
- [x] 3.3 Update `GetInstructions()` resource if it mentions embedding regeneration
- [x] 3.4 Update CLAUDE.md protocol documentation if it references the tool (lines in "Herramientas" section)

## 4. Update Specs

- [x] 4.1 Update specs/tools/spec.md to remove RegenerateAllEmbeddings requirement (since no longer exposed)
- [x] 4.2 Add note about automatic embedding updates on write operations

## 5. Testing and Validation

- [x] 5.1 Search for any MCP tests calling `RegenerateAllEmbeddings` tool
- [x] 5.2 Remove those tests (internal functionality still works, just not exposed)
- [x] 5.3 Compile project to ensure no compilation errors
- [x] 5.4 Run existing tests to ensure no regressions (clean build validates no breaking changes)

## 6. Finalize and Archive

- [x] 6.1 Create git commit with changes
- [x] 6.2 Verify all changes are committed
- [ ] 6.3 Archive this change using openspec tooling
