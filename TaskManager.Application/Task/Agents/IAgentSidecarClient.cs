using System.Threading.Tasks;

namespace TaskManager.Application.Task.Agents;

/// <summary>
/// Application port to the out-of-process Claude Agent SDK sidecar.
/// The API process never runs the agent itself (see the constitution).
/// </summary>
public interface IAgentSidecarClient
{
    System.Threading.Tasks.Task RequestImplementationAsync(AgentImplementationRequest request);
}

public record AgentImplementationRequest(
    string TaskId,
    string Name,
    string Description,
    string RepositoryUrl,
    AgentRunCredential? Credential);

// Credential resolved for this run (user or team scope). Serialized to the sidecar
// as { oauthToken, apiKey, scope } — never logged, never put on the message bus.
public record AgentRunCredential(string? OauthToken, string? ApiKey, string Scope);
