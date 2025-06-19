using Neoverse.DocumentManagement.Application.Interfaces;
using Neoverse.DocumentManagement.Domain.Entities;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.DocumentManagement.Application;

public class DocumentService
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<IEnumerable<Document>> GetDocumentsForCustomerAsync(Guid customerId, CancellationToken ct = default)
        => await _documentRepository.GetForCustomerAsync(customerId, ct);

    public async Task<Document?> GetByIdAsync(Guid id, string? language = null, CancellationToken ct = default)
        => await _documentRepository.GetByIdAsync(id, language, ct);

    public async Task<IEnumerable<Translation>> GetTranslationsAsync(Guid id, CancellationToken ct = default)
        => await _documentRepository.GetTranslationsAsync(id, ct);

    public async Task<Document> CreateDocumentAsync(string title, Guid customerId, CancellationToken ct = default)
    {
        var doc = new Document(title, customerId);
        await _documentRepository.AddAsync(doc, ct);
        return doc;
    }
}
