using Neoverse.SharedKernel.Attributes;
using Neoverse.Customers.Domain.Events;
using Neoverse.SharedKernel.Entities;

namespace Neoverse.Customers.Domain.Entities;

public class Customer : BaseEntity
{
    [MultiLanguage]
    public string Name { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;

    protected Customer() {}

    public Customer(string name, Email email)
    {
        Name = name;
        Email = email;
        AddDomainEvent(new CustomerCreatedEvent(this));
    }

    public void UpdateName(string name)
    {
        Name = name;
    }
}


public record Email([property: MaskOnResponse] string Value);
