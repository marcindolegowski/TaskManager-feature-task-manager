using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accessory.Builder.CQRS.IntegrationEvents.Common;
using Accessory.Builder.MessageBus.Common;
using Accessory.Builder.MessageBus.IntegrationEvent;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace Accessory.Builder.MessageBus.ServiceBus.Common;

public class ServiceBusEventSubscriber : IEventSubscriber
{
    private readonly ChannelFactory _channelFactory;
    private readonly BusProperties _busProperties;
    private readonly IEventManager _eventManager;
    private readonly IIntegrationEventDispatcher _eventDispatcher;
    private string? _tag = null;

    public ServiceBusEventSubscriber(
        ChannelFactory channelFactory,
        BusProperties busProperties,
        IEventManager eventManager,
        IIntegrationEventDispatcher eventDispatcher)
    {
        _channelFactory = channelFactory;
        _busProperties = busProperties;
        _eventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    public async Task RegisterOnMessageHandlerAndReceiveMessages()
    {

        var channel = _channelFactory.CreateForConsumer();
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) => await MessageHandler(channel, ea).ConfigureAwait(false);
        string normalizedEventQueueName = _busProperties.EventQueueNameName ?? string.Empty;
        _tag = channel.BasicConsume(queue: normalizedEventQueueName, autoAck: false, consumer: consumer);
    }

    private async Task MessageHandler(IModel channel, BasicDeliverEventArgs ea)
    {
        var processed = false;
        var eventName = ea.RoutingKey;

        if (_eventManager.HasSubscriptionsForEvent(eventName))
        {
            var messageAsString = Encoding.UTF8.GetString(ea.Body.ToArray());

            var eventType = _eventManager.GetEventTypeByName(eventName);
            var integrationEvent = (IIntegrationEvent)JsonConvert.DeserializeObject(messageAsString, eventType)!;
            var subscriptions = _eventManager.GetHandlersForEvent(eventName).ToList();
            foreach (var subscription in subscriptions)
            {
                await _eventDispatcher.SendAsync(integrationEvent, subscription).ConfigureAwait(false);
            }

            processed = true;
        }

        if (processed)
        {
            channel.BasicAck(ea.DeliveryTag, false);
        }
    }

    public void Subscribe<T, TH>()
        where T : IIntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        _eventManager.AddSubscription<T, TH>();
    }

    public ValueTask DisposeAsync()
    {
        DisposeOrClose();
        return ValueTask.CompletedTask;
    }

    public Task CloseSubscriptionAsync()
    {
        DisposeOrClose();
        return Task.CompletedTask;
    }

    private void DisposeOrClose()
    {
        if (_tag != null)
        {
            var channel = _channelFactory.CreateForConsumer();
            channel.BasicCancel(_tag);
        }
    }
}