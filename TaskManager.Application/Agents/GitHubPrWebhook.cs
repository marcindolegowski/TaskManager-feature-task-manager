using System.Text.Json.Serialization;

namespace TaskManager.Application.Agents;

// Minimal slice of the GitHub `pull_request` webhook payload we act on.
public class GitHubPrWebhook
{
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("pull_request")]
    public GitHubPullRequest? PullRequest { get; set; }
}

public class GitHubPullRequest
{
    [JsonPropertyName("merged")]
    public bool Merged { get; set; }

    [JsonPropertyName("head")]
    public GitHubRef? Head { get; set; }
}

public class GitHubRef
{
    [JsonPropertyName("ref")]
    public string? Ref { get; set; }
}
