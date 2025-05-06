using System.Threading.Tasks;

namespace Accessory.Builder.MessageBus.IntegrationEvent;

public interface IEventSubscriber
{
    Task RegisterOnMessageHandlerAndReceiveMessages();
    void Subscribe<T, TH>()
        where T : IIntegrationEvent
        where TH : IIntegrationEventHandler<T>;
    Task CloseSubscriptionAsync();
    ValueTask DisposeAsync();
}