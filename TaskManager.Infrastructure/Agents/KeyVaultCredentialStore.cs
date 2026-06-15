using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using TaskManager.Application.Agents;

namespace TaskManager.Infrastructure.Agents;

/// <summary>
/// Stores agent credentials as Azure Key Vault secrets, one per user and one per team.
/// The token JSON never leaves Key Vault except when resolved for a run; never logged.
/// </summary>
public class KeyVaultCredentialStore : ICredentialStore
{
    private readonly SecretClient _client;

    public KeyVaultCredentialStore(SecretClient client)
    {
        _client = client;
    }

    public Task<AgentCredential?> GetUserAsync(string userId) => GetAsync(UserSecret(userId));
    public Task<AgentCredential?> GetTeamAsync(string teamId) => GetAsync(TeamSecret(teamId));
    public Task SetUserAsync(string userId, AgentCredential credential) => SetAsync(UserSecret(userId), credential);
    public Task SetTeamAsync(string teamId, AgentCredential credential) => SetAsync(TeamSecret(teamId), credential);

    private async Task<AgentCredential?> GetAsync(string name)
    {
        try
        {
            KeyVaultSecret secret = await _client.GetSecretAsync(name);
            return JsonSerializer.Deserialize<AgentCredential>(secret.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    private async Task SetAsync(string name, AgentCredential credential)
    {
        await _client.SetSecretAsync(name, JsonSerializer.Serialize(credential));
    }

    private static string UserSecret(string userId) => $"agent-cred-user-{Sanitize(userId)}";
    private static string TeamSecret(string teamId) => $"agent-cred-team-{Sanitize(teamId)}";

    // Key Vault secret names allow only alphanumerics and dashes.
    private static string Sanitize(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '-');
        return sb.ToString();
    }
}
