using Xunit;
using Morla.Domain.Models;
using Morla.Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Morla.Infrastructure.Tests.Database.Extensions;

public class KnowledgeQueryExtensionsTests
{
    /// <summary>
    /// Integration test: Verify WhereNotDeleted() extension filters out soft-deleted entries
    /// This test uses in-memory DbContext for testing without database
    /// </summary>
    [Fact]
    public async Task WhereNotDeleted_ShouldExcludeSoftDeletedEntries()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<Morla.Infrastructure.Database.MorlaContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        using (var context = new Morla.Infrastructure.Database.MorlaContext(options))
        {
            // Create test data
            var activeEntry = new Knowledge
            {
                RowId = "active-entry",
                Title = "Active Knowledge",
                Summary = "Active Summary",
                Content = "Active Content",
                IsDeleted = false,
                DeletedAt = null
            };

            var softDeletedEntry = new Knowledge
            {
                RowId = "deleted-entry",
                Title = "Deleted Knowledge",
                Summary = "Deleted Summary",
                Content = "Deleted Content",
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            };

            context.Knowledges.Add(activeEntry);
            context.Knowledges.Add(softDeletedEntry);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = new Morla.Infrastructure.Database.MorlaContext(options))
        {
            var results = await context.Knowledges
                .WhereNotDeleted()
                .ToListAsync();

            // Only active entry should be returned
            Assert.Single(results);
            Assert.Equal("active-entry", results[0].RowId);
        }
    }

    /// <summary>
    /// Integration test: Verify without WhereNotDeleted() both active and deleted entries are returned
    /// </summary>
    [Fact]
    public async Task WithoutWhereNotDeleted_ShouldIncludeSoftDeletedEntries()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<Morla.Infrastructure.Database.MorlaContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        using (var context = new Morla.Infrastructure.Database.MorlaContext(options))
        {
            context.Knowledges.Add(new Knowledge
            {
                RowId = "active-entry",
                Title = "Active",
                Summary = "Active",
                Content = "Active",
                IsDeleted = false
            });

            context.Knowledges.Add(new Knowledge
            {
                RowId = "deleted-entry",
                Title = "Deleted",
                Summary = "Deleted",
                Content = "Deleted",
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = new Morla.Infrastructure.Database.MorlaContext(options))
        {
            var results = await context.Knowledges.ToListAsync();

            // Both entries should be returned
            Assert.Equal(2, results.Count);
        }
    }
}
