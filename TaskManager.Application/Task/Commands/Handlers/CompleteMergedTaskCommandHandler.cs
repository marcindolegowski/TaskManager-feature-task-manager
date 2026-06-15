using System;
using Accessory.Builder.Core.Domain.Exceptions;
using Accessory.Builder.Core.Domain.Rules;
using Accessory.Builder.CQRS.Core.Commands;
using TaskManager.Application.Agents;
using TaskManager.Core.Domain.User;
using TaskManager.Core.Repositories;

namespace TaskManager.Application.Task.Commands.Handlers;

// PR merged (via webhook): mark the run merged and advance the task InProgress -> Completed.
public class CompleteMergedTaskCommandHandler : ICommandHandler<CompleteMergedTaskCommand>
{
    private readonly IAgentRunStore _store;
    private readonly ITaskRepository _taskRepository;

    public CompleteMergedTaskCommandHandler(IAgentRunStore store, ITaskRepository taskRepository)
    {
        _store = store;
        _taskRepository = taskRepository;
    }

    public async System.Threading.Tasks.Task Handle(CompleteMergedTaskCommand command)
    {
        if (string.IsNullOrEmpty(command.TaskId) || !Guid.TryParse(command.TaskId, out var guid))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.TaskId)));

        await _store.RecordMergedAsync(guid);

        var task = await _taskRepository.FindByIdAsync(new TaskId(guid));
        if (task != null && task.Status == Core.Domain.Task.Status.InProgress)
            task.MoveToNextStatus(); // -> Completed
    }
}
