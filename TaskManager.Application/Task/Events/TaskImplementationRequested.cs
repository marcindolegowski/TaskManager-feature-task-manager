using System;
using Accessory.Builder.Core.Common;
using Accessory.Builder.MessageBus.IntegrationEvent;

namespace TaskManager.Application.Task.Events;

public class TaskImplementationRequested : IntegrationEvent
{
    public Guid TaskId { get; }

    public string Name { get; }

    public string Description { get; }

    public string RepositoryUrl { get; }

    public TaskImplementationRequested(Guid taskId, string name, string description, string repositoryUrl)
    {
        TaskId = taskId;
        Name = name;
        Description = description;
        RepositoryUrl = repositoryUrl;
        CreationDate = SystemTime.OffsetNow();
    }
}
