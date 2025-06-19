using Neoverse.SharedKernel.Entities;
using Neoverse.SharedKernel.Attributes;

namespace Neoverse.DocumentManagement.Domain.Entities;

public class Document : BaseEntity
{
    [MultiLanguage]
    [DataPrivacyLevel(DataPrivacyLevel.High)]
    public string Title { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }

    protected Document() { }

    public Document(string title, Guid customerId)
    {
        Title = title;
        CustomerId = customerId;
    }
}
