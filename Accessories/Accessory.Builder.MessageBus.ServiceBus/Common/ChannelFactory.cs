using RabbitMQ.Client;
using System.Threading;

namespace Accessory.Builder.MessageBus.ServiceBus.Common;

public sealed class ChannelFactory
{
    private readonly IConnectionProvider _connectionProvider;

    public ChannelFactory(IConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    private readonly ThreadLocal<IModel> _consumerCache = new(true);
    private readonly ThreadLocal<IModel> _producerCache = new(true);

    public IModel CreateForProducer() => Create(_connectionProvider.ProducerConnection, _producerCache);

    public IModel CreateForConsumer() => Create(_connectionProvider.ConsumerConnection, _consumerCache);

    private IModel Create(IConnection connection, ThreadLocal<IModel> cache)
    {
        if (cache.Value is not null)
        {
            return cache.Value;
        }

        var channel = connection.CreateModel();
        cache.Value = channel;
        return channel;
    }

    public void Dispose()
    {
        foreach (var channel in _consumerCache.Values)
        {
            channel.Dispose();
        }
        foreach (var channel in _producerCache.Values)
        {
            channel.Dispose();
        }

        _consumerCache.Dispose();
        _producerCache.Dispose();
    }
}
