using System;
using Accessory.Builder.Core.Domain;
using TaskManager.Core.Domain.User;

namespace TaskManager.Core.Domain.Events;

public class TaskCreated : DomainEventBase
{
    public TaskId UserId { get; }
    public string Name { get; }
    public string Description { get; }
    public DateTimeOffset CreationDate { get; }

    public TaskCreated(TaskId userId, string name, string description, DateTimeOffset creationDate)
    {
        UserId = userId;
        Name = name;
        Description = description;
        CreationDate = creationDate;
    }
}