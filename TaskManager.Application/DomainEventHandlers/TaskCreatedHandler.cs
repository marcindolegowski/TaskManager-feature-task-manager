using Accessory.Builder.CQRS.Core.Events;
using Microsoft.Extensions.Logging;
using TaskManager.Core.Domain.Events;

namespace TaskManager.Application.DomainEventHandlers;

public class TaskCreatedHandler : IDomainEventHandler<TaskCreated>
{
    private readonly ILogger<TaskCreatedHandler> _logger;

    public TaskCreatedHandler(ILogger<TaskCreatedHandler> logger)
    {
        _logger = logger;
    }

    public async System.Threading.Tasks.Task Handle(TaskCreated integrationEvent)
    {
        _logger.Log(LogLevel.Information,"Reaction for this event");
    }
}