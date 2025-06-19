using System;

namespace Neoverse.SharedKernel.Entities;

public class Translation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
