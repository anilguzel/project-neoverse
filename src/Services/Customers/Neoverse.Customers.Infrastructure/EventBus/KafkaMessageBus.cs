using Confluent.Kafka;

namespace Neoverse.Customers.Infrastructure.EventBus;

public class KafkaMessageBus
{
    private readonly IProducer<Null, string> _producer;

    public KafkaMessageBus(string bootstrapServers)
    {
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public Task ProduceAsync(string topic, string message, CancellationToken ct = default)
        => _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }, ct);
}
