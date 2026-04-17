# Soft-Delete Migration Strategy

## Pre-Deployment Checklist

- [ ] Backup production database
- [ ] Run full test suite
- [ ] Deploy to staging environment
- [ ] Verify all queries work correctly
- [ ] Test soft-delete and restore operations
- [ ] Load test on staging (optional)

## Deployment Steps

### Phase 1: Database Migration
1. Pull latest code with soft-delete changes
2. Build Release: `dotnet build -c Release`
3. Apply EF Core migration: `dotnet ef database update`
   - Adds `IsDeleted` and `DeletedAt` columns to Knowledges table
   - Creates index on `IsDeleted`
4. Verify migration completed successfully

### Phase 2: Application Deployment
1. Stop current application instance
2. Deploy new version with soft-delete support
3. Start application (MediatR auto-registers new handlers)
4. Verify MCP tools are available (DeleteKnowledgeByRowKey with hardDelete param, RestoreKnowledgeByRowKey)

### Phase 3: Verification
1. **Query Verification**: Run searches and verify results exclude soft-deleted
2. **API Testing**:
   - Test DELETE /knowledge/{rowKey} (soft-delete by default)
   - Test DELETE /knowledge/{rowKey}?hardDelete=true (hard-delete)
   - Test POST /knowledge/{rowKey}/restore (restore deleted)
3. **MCP Testing**: Run MCP client and verify both delete modes and restore
4. **Log Review**: Check for any soft-delete operation logs

## Rollback Plan

### Scenario 1: Pre-Migration Issues
- Don't apply migration
- Revert to previous version

### Scenario 2: Post-Migration Issues
- **Immediate**: Revert query filters by removing `.WhereNotDeleted()` calls (all entries visible again)
- **If needed**: Reverse migration with `dotnet ef migrations remove`

### Step-by-Step Rollback
```bash
# 1. Stop application
# 2. Restore backup (if needed)
# 3. Revert deployment to previous version
# 4. Remove migration:
dotnet ef migrations remove --project src/morla.infrastructure --startup-project src/morla.hosts.migrations
# 5. Start application with previous version
```

## Monitoring

### During Deployment
- Monitor application logs for errors
- Check database operation times (should have minimal impact)
- Verify soft-delete operations are logged correctly

### Post-Deployment
- Monitor search query performance (should be unaffected)
- Track soft-delete vs hard-delete usage
- Review audit logs for deletion operations

## Success Criteria

- ✅ Database migration completes without errors
- ✅ As a User, I can query knowledge without seeing soft-deleted entries
- ✅ As a User, I can soft-delete an entry and it's hidden from searches
- ✅ As a User, I can restore a soft-deleted entry and it reappears in searches
- ✅ As an Admin, I can force hard-delete an entry permanently
- ✅ No performance degradation observed

## Timeline Estimate

- **Database Migration**: 1-5 minutes (non-breaking, instant for reasonably-sized db)
- **Application Deployment**: 2-5 minutes
- **Testing & Verification**: 10-15 minutes
- **Total**: ~30 minutes

## Notes

- The migration is non-blocking and safe (adds nullable columns with defaults)
- All existing code continues to work without changes
- Soft-delete is transparent to end users (entries simply disappear from searches)
- Hard-delete requires explicit parameter/flag
