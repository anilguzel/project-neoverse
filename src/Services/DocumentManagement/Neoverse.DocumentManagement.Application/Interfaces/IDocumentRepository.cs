using Neoverse.DocumentManagement.Domain.Entities;
using Neoverse.SharedKernel.Entities;
using Neoverse.SharedKernel.Repositories;

namespace Neoverse.DocumentManagement.Application.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdAsync(Guid id, string? language, CancellationToken cancellationToken = default);
    Task<IEnumerable<Translation>> GetTranslationsAsync(Guid id, CancellationToken cancellationToken = default);
}
