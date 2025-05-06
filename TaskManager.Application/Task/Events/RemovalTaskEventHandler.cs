using Accessory.Builder.MessageBus.IntegrationEvent;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Task.Events;

namespace TaskManager.Application.User.Events;

public class RemovalTaskEventHandler : IIntegrationEventHandler<RemovalTaskEvent>
{
    private readonly ILogger<RemovalTaskEventHandler> _logger;

    public RemovalTaskEventHandler(ILogger<RemovalTaskEventHandler> logger)
    {
        _logger = logger;
    }

    public async System.Threading.Tasks.Task HandleAsync(RemovalTaskEvent integrationEvent)
    {
        _logger.Log(LogLevel.Information, "Reaction for this event");
    }
}