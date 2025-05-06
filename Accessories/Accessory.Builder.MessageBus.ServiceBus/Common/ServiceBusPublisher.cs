using System;
using System.Text;
using System.Threading.Tasks;
using Accessory.Builder.MessageBus.Common;
using Accessory.Builder.MessageBus.IntegrationEvent;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Accessory.Builder.MessageBus.ServiceBus.Common;

public class ServiceBusPublisher<T> : IBusPublisher<T> where T : IIntegrationEvent
{
    private readonly ChannelFactory _channelFactory;
    private readonly BusProperties _busProperties;
    private readonly ILogger<ServiceBusPublisher<T>> _logger;

    public ServiceBusPublisher(
        ChannelFactory channelFactory,
        BusProperties busProperties,
        ILogger<ServiceBusPublisher<T>> logger)
    {
        _channelFactory = channelFactory;
        _busProperties = busProperties;
        _logger = logger;
    }

    public async Task PublishEventAsync(T payload, string? sessionId)
    {
        var eventType = MessageBus.Extensions.GetEventFor<T>();
        try
        {
            var message = CreateMessage(eventType, payload);
            var channel = _channelFactory.CreateForProducer();

            var prop = channel.CreateBasicProperties();

            channel.BasicPublish(
                exchange: _busProperties.EventExchangeName,
                routingKey: eventType,
                basicProperties: prop,
                body: message,
                mandatory: true);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    private byte[]? CreateMessage(string? messageSubject, object payload)
    {
        string data = payload is string ? (string)payload : JsonConvert.SerializeObject(payload);
        var body = Encoding.UTF8.GetBytes(data);
        return body;
    }
}