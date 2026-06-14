using System.Threading.Tasks;

namespace TaskManager.Application.Task.Agents;

/// <summary>
/// Application port to the out-of-process Claude Agent SDK sidecar.
/// The API process never runs the agent itself (see the constitution).
/// </summary>
public interface IAgentSidecarClient
{
    Task RequestImplementationAsync(AgentImplementationRequest request);
}

public record AgentImplementationRequest(
    string TaskId,
    string Name,
    string Description,
    string RepositoryUrl);
