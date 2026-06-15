using Accessory.Builder.Core.Domain.Exceptions;
using Accessory.Builder.Core.Domain.Rules;
using Accessory.Builder.CQRS.Core.Commands;
using Accessory.Builder.MessageBus.Common;
using TaskManager.Application.Agents;
using TaskManager.Application.Task.Events;
using TaskManager.Core.Repositories;

namespace TaskManager.Application.Task.Commands.Handlers;

public class RequestTaskImplementationCommandHandler : ICommandHandler<RequestTaskImplementationCommand>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IBusPublisher<TaskImplementationRequested> _busPublisher;
    private readonly IAgentRunStore _agentRunStore;

    public RequestTaskImplementationCommandHandler(
        ITaskRepository taskRepository,
        IBusPublisher<TaskImplementationRequested> busPublisher,
        IAgentRunStore agentRunStore)
    {
        _taskRepository = taskRepository;
        _busPublisher = busPublisher;
        _agentRunStore = agentRunStore;
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

        await _agentRunStore.RecordRequestedAsync(task.Id.Value, $"task/{task.Id.Value}");

        await _busPublisher.PublishEventAsync(new TaskImplementationRequested(
            task.Id.Value, task.Name, task.Description, command.RepositoryUrl));
    }
}
