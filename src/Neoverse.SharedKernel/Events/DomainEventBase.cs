namespace Neoverse.SharedKernel.Events;

public abstract record DomainEventBase(DateTime OccurredOn) : IDomainEvent;
