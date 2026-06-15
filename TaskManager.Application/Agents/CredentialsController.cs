using System.Threading.Tasks;
using Accessory.Builder.CQRS.Core.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Application.Agents;

// Connect-account endpoints. A developer pastes the token from `claude setup-token`
// (their login is your SSO for Team/Enterprise); the team admin sets the fallback.
public class CredentialsController : BaseController
{
    private readonly ICommandDispatcher _commandDispatcher;

    public CredentialsController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [HttpPost("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConnectMe([FromBody] ConnectUserCredentialCommand command)
    {
        await _commandDispatcher.Send(command);
        return Ok();
    }

    [HttpPost("team")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetTeam([FromBody] SetTeamCredentialCommand command)
    {
        await _commandDispatcher.Send(command);
        return Ok();
    }
}
