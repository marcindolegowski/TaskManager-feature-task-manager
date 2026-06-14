using Accessory.Builder.Core.Domain.Exceptions;
using Accessory.Builder.Core.Domain.Rules;
using Accessory.Builder.CQRS.Core.Commands;
using Accessory.Builder.MessageBus.Common;
using TaskManager.Application.Task.Events;
using TaskManager.Core.Repositories;

namespace TaskManager.Application.Task.Commands.Handlers;

public class RequestTaskImplementationCommandHandler : ICommandHandler<RequestTaskImplementationCommand>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IBusPublisher<TaskImplementationRequested> _busPublisher;

    public RequestTaskImplementationCommandHandler(
        ITaskRepository taskRepository,
        IBusPublisher<TaskImplementationRequested> busPublisher)
    {
        _taskRepository = taskRepository;
        _busPublisher = busPublisher;
    }

    public async System.Threading.Tasks.Task Handle(RequestTaskImplementationCommand command)
    {
        if (string.IsNullOrEmpty(command.Name))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.Name)));
        if (string.IsNullOrEmpty(command.RepositoryUrl))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.RepositoryUrl)));

        var task = await _taskRepository.FindByTaskName(command.Name);
        if (task == null)
            throw new BrokenBusinessRuleException(new DoesNotExistException());

        await _busPublisher.PublishEventAsync(new TaskImplementationRequested(
            task.Id.Value, task.Name, task.Description, command.RepositoryUrl));
    }
}
