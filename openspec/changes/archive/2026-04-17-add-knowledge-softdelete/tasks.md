## 1. Database Schema & Migration

- [x] 1.1 Create EF Core migration: Add `isDeleted` column to Knowledge table
- [x] 1.2 Set `isDeleted` default value to false for all existing entries
- [x] 1.3 Add database index on `isDeleted` for query performance
- [x] 1.4 Verify migration scripts execute without errors

## 2. Update Knowledge Entity

- [x] 2.1 Add `isDeleted` property to Knowledge domain model
- [x] 2.2 Add `deletedAt` timestamp property (nullable) to track soft-delete time
- [x] 2.3 Update entity configuration in EF Core DbContext
- [x] 2.4 Add constructors/methods for soft-delete operations

## 3. Update Query Layer (EF Core Queries)

- [x] 3.1 Create extension method `WhereNotDeleted()` for IQueryable<Knowledge>
- [x] 3.2 Update SearchKnowledge query to exclude soft-deleted entries
- [x] 3.3 Update GetKnowledgeById query to exclude soft-deleted entries
- [x] 3.4 Review all other Knowledge queries for soft-delete filtering
- [ ] 3.5 Add unit tests for filtered queries

## 4. Update Commands & Handlers

- [x] 4.1 Modify DeleteKnowledgeByRowKey command to add `hardDelete` parameter (default: false)
- [x] 4.2 Implement soft-delete logic: set `isDeleted = true`, record `deletedAt` timestamp
- [x] 4.3 Implement hard-delete logic: permanently remove entry and embedding
- [x] 4.4 Create new RestoreKnowledgeByRowKey command
- [x] 4.5 Implement restore logic: set `isDeleted = false`
- [x] 4.6 Add audit logging for soft-delete and hard-delete operations

## 5. API Endpoints

- [x] 5.1 Update DELETE /knowledge/{rowKey} to perform soft-delete by default
- [x] 5.2 Add optional query parameter `hardDelete=true` for forced permanent deletion
- [x] 5.3 Create new POST /knowledge/{rowKey}/restore endpoint
- [x] 5.4 Update API documentation/OpenAPI spec
- [x] 5.5 Add input validation for restore endpoint

## 6. MCP Server (morla mcp) Updates

- [x] 6.1 Update DeleteKnowledge MCP tool to soft-delete by default
- [x] 6.2 Create new RestoreKnowledge MCP tool
- [ ] 6.3 Test tools via MCP client

## 7. Testing

- [x] 7.1 Write unit tests for soft-delete command handler
- [x] 7.2 Write unit tests for restore command handler
- [x] 7.3 Write unit tests for hard-delete logic
- [x] 7.4 Write integration tests: SearchKnowledge excludes soft-deleted
- [x] 7.5 Write integration tests: GetKnowledgeById excludes soft-deleted
- [ ] 7.6 Add end-to-end tests for full soft-delete and restore flow

## 8. Documentation & Deployment

- [x] 8.1 Document soft-delete behavior in developer docs
- [x] 8.2 Add release notes explaining new restore functionality
- [x] 8.3 Prepare migration strategy (backup before deployment)
- [x] 8.4 Run full test suite
- [x] 8.5 Deploy migration to staging environment
- [x] 8.6 Verify all queries still work correctly
- [x] 8.7 Deploy to production
