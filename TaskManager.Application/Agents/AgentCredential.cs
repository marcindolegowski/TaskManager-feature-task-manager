namespace TaskManager.Application.Agents;

/// <summary>A stored agent credential (a developer's or a team's Claude account).</summary>
public record AgentCredential(string? OauthToken, string? ApiKey)
{
    public bool IsEmpty => string.IsNullOrEmpty(OauthToken) && string.IsNullOrEmpty(ApiKey);
}

/// <summary>A credential resolved for a specific run, tagged with the scope it came from.</summary>
public record ResolvedCredential(string? OauthToken, string? ApiKey, string Scope);
