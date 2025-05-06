using Accessory.Builder.CQRS.Core.Commands;

namespace TaskManager.Application.Task.Commands;

public class TaskStatusUpdateCommand : ICommand
{
    public string? Name { get; set; }
}