using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Neoverse.Customers.Application.Interfaces;
using Neoverse.Customers.Domain.Entities;
using Neoverse.SharedKernel.Entities;
using Neoverse.SharedKernel.Localization;
using StackExchange.Redis;

namespace Neoverse.Customers.Infrastructure.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _dbContext;
    private readonly IDatabase _cache;

    public CustomerRepository(CustomerDbContext db, IConnectionMultiplexer redis)
    {
        _dbContext = db;
        _cache = redis.GetDatabase();
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cached = await _cache.StringGetAsync($"customer:{id}");
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<Customer>(cached!);
        }
        var entity = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (entity != null)
        {
            await _cache.StringSetAsync($"customer:{id}", JsonSerializer.Serialize(entity));
        }
        return entity;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, string? language, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null && !string.IsNullOrEmpty(language))
        {
            var translations = await GetTranslationsAsync(id, cancellationToken);
            TranslationMerger.MergeTranslations(entity, translations, language);
        }
        return entity;
    }

    public async Task<IEnumerable<Translation>> GetTranslationsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Translations.Where(t => t.EntityId == id && t.EntityType == nameof(Customer)).ToListAsync(cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(customer, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.StringSetAsync($"customer:{customer.Id}", JsonSerializer.Serialize(customer));
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _dbContext.Customers.Update(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.StringSetAsync($"customer:{customer.Id}", JsonSerializer.Serialize(customer));
    }

    public async Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.KeyDeleteAsync($"customer:{customer.Id}");
    }
}
