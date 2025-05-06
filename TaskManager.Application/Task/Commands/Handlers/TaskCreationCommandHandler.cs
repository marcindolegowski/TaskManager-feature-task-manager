using Accessory.Builder.Core.Domain.Exceptions;
using Accessory.Builder.Core.Domain.Rules;
using Accessory.Builder.CQRS.Core.Commands;
using TaskManager.Core.Repositories;

namespace TaskManager.Application.Task.Commands.Handlers;

public class TaskCreationCommandHandler : ICommandHandler<TaskCreationCommand>
{
    private readonly ITaskRepository _taskRepository;

    public TaskCreationCommandHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async System.Threading.Tasks.Task Handle(TaskCreationCommand command)
    {
        if (string.IsNullOrEmpty(command.Name))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.Name)));
        if (string.IsNullOrEmpty(command.Description))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.Description)));

        if (await _taskRepository.FindByTaskName(command.Name) != null)
            throw new BrokenBusinessRuleException(new DuplicateValueException());

        _taskRepository.Add(new Core.Domain.Task.Task(
            command.Name, command.Description));
    }
}