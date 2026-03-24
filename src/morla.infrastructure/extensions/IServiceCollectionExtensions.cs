using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Morla.Domain.Repository;
using Morla.Infrastructure.Database;
using morla.infrastructure.repositories;
using Morla.Application.Extensions;

namespace Morla.Infrastructure.Extensions;


public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddDbContext<MorlaContext>(options =>
            options.UseSqlite("Data Source=morla.db"));

        // =====================
        // REGISTER REPOSITORIES
        // =====================
        services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();
        services.AddApplicationServices();

        //mirar si hay migraciones pendientes y aplicarlas
        using (var scope = services.BuildServiceProvider().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MorlaContext>();
            context.Database.Migrate();
        }
        
        return services;
    }
}