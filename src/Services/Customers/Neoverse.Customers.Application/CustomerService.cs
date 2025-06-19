using Neoverse.Customers.Application.Interfaces;
using Neoverse.Customers.Domain.Entities;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.Customers.Application;

public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer> CreateCustomerAsync(string name, string email, CancellationToken ct = default)
    {
        var customer = new Customer(name, new Email(email));
        await _customerRepository.AddAsync(customer, ct);
        return customer;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, string? language = null, CancellationToken ct = default)
        => await _customerRepository.GetByIdAsync(id, language, ct);

    public async Task<IEnumerable<Translation>> GetTranslationsAsync(Guid id, CancellationToken ct = default)
        => await _customerRepository.GetTranslationsAsync(id, ct);
}
