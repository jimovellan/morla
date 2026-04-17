using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Morla.Domain.Models;
using Morla.Infrastructure.Database.Interceptors;

namespace Morla.Infrastructure.Database
{
    public class MorlaContext : DbContext
    {
        public MorlaContext(DbContextOptions<MorlaContext> options) : base(options)
        {
            
        }

        public DbSet<Knowledge> Knowledges { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.AddInterceptors(new VssInterceptor());
            
            // Suppress PendingModelChangesWarning during migrations
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MorlaContext).Assembly);

        }

        
    }
}