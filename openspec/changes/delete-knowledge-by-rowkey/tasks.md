# Delete Knowledge by RowKey - Implementation Tasks

## Overview
Implement DeleteKnowledgeByRowKey MCP tool following CQRS pattern. Tasks sequenced for dependency order.

---

## Tasks

### Phase 1: Repository Layer (Foundation)

#### Task 1.1: ✅ COMPLETED - Repository Method Already Exists
**Status**: Already implemented as `DeleteAsync(string rowId)` in `KnowledgeRepository`  
**File**: `src/morla.infrastructure/database/repositories/KnowledgeRepository.cs`  
**Note**: Method exists and properly deletes knowledge by RowId. Leveraging existing implementation.

---

#### Task 1.2: ✅ COMPLETED - Repository Implementation Verified  
**Status**: Verified existing `DeleteAsync(string rowId)` implementation  
**Implementation**: 
```csharp
public async Task DeleteAsync(string rowId)
{
    var knowledge = await GetByIdAsync(rowId);
    if (knowledge != null)
    {
        _context.Knowledges.Remove(knowledge);
        await _context.SaveChangesAsync();
    }
}
```
**Verified**:
- ✅ Queries by RowId (string GUID)
- ✅ Removes from DbContext
- ✅ Persists changes to DB
- ✅ No compilation errors

---

### Phase 2: Application Layer (Command & Handler)

#### Task 2.1: ✅ COMPLETED - DeleteKnowledgeCommand Created
**File**: `src/morla.application/UseCases/Commands/DeleteKnowledge/DeleteKnowledgeCommand.cs`  
**Status**: Implemented and compiles successfully

**Implementation**:
```csharp
public record DeleteKnowledgeCommand(string RowKey) : IRequest<DeleteKnowledgeResponse>;

public record DeleteKnowledgeResponse(
    bool Success,
    string Message,
    string? DeletedRowKey = null,
    string? DeletedId = null,
    string? Error = null);
```

**Verified**:
- ✅ MediatR IRequest<T> pattern
- ✅ RowKey parameter for deletion
- ✅ Response records for success/error
- ✅ Proper error reporting

---

#### Task 2.2: ✅ COMPLETED - DeleteKnowledgeCommandHandler Created
**File**: `src/morla.application/UseCases/Commands/DeleteKnowledge/DeleteKnowledgeCommandHandler.cs`  
**Status**: Implemented, auto-registered via MediatR assembly scan

**Implementation**:
- Handles DeleteKnowledgeCommand
- Validates RowKey exists via repository.GetByIdAsync(rowKey)
- Calls repository.DeleteAsync(rowKey)
- Returns appropriate response (success or NOT_FOUND error)
- Includes Serilog logging per conventions
- Exception handling with error responses

**Verified**:
- ✅ IRequestHandler<T, TResponse> implementation
- ✅ Repository injection pattern
- ✅ Logging with Serilog
- ✅ Error handling (null check, exceptions)
- ✅ Auto-registered by MediatR.AddMediatR() in ApplicationExtensions
- ✅ Code compiles
   ```csharp
   namespace Morla.Application.UseCases.Commands.DeleteKnowledge;
   
   public record DeleteKnowledgeCommand(string RowKey) : IRequest<DeleteKnowledgeResponse>;
   
   public record DeleteKnowledgeResponse(
       bool Success,
       string Message,
       string? DeletedRowKey = null,
       string? DeletedId = null,
       string? Error = null);
   ```
3. Verify file structure matches other commands

**Definition of Done**:
- ✅ Record type with RowKey parameter
- ✅ Implements `IRequest<T>`
- ✅ Response record defined
- ✅ Namespace matches pattern
- ✅ Can be referenced by handler

---

#### Task 2.2: ✅ COMPLETED - DeleteKnowledgeCommandHandler Created
**File**: `src/morla.application/UseCases/Commands/DeleteKnowledge/DeleteKnowledgeCommandHandler.cs`  
**Status**: Implemented, auto-registered via MediatR assembly scan

**Implementation**:
- Handles DeleteKnowledgeCommand
- Validates RowKey exists via repository.GetByIdAsync(rowKey)
- Calls repository.DeleteAsync(rowKey)
- Returns appropriate response (success or NOT_FOUND error)
- Includes Serilog logging per conventions
- Exception handling with error responses

**Verified**:
- ✅ IRequestHandler<T, TResponse> implementation
- ✅ Repository injection pattern
- ✅ Logging with Serilog
- ✅ Error handling (null check, exceptions)
- ✅ Auto-registered by MediatR.AddMediatR() in ApplicationExtensions
- ✅ Code compiles

---

### Phase 3: MCP Integration

#### Task 3.1: ✅ COMPLETED - MCP Tool Method Added to KnowledgeTools
**File**: `src/morla.infrastructure/tools/KnowledgeTools.cs`  
**Status**: Implemented and compiles successfully

**Implementation**:
- Added method `DeleteKnowledgeByRowKey(string rowKey)` with `[McpServerTool]` attribute
- Integrated with MediatR: sends `DeleteKnowledgeCommand` via mediator
- Validates rowKey parameter
- Returns properly formatted JSON response with success/error info
- Includes comprehensive documentation string in `[Description]` attribute

**Method Signature**:
```csharp
[McpServerTool, Description("...")]
public async Task<object> DeleteKnowledgeByRowKey(string rowKey)
```

**Verified**:
- ✅ Method decorated with `[McpServerTool]`
- ✅ Calls mediator.Send(new DeleteKnowledgeCommand(rowKey))
- ✅ Returns object with success/deleted info
- ✅ Serilog logging per conventions
- ✅ Error handling
- ✅ Added import: `using Morla.Application.UseCases.Commands.DeleteKnowledge;`
- ✅ Code compiles

---

#### Task 3.2: ✅ COMPLETED - Handler Auto-Registered via Assembly Scan
**Status**: No manual registration needed

**Architecture**:
- MCP server uses: `.WithToolsFromAssembly(typeof(KnowledgeTools).Assembly)`
- KnowledgeTools is decorated with `[McpServerToolType]`
- DeleteKnowledgeByRowKey method has `[McpServerTool]` attribute
- Automatic discovery and registration at server startup

**Verified**:
- ✅ Tool discoverable via MCP protocol
- ✅ Automatically registered at server init
- ✅ Tool name: `delete_knowledge_by_rowkey` (inferred from method name)
- ✅ No manual registry code needed
- ✅ Code compiles

---
   ```csharp
   services.AddScoped<DeleteKnowledgeRowKeyHandler>();
   registry.RegisterHandler<DeleteKnowledgeRowKeyHandler>();
   ```
3. Ensure it's registered before MCP starts

**Definition of Done**:
- ✅ Handler registered in registry
- ✅ Tool name matches: `delete_knowledge_by_rowkey`
- ✅ Registration happens at MCP init time
- ✅ No compilation errors

---

### Phase 4: Testing & Verification

#### Task 4.1: Test Repository Layer
**File**: `Create test file in appropriate test project`  
**Effort**: 20 min  
**Type**: Unit tests

Steps:
1. Create test file: `Tests/Morla.Infrastructure.Tests/Repository/KnowledgeRepositoryDeleteTests.cs`
2. Write tests:
   - Test delete existing knowledge by rowKey → returns entity
   - Test delete non-existent rowKey → returns null
   - Test verify entity deleted from DB
3. Run tests locally

**Definition of Done**:
- ✅ All tests pass
- ✅ 3+ test cases (happy path + error cases)
- ✅ Mock or real DB context used appropriately

---

#### Task 4.2: Test Command Handler
**File**: `Tests/Morla.Application.Tests/UseCases/Commands/DeleteKnowledgeTests.cs`  
**Effort**: 20 min  
**Type**: Unit tests

Steps:
1. Create test file
2. Write tests:
   - Mock repository returns knowledge → handler returns success
   - Mock repository returns null → handler returns error
   - Verify logging called
3. Run tests

**Definition of Done**:
- ✅ All tests pass
- ✅ Happy path and error cases covered
- ✅ Logging verified

---

#### Task 4.3: Manual E2E Test
**File**: Test via MCP client  
**Effort**: 15 min  
**Type**: Integration test

Steps:
1. Start MCP server: `dotnet run mcp` (from `src/morla.hosts`)
2. Create a test knowledge entry (using SetKnowledge tool)
3. Call `delete_knowledge_by_rowkey` with the rowKey
4. Verify response shows success
5. Query knowledge (should be gone)
6. Try deleting again (should return error)

**Definition of Done**:
- ✅ Tool callable from MCP client
- ✅ Returns correct response format
- ✅ Entity actually deleted from DB
- ✅ Error handling works (missing rowKey)

---

#### Task 4.4: Documentation & Finalization
**File**: `docs/morla.instructions.md` or MCP schema doc  
**Effort**: 15 min  
**Type**: Documentation

Steps:
1. Add to MCP tools documentation:
   ```markdown
   ### delete_knowledge_by_rowkey
   
   Delete a knowledge entry by its RowKey (external unique identifier).
   
   **Input**:
   - `rowKey` (string, required): The external RowKey identifier
   
   **Returns**: Success/error response with deleted entity details
   
   **Example**:
   ```
   delete_knowledge_by_rowkey({ rowKey: "topic:project:timestamp" })
   ```
   ```
2. Update README if needed (mention full CRUD support now)
3. Run build: `dotnet build`

**Definition of Done**:
- ✅ Documentation added
- ✅ Build succeeds
- ✅ No warnings or errors
- ✅ Ready for merge/release

---

## Summary

**Total Effort**: ~3.5-4 hours (with testing)

**Dependency Order**:
1. Phase 1 (Repository) → Foundation
2. Phase 2 (Application) → Depends on Phase 1
3. Phase 3 (MCP) → Depends on Phases 1 & 2
4. Phase 4 (Testing) → Verification after all phases

**Deliverables**:
- ✅ DeleteKnowledge CQRS command + handler
- ✅ IKnowledgeRepository extension + implementation
- ✅ MCP DeleteKnowledgeRowKeyHandler tool
- ✅ Registered in MCP registry
- ✅ Tests & documentation
