using Accessory.Builder.MessageBus.IntegrationEvent;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Task.Agents;

namespace TaskManager.Application.Task.Events;

public class TaskImplementationRequestedHandler : IIntegrationEventHandler<TaskImplementationRequested>
{
    private readonly IAgentSidecarClient _sidecar;
    private readonly ILogger<TaskImplementationRequestedHandler> _logger;

    public TaskImplementationRequestedHandler(
        IAgentSidecarClient sidecar,
        ILogger<TaskImplementationRequestedHandler> logger)
    {
        _sidecar = sidecar;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task HandleAsync(TaskImplementationRequested integrationEvent)
    {
        _logger.LogInformation(
            "Dispatching task {TaskId} ({Name}) to the agent sidecar",
            integrationEvent.TaskId, integrationEvent.Name);

        await _sidecar.RequestImplementationAsync(new AgentImplementationRequest(
            integrationEvent.TaskId.ToString(),
            integrationEvent.Name,
            integrationEvent.Description,
            integrationEvent.RepositoryUrl));
    }
}
