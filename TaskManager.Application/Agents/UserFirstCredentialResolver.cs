using System.Threading.Tasks;

namespace TaskManager.Application.Agents;

/// <summary>
/// User-first, team-fallback policy: run under the developer's own Claude account when
/// they have connected one, otherwise the shared team account. Returns null when neither
/// is configured, in which case the sidecar falls back to its own ambient credentials.
/// </summary>
public class UserFirstCredentialResolver : ICredentialResolver
{
    private readonly ICredentialStore _store;
    private readonly string _teamId;

    public UserFirstCredentialResolver(ICredentialStore store, string teamId)
    {
        _store = store;
        _teamId = string.IsNullOrEmpty(teamId) ? "default" : teamId;
    }

    public async Task<ResolvedCredential?> ResolveAsync(string? userId)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await _store.GetUserAsync(userId);
            if (user is { IsEmpty: false })
                return new ResolvedCredential(user.OauthToken, user.ApiKey, "user");
        }

        var team = await _store.GetTeamAsync(_teamId);
        if (team is { IsEmpty: false })
            return new ResolvedCredential(team.OauthToken, team.ApiKey, "team");

        return null;
    }
}
