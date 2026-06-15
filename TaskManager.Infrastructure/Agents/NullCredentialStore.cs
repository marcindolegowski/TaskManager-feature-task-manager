using System;
using System.Threading.Tasks;
using TaskManager.Application.Agents;

namespace TaskManager.Infrastructure.Agents;

/// <summary>
/// Fallback used when no Key Vault is configured (e.g. local dev). Resolution returns
/// nothing, so runs use the sidecar's own ambient credentials; setting fails loudly.
/// </summary>
public class NullCredentialStore : ICredentialStore
{
    public Task<AgentCredential?> GetUserAsync(string userId) => Task.FromResult<AgentCredential?>(null);
    public Task<AgentCredential?> GetTeamAsync(string teamId) => Task.FromResult<AgentCredential?>(null);

    public Task SetUserAsync(string userId, AgentCredential credential) => throw NotConfigured();
    public Task SetTeamAsync(string teamId, AgentCredential credential) => throw NotConfigured();

    private static InvalidOperationException NotConfigured() =>
        new("Credential storage is not configured. Set AZURE_KEYVAULT_URI to enable Key Vault.");
}
