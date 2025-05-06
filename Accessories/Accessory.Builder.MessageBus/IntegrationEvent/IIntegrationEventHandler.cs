using System.Threading.Tasks;

namespace Accessory.Builder.MessageBus.IntegrationEvent;

public interface IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    Task HandleAsync(TIntegrationEvent integrationEvent);
}