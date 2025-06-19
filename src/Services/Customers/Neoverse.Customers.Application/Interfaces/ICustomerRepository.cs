using Neoverse.Customers.Domain.Entities;
using Neoverse.SharedKernel.Entities;
using Neoverse.SharedKernel.Repositories;

namespace Neoverse.Customers.Application.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByIdAsync(Guid id, string? language, CancellationToken cancellationToken = default);
    Task<IEnumerable<Translation>> GetTranslationsAsync(Guid id, CancellationToken cancellationToken = default);
}
