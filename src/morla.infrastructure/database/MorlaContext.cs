using Microsoft.EntityFrameworkCore;
using Morla.Domain.Models;

namespace Morla.Infrastructure.Database
{
    public class MorlaContext : DbContext
    {
        public MorlaContext(DbContextOptions<MorlaContext> options) : base(options)
        {
            
        }

        public DbSet<Knowledge> Knowledges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MorlaContext).Assembly);

        }

        
    }
}