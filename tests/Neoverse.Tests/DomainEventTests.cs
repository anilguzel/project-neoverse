using System;
using System.Threading;
using System.Threading.Tasks;
using Neoverse.SharedKernel.Events;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

record TestEvent : DomainEventBase
{
    public TestEvent() : base(DateTime.UtcNow) { }
}

class TestHandler : IDomainEventHandler<TestEvent>
{
    public bool Handled { get; private set; }
    public Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Handled = true;
        return Task.CompletedTask;
    }
}

public class DomainEventTests
{
    [Fact]
    public async Task Dispatcher_InvokesRegisteredHandler_WhenDispatched()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<TestEvent>, TestHandler>();
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDomainEventDispatcher>();
        var ev = new TestEvent();
        await dispatcher.DispatchAsync(new[] { ev });
        var handler = provider.GetRequiredService<IDomainEventHandler<TestEvent>>() as TestHandler;
        Assert.True(handler!.Handled);
    }
}
