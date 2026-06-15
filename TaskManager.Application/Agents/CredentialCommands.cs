using Accessory.Builder.CQRS.Core.Commands;

namespace TaskManager.Application.Agents;

// A developer connects their own Claude account (token from `claude setup-token`).
public class ConnectUserCredentialCommand : ICommand
{
    public string? UserId { get; set; }
    public string? OauthToken { get; set; }
    public string? ApiKey { get; set; }
}

// A team admin sets the shared team account used as fallback.
public class SetTeamCredentialCommand : ICommand
{
    public string? TeamId { get; set; }
    public string? OauthToken { get; set; }
    public string? ApiKey { get; set; }
}
