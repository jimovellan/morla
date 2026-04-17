using Microsoft.EntityFrameworkCore;
using Morla.Domain.Models;

namespace Morla.Infrastructure.Database.Extensions;

/// <summary>
/// Extension methods for IQueryable<Knowledge> to apply soft-delete filtering
/// </summary>
public static class KnowledgeQueryExtensions
{
    /// <summary>
    /// Filters out soft-deleted knowledge entries (excludes IsDeleted = true)
    /// </summary>
    /// <param name="query">The queryable source</param>
    /// <returns>Filtered query excluding soft-deleted entries</returns>
    public static IQueryable<Knowledge> WhereNotDeleted(this IQueryable<Knowledge> query)
    {
        return query.Where(k => !k.IsDeleted);
    }
}
