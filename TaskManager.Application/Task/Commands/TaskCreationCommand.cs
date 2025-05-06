using Accessory.Builder.CQRS.Core.Commands;

namespace TaskManager.Application.Task.Commands;

public class TaskCreationCommand : ICommand
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}