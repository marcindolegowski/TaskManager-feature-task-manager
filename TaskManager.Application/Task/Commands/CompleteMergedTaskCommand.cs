using Accessory.Builder.CQRS.Core.Commands;

namespace TaskManager.Application.Task.Commands;

public class CompleteMergedTaskCommand : ICommand
{
    public string? TaskId { get; set; }
}
