using System;
using System.Threading.Tasks;
using Accessory.Builder.CQRS.Core.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Task.Commands;

namespace TaskManager.Application.Agents;

public class AgentController : BaseController
{
    private readonly ICommandDispatcher _commandDispatcher;

    public AgentController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    /// <summary>
    /// Sidecar callback: report the draft PR it opened for a task.
    /// </summary>
    [HttpPost("result")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Result([FromBody] RecordAgentResultCommand command)
    {
        await _commandDispatcher.Send(command);
        return Ok();
    }

    /// <summary>
    /// GitHub `pull_request` webhook. On a merged PR from a task/{id} branch, completes the task.
    /// TODO(security): verify the X-Hub-Signature-256 HMAC before acting (see strategy doc guardrails).
    /// </summary>
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Webhook([FromBody] GitHubPrWebhook payload)
    {
        var head = payload?.PullRequest?.Head?.Ref ?? string.Empty;
        if (payload?.Action == "closed" && payload.PullRequest?.Merged == true && head.StartsWith("task/", StringComparison.Ordinal))
        {
            await _commandDispatcher.Send(new CompleteMergedTaskCommand
            {
                TaskId = head.Substring("task/".Length),
            });
        }
        return Ok();
    }
}
