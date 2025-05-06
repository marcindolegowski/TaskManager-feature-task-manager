using System;
using Accessory.Builder.Core.Domain;

namespace TaskManager.Core.Domain.User;

public class TaskId : TypedIdValueBase
{
    public TaskId(Guid value) : base(value) { }

    public static TaskId Default => new TaskId(default);
    public static TaskId Generate() => new TaskId(Guid.NewGuid());
}