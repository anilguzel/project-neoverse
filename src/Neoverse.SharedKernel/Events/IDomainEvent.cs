namespace Neoverse.SharedKernel.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
