using Accessory.Builder.Core.Domain.Exceptions;
using Accessory.Builder.Core.Domain.Rules;
using Accessory.Builder.CQRS.Core.Commands;

namespace TaskManager.Application.Agents;

public class ConnectUserCredentialCommandHandler : ICommandHandler<ConnectUserCredentialCommand>
{
    private readonly ICredentialStore _store;

    public ConnectUserCredentialCommandHandler(ICredentialStore store)
    {
        _store = store;
    }

    public async System.Threading.Tasks.Task Handle(ConnectUserCredentialCommand command)
    {
        // TODO(security): take the user identity from the authenticated principal, not the body.
        if (string.IsNullOrEmpty(command.UserId))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.UserId)));
        if (string.IsNullOrEmpty(command.OauthToken) && string.IsNullOrEmpty(command.ApiKey))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.OauthToken)));

        await _store.SetUserAsync(command.UserId, new AgentCredential(command.OauthToken, command.ApiKey));
    }
}

public class SetTeamCredentialCommandHandler : ICommandHandler<SetTeamCredentialCommand>
{
    private readonly ICredentialStore _store;

    public SetTeamCredentialCommandHandler(ICredentialStore store)
    {
        _store = store;
    }

    public async System.Threading.Tasks.Task Handle(SetTeamCredentialCommand command)
    {
        if (string.IsNullOrEmpty(command.OauthToken) && string.IsNullOrEmpty(command.ApiKey))
            throw new BrokenBusinessRuleException(new RequiredValueException(nameof(command.OauthToken)));

        var teamId = string.IsNullOrEmpty(command.TeamId) ? "default" : command.TeamId;
        await _store.SetTeamAsync(teamId, new AgentCredential(command.OauthToken, command.ApiKey));
    }
}
