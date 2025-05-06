using Accessory.Builder.CQRS.Core.Commands;

namespace TaskManager.Application.Task.Commands;

public class RemovalTaskCommand : ICommand
{
    public string? Name { get; set; }
}