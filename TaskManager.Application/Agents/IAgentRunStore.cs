using System;
using System.Threading.Tasks;

namespace TaskManager.Application.Agents;

/// <summary>
/// Port for recording agent-run lifecycle metadata (PR URL, cost, status) for a task,
/// kept out of the Task aggregate. Writes are committed by the command UnitOfWork.
/// </summary>
public interface IAgentRunStore
{
    System.Threading.Tasks.Task RecordRequestedAsync(Guid taskId, string branch);
    System.Threading.Tasks.Task RecordPrOpenedAsync(Guid taskId, string prUrl, decimal costUsd);
    System.Threading.Tasks.Task RecordMergedAsync(Guid taskId);
}
