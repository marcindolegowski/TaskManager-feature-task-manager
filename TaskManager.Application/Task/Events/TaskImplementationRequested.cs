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

    // Identity of the developer who requested the work; the token is resolved at the
    // edge (never travels on the bus). Null => fall back to the team/ambient credential.
    public string? UserId { get; }

    public TaskImplementationRequested(Guid taskId, string name, string description, string repositoryUrl, string? userId)
    {
        TaskId = taskId;
        Name = name;
        Description = description;
        RepositoryUrl = repositoryUrl;
        UserId = userId;
        CreationDate = SystemTime.OffsetNow();
    }
}
