using Accessory.Builder.Core.Common;
using Accessory.Builder.MessageBus.IntegrationEvent;

namespace TaskManager.Application.Task.Events;

public class TaskCompletedEvent : IntegrationEvent
{
    public string Name { get; }

    public TaskCompletedEvent(string name)
    {
        Name = name;
        CreationDate = SystemTime.OffsetNow();
    }
}