using Accessory.Builder.Core.Domain.Exceptions;
using Accessory.Builder.Core.Domain.Rules;
using Accessory.Builder.CQRS.Core.Commands;
using Accessory.Builder.MessageBus.Common;
using TaskManager.Application.Task.Commands;
using TaskManager.Application.Task.Events;
using TaskManager.Core.Repositories;

namespace TaskManager.Application.User.Commands.Handlers;

public class RemovalCommandHandler : ICommandHandler<RemovalTaskCommand>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IBusPublisher<RemovalTaskEvent> _busPublisher;

    public RemovalCommandHandler(
        ITaskRepository userRepository,
        IBusPublisher<RemovalTaskEvent> busPublisher)
    {
        _taskRepository = userRepository;
        _busPublisher = busPublisher;
    }

    public async System.Threading.Tasks.Task Handle(RemovalTaskCommand command)
    {
        if (string.IsNullOrEmpty(command.Name))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.Name)));
        if (await _taskRepository.FindByTaskName(command.Name) == null)
            throw new BrokenBusinessRuleException(new DoesNotExistException());

        await _busPublisher.PublishEventAsync(
            new RemovalTaskEvent(
                command.Name));
    }
}