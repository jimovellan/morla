using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Morla.Domain.Models;

namespace Morla.Infrastructure.Database.Configurations
{
    public class KnowledgeConfiguration : IEntityTypeConfiguration<Knowledge>
    {
        public void Configure(EntityTypeBuilder<Knowledge> builder)
        {
            builder.ToTable("Knowledges");
            
            // Id como PRIMARY KEY autoincremental (vinculado con vec0 rowid)
            builder.HasKey(k => k.Id);
            builder.Property(k => k.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("Id");
            
            // RowId como UNIQUE INDEX (GUID para referencia externa/búsquedas)
            builder.HasAlternateKey(k => k.RowId);
            builder.Property(k => k.RowId)
                .IsRequired()
                .HasMaxLength(36)  // Longitud de GUID
                .HasColumnName("RowId");
            
            builder.Property(k => k.Title).IsRequired().HasMaxLength(200);
            builder.Property(k => k.Content).IsRequired();
            builder.Property(k => k.UpdatedAt).IsRequired();
            builder.Property(k => k.CreatedAt).IsRequired();
            builder.Property(k => k.Topic).HasMaxLength(100);
            builder.Property(k => k.Project).HasMaxLength(250);
            builder.Property(k => k.Summary).IsRequired();
            
            // Soft-delete properties
            builder.Property(k => k.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("IsDeleted");
            
            builder.Property(k => k.DeletedAt)
                .HasColumnName("DeletedAt")
                .IsRequired(false);  // Nullable
            
            // Index for query performance (filter out soft-deleted entries)
            builder.HasIndex(k => k.IsDeleted)
                .HasDatabaseName("idx_knowledges_isdeleted");
        }
    }
}