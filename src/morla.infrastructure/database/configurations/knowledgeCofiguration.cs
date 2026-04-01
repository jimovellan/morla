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
        }
    }
}