# Release Notes - Soft-Delete Feature

## Version: v0.0.7x (Next Release)

### New Features

#### Soft-Delete Knowledge Entries
Knowledge entries can now be soft-deleted, preserving data while marking entries as inactive. This enables:
- **Data Compliance**: Maintain audit trails without permanent loss
- **Recovery**: Restore accidentally deleted entries
- **Safety**: Default behavior is soft-delete; hard-delete requires explicit parameter

**Default Behavior Change**: 
- `DELETE /knowledge/{rowKey}` now performs **soft-delete** (marks entry, preserves data)
- Use `?hardDelete=true` for permanent deletion

#### New Endpoints
- `POST /knowledge/{rowKey}/restore` - Restore soft-deleted entries

#### New MCP Tools
- `RestoreKnowledgeByRowKey(rowKey)` - Restore soft-deleted entries via MCP

### Database Changes

**Migration**: `AddSoftDeleteToKnowledge`
- Adds `IsDeleted` column (bool, default: false)
- Adds `DeletedAt` column (datetime?, nullable)
- Creates index on `IsDeleted` for performance

**Backward Compatibility**: ✅ Non-breaking
- Existing entries default to IsDeleted=false (active)
- All existing queries continue to work without code changes

### Breaking Changes

None. The feature is backward compatible.

### Performance Impact

- **Minimal**: Single WHERE clause on IsDeleted column
- **Index added** on IsDeleted for query optimization
- **No significant impact** on existing operations

### Testing

- ✅ Unit tests for soft-delete and restore commands
- ✅ Integration tests for query filtering
- ✅ Manual testing recommended on staging

### Migration Instructions

1. **Deploy database migration**: `dotnet ef database update`
2. **Verify** all queries exclude soft-deleted entries
3. **Test** restore functionality
4. **Monitor** for any query anomalies

### Rollback Plan

1. Remove `.WhereNotDeleted()` filters (reverts to returning all entries)
2. Drop IsDeleted and DeletedAt columns (requires migration)

### Known Limitations

- Soft-deleted entries cannot be directly searched (by design)
- Hard-delete is permanent and irreversible
- No scheduled automatic hard-delete policy (future enhancement)

### Support

For questions or issues, refer to the feature documentation: `docs/SOFT_DELETE_FEATURE.md`
