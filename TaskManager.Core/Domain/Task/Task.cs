using System;
using Accessory.Builder.Core.Common;
using Accessory.Builder.Core.Domain;
using Accessory.Builder.Core.Domain.Exceptions;
using TaskManager.Core.Domain.Events;
using TaskManager.Core.Domain.Rules;
using TaskManager.Core.Domain.User;

namespace TaskManager.Core.Domain.Task;

public class Task : Entity, IAggregateRoot<TaskId>
{
    public TaskId Id { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public DateTimeOffset CreationDate { get; private set; }

    public DateTimeOffset LastUpdateDate { get; private set; }

    public Status Status { get; private set; }

    public Task(string name, string description)
    {
        Id = TaskId.Generate();
        Name = name;
        Description = description;
        CreationDate = SystemTime.OffsetNow();
        LastUpdateDate = SystemTime.OffsetNow();
        Status = Status.NotStarted;

        AddDomainEvent(new TaskCreated(Id, Name, Description, CreationDate));
    }

    public void MoveToNextStatus()
    {
        Status = Status switch
        {
            Status.NotStarted => SetStatusAndReturn(Status.InProgress),
            Status.InProgress => SetStatusAndReturn(Status.Completed),
            Status.Completed => throw new BrokenBusinessRuleException(new TaskAlreadyCompletedException()),
            _ => throw new BrokenBusinessRuleException(new UnsupportedTaskStatusException())
        };
    }

    private Status SetStatusAndReturn(Status newStatus)
    {
        LastUpdateDate = SystemTime.OffsetNow();
        return newStatus;
    }
}

public enum Status
{
    NotStarted, InProgress, Completed
}