using Accessory.Builder.MessageBus.IntegrationEvent;
using Microsoft.Extensions.Logging;

namespace TaskManager.Application.Task.Events;

public class TaskCompletedEventHandler : IIntegrationEventHandler<TaskCompletedEvent>
{
    private readonly ILogger<TaskCompletedEventHandler> _logger;

    public TaskCompletedEventHandler(ILogger<TaskCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async System.Threading.Tasks.Task HandleAsync(TaskCompletedEvent integrationEvent)
    {
        _logger.Log(LogLevel.Information, "Reaction for this event");
    }
}