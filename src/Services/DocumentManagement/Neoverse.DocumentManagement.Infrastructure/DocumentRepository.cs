using Microsoft.EntityFrameworkCore;
using Neoverse.DocumentManagement.Application.Interfaces;
using Neoverse.DocumentManagement.Domain.Entities;
using Neoverse.SharedKernel.Repositories;
using Neoverse.SharedKernel.Entities;
using Neoverse.SharedKernel.Localization;
using StackExchange.Redis;
using System.Text.Json;

namespace Neoverse.DocumentManagement.Infrastructure;

public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentDbContext _dbContext;
    private readonly IDatabase _cache;

    public DocumentRepository(DocumentDbContext dbContext, IConnectionMultiplexer redis)
    {
        _dbContext = dbContext;
        _cache = redis.GetDatabase();
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cached = await _cache.StringGetAsync($"document:{id}");
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<Document>(cached!);
        }
        var entity = await _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (entity != null)
        {
            await _cache.StringSetAsync($"document:{id}", JsonSerializer.Serialize(entity));
        }
        return entity;
    }

    public async Task<Document?> GetByIdAsync(Guid id, string? language, CancellationToken cancellationToken = default)
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
        => await _dbContext.Translations.Where(t => t.EntityId == id && t.EntityType == nameof(Document)).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Document>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await _dbContext.Documents.Where(d => d.CustomerId == customerId).ToListAsync(cancellationToken);

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _dbContext.Documents.AddAsync(document, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.StringSetAsync($"document:{document.Id}", JsonSerializer.Serialize(document));
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _dbContext.Documents.Update(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.StringSetAsync($"document:{document.Id}", JsonSerializer.Serialize(document));
    }

    public async Task DeleteAsync(Document document, CancellationToken cancellationToken = default)
    {
        _dbContext.Documents.Remove(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _cache.KeyDeleteAsync($"document:{document.Id}");
    }
}
