using System;
using System.Linq;
using Accessory.Builder.Core.Common;
using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Agents;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Agents;

public class AgentRunStore : IAgentRunStore
{
    private readonly DatabaseContext _context;

    public AgentRunStore(DatabaseContext context)
    {
        _context = context;
    }

    public System.Threading.Tasks.Task RecordRequestedAsync(Guid taskId, string branch)
    {
        _context.Set<AgentRun>().Add(new AgentRun
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Status = "Requested",
            Branch = branch,
            CreatedAt = SystemTime.OffsetNow(),
            UpdatedAt = SystemTime.OffsetNow(),
        });
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async System.Threading.Tasks.Task RecordPrOpenedAsync(Guid taskId, string prUrl, decimal costUsd)
    {
        var run = await LatestAsync(taskId);
        if (run == null) return;
        run.Status = "PrOpened";
        run.PrUrl = prUrl;
        run.CostUsd = costUsd;
        run.UpdatedAt = SystemTime.OffsetNow();
    }

    public async System.Threading.Tasks.Task RecordMergedAsync(Guid taskId)
    {
        var run = await LatestAsync(taskId);
        if (run == null) return;
        run.Status = "Merged";
        run.UpdatedAt = SystemTime.OffsetNow();
    }

    private System.Threading.Tasks.Task<AgentRun?> LatestAsync(Guid taskId) =>
        _context.Set<AgentRun>()
            .Where(r => r.TaskId == taskId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
}
