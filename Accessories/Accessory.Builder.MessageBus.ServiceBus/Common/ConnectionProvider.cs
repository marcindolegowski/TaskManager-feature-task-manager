using RabbitMQ.Client;

namespace Accessory.Builder.MessageBus.ServiceBus.Common;

public sealed class ConnectionProvider : IConnectionProvider
{
    public ConnectionProvider(IConnection consumerConnection, IConnection producerConnection)
    {
        ConsumerConnection = consumerConnection;
        ProducerConnection = producerConnection;
    }

    public IConnection ConsumerConnection { get; }
    public IConnection ProducerConnection { get; }
}
