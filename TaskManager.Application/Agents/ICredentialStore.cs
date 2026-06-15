namespace TaskManager.Application.Agents;

/// <summary>
/// Secure store for agent credentials at two scopes: per developer and per team.
/// Backed by Azure Key Vault in production. Tokens are never logged or put on the bus.
/// </summary>
public interface ICredentialStore
{
    System.Threading.Tasks.Task<AgentCredential?> GetUserAsync(string userId);
    System.Threading.Tasks.Task<AgentCredential?> GetTeamAsync(string teamId);
    System.Threading.Tasks.Task SetUserAsync(string userId, AgentCredential credential);
    System.Threading.Tasks.Task SetTeamAsync(string teamId, AgentCredential credential);
}
