using System;

namespace Accessory.Builder.MessageBus.IntegrationEvent;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
}