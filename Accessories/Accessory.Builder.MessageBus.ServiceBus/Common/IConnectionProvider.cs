using RabbitMQ.Client;

namespace Accessory.Builder.MessageBus.ServiceBus.Common;

public interface IConnectionProvider
{
    IConnection ConsumerConnection { get; }
    IConnection ProducerConnection { get; }
}
