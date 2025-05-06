using Accessory.Builder.Core.Domain.Exceptions;
using Accessory.Builder.Core.Domain.Rules;
using Accessory.Builder.CQRS.Core.Commands;
using Accessory.Builder.MessageBus.Common;
using TaskManager.Application.Task.Events;
using TaskManager.Core.Repositories;

namespace TaskManager.Application.Task.Commands.Handlers;

public class TaskStatusUpdateCommandHandler : ICommandHandler<TaskStatusUpdateCommand>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IBusPublisher<TaskCompletedEvent> _busPublisher;

    public TaskStatusUpdateCommandHandler(ITaskRepository taskRepository, IBusPublisher<TaskCompletedEvent> busPublisher)
    {
        _taskRepository = taskRepository;
        _busPublisher = busPublisher;
    }

    public async System.Threading.Tasks.Task Handle(TaskStatusUpdateCommand command)
    {
        if (string.IsNullOrEmpty(command.Name))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.Name)));
        var task = await _taskRepository.FindByTaskName(command.Name);
        if (task == null)
            throw new BrokenBusinessRuleException(new DoesNotExistException());

        task.MoveToNextStatus();
        if (task.Status == Core.Domain.Task.Status.Completed)
        {
            await _busPublisher.PublishEventAsync(
                new TaskCompletedEvent(task.Name));
        }
    }
}