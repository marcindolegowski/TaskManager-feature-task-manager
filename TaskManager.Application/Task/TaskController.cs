using Accessory.Builder.CQRS.Core.Commands;
using Accessory.Builder.CQRS.Core.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManager.Application.Task.Commands;
using TaskManager.Application.Task.Queries;

namespace TaskManager.Application.Task;

public class TaskController : BaseController
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public TaskController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    /// <summary>
    /// Get task's betting
    /// </summary>
    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public async Task<IActionResult> Get(string name)
    {
        var dto = await _queryDispatcher.QueryAsync(new SpecificTaskQuery { Name = name });
        return Json(dto);
    }

    /// <summary>
    /// Get task's betting
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public async Task<IActionResult> Get()
    {
        var dto = await _queryDispatcher.QueryAsync(new TasksQuery());
        return Json(dto);
    }

    /// <summary>
    /// Create task's betting
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public async Task<IActionResult> Post([FromBody] TaskCreationCommand command)
    {
        await _commandDispatcher.Send(command);
        return Ok();
    }

    /// <summary>
    /// Create task's betting
    /// </summary>
    [HttpPatch("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public async Task<IActionResult> Patch([FromBody] TaskStatusUpdateCommand command)
    {
        await _commandDispatcher.Send(command);
        return Ok();
    }

    /// <summary>
    /// Delete task's betting
    /// </summary>
    [HttpDelete("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public async Task<IActionResult> Delete([FromBody] RemovalTaskCommand command)
    {
        await _commandDispatcher.Send(command);
        return Ok();
    }
}