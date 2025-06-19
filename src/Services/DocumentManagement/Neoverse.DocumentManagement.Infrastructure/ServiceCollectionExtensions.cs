using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Neoverse.SharedKernel.Interceptors;
using Neoverse.SharedKernel.Configuration;
using Neoverse.DocumentManagement.Application.Interfaces;
using Neoverse.SharedKernel.Events;

namespace Neoverse.DocumentManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentInfrastructure(this IServiceCollection services, string connectionString, string redisConnection, IConfiguration configuration, DatabaseProvider provider = DatabaseProvider.Postgres)
    {
        services.Configure<DataProtectionSettings>(configuration.GetSection("Encryption"));
        services.AddDbContext<DocumentDbContext>((sp, options) =>
        {
            switch (provider)
            {
                case DatabaseProvider.SqlServer:
                    options.UseSqlServer(connectionString);
                    break;
                case DatabaseProvider.Oracle:
                    options.UseOracle(connectionString);
                    break;
                default:
                    options.UseNpgsql(connectionString);
                    break;
            }
            options.AddInterceptors(
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<MultiLanguageSaveChangesInterceptor>(),
                sp.GetRequiredService<DataProtectionSaveChangesInterceptor>(),
                sp.GetRequiredService<DomainEventDispatchInterceptor>());
        });
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<DomainEventDispatchInterceptor>();
        return services;
    }
}
