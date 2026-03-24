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
            builder.HasKey(k => k.Id);
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