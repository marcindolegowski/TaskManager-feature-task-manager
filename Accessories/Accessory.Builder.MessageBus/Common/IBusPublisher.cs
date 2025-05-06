using System.Threading.Tasks;
using Accessory.Builder.MessageBus.IntegrationEvent;

namespace Accessory.Builder.MessageBus.Common;

public interface IBusPublisher<T>  where T : IIntegrationEvent
{
    Task PublishEventAsync(T payload, string? sessionId = null);
}