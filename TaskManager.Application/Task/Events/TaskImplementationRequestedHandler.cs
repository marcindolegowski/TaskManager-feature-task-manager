using Accessory.Builder.MessageBus.IntegrationEvent;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Agents;
using TaskManager.Application.Task.Agents;

namespace TaskManager.Application.Task.Events;

public class TaskImplementationRequestedHandler : IIntegrationEventHandler<TaskImplementationRequested>
{
    private readonly IAgentSidecarClient _sidecar;
    private readonly ICredentialResolver _credentialResolver;
    private readonly ILogger<TaskImplementationRequestedHandler> _logger;

    public TaskImplementationRequestedHandler(
        IAgentSidecarClient sidecar,
        ICredentialResolver credentialResolver,
        ILogger<TaskImplementationRequestedHandler> logger)
    {
        _sidecar = sidecar;
        _credentialResolver = credentialResolver;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task HandleAsync(TaskImplementationRequested integrationEvent)
    {
        // Resolve the developer's (or team's) credential at the edge — not on the bus.
        var resolved = await _credentialResolver.ResolveAsync(integrationEvent.UserId);
        var credential = resolved is null
            ? null
            : new AgentRunCredential(resolved.OauthToken, resolved.ApiKey, resolved.Scope);

        _logger.LogInformation(
            "Dispatching task {TaskId} ({Name}) to the agent sidecar under {Scope} credential",
            integrationEvent.TaskId, integrationEvent.Name, resolved?.Scope ?? "ambient");

        await _sidecar.RequestImplementationAsync(new AgentImplementationRequest(
            integrationEvent.TaskId.ToString(),
            integrationEvent.Name,
            integrationEvent.Description,
            integrationEvent.RepositoryUrl,
            credential));
    }
}
