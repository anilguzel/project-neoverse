using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Neoverse.Customers.Application.Interfaces;
using Neoverse.Customers.Infrastructure.EventBus;
using Neoverse.Customers.Infrastructure.EventHandler;
using Neoverse.Customers.Infrastructure.Repository;
using StackExchange.Redis;
using Neoverse.SharedKernel.Configuration;
using Neoverse.SharedKernel.Events;
using Neoverse.SharedKernel.Interceptors;

namespace Neoverse.Customers.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerInfrastructure(this IServiceCollection services, string connectionString, string redisConnection, string kafkaBootstrap, IConfiguration configuration, DatabaseProvider provider = DatabaseProvider.Postgres)
    {
        services.Configure<DataProtectionSettings>(configuration.GetSection("Encryption"));
        services.AddDbContext<CustomerDbContext>((sp, options) =>
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
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddTransient<CustomerCreatedHandler>();
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddSingleton(new KafkaMessageBus(kafkaBootstrap));
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<DomainEventDispatchInterceptor>();
        return services;
    }
}
