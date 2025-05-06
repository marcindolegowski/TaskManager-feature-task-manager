using System;
using Accessory.Builder.Core.Common;

namespace Accessory.Builder.MessageBus.IntegrationEvent;

public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; protected set; }

    public DateTimeOffset CreationDate { get; protected set; }

    protected IntegrationEvent(Guid id, DateTimeOffset creationDate)
    {
        Id = id;
        CreationDate = creationDate;
    }

    protected IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = SystemTime.Now();
    }
}