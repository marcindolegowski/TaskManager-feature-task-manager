using Accessory.Builder.CQRS.Core.Commands;

namespace TaskManager.Application.Task.Commands;

public class RecordAgentResultCommand : ICommand
{
    public string? TaskId { get; set; }

    public string? PrUrl { get; set; }

    public decimal CostUsd { get; set; }
}
