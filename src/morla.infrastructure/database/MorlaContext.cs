using Microsoft.EntityFrameworkCore;
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
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MorlaContext).Assembly);

        }

        
    }
}