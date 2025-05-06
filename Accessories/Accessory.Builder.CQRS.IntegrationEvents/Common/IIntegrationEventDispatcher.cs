using System;
using System.Threading.Tasks;
using Accessory.Builder.MessageBus.IntegrationEvent;

namespace Accessory.Builder.CQRS.IntegrationEvents.Common;

public interface IIntegrationEventDispatcher
{
    Task SendAsync<T>(
        T @event,
        Type? handlerType = null) where T : class, IIntegrationEvent;
}