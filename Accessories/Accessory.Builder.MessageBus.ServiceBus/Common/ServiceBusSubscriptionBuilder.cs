using Accessory.Builder.MessageBus.Common;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace Accessory.Builder.MessageBus.ServiceBus.Common;

public class ServiceBusSubscriptionBuilder : IServiceBusSubscriptionBuilder
{
    private readonly ChannelFactory _channelFactory;
    private readonly BusProperties _busProperties;
    private readonly ILogger<ServiceBusSubscriptionBuilder> _logger;

    public ServiceBusSubscriptionBuilder(
        ChannelFactory channelFactory,
        BusProperties busProperties, 
        ILogger<ServiceBusSubscriptionBuilder> logger)
    {
        _channelFactory = channelFactory;
        _busProperties = busProperties;
        _logger = logger;
    }

    public async Task AddCustomRule(string eventType)
    {
        var channel = _channelFactory.CreateForConsumer();
        var exchange = _busProperties.EventExchangeName;
        var queue = _busProperties.EventQueueNameName;

        channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue: queue, exchange: exchange, routingKey: eventType);
    }

    public Task RemoveDefaultRule() => Task.CompletedTask;
}