using Accessory.Builder.Core.Common;
using Accessory.Builder.MessageBus.IntegrationEvent;

namespace TaskManager.Application.Task.Events;

public class RemovalTaskEvent : IntegrationEvent
{
    public string Name { get; }

    public RemovalTaskEvent(string name)
    {
        Name = name;
        CreationDate = SystemTime.OffsetNow();
    }
}