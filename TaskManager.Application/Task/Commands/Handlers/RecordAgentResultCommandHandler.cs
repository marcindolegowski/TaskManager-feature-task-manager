using System;
using Accessory.Builder.Core.Domain.Exceptions;
using Accessory.Builder.Core.Domain.Rules;
using Accessory.Builder.CQRS.Core.Commands;
using TaskManager.Application.Agents;
using TaskManager.Core.Domain.User;
using TaskManager.Core.Repositories;

namespace TaskManager.Application.Task.Commands.Handlers;

// Sidecar reports the opened PR: record it and advance the task NotStarted -> InProgress.
public class RecordAgentResultCommandHandler : ICommandHandler<RecordAgentResultCommand>
{
    private readonly IAgentRunStore _store;
    private readonly ITaskRepository _taskRepository;

    public RecordAgentResultCommandHandler(IAgentRunStore store, ITaskRepository taskRepository)
    {
        _store = store;
        _taskRepository = taskRepository;
    }

    public async System.Threading.Tasks.Task Handle(RecordAgentResultCommand command)
    {
        if (string.IsNullOrEmpty(command.TaskId) || !Guid.TryParse(command.TaskId, out var guid))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.TaskId)));
        if (string.IsNullOrEmpty(command.PrUrl))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.PrUrl)));

        await _store.RecordPrOpenedAsync(guid, command.PrUrl, command.CostUsd);

        var task = await _taskRepository.FindByIdAsync(new TaskId(guid));
        if (task != null && task.Status == Core.Domain.Task.Status.NotStarted)
            task.MoveToNextStatus(); // -> InProgress
    }
}
