using System;

namespace TaskManager.Infrastructure.Persistence;

/// <summary>
/// Operational record of an agent run for a task. Deliberately NOT part of the
/// Task aggregate: PR URL, run id and cost are agent-infrastructure metadata, not
/// domain invariants. Keyed by TaskId; the Task aggregate stays clean.
/// </summary>
public class AgentRun
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string Status { get; set; } = "Requested"; // Requested | PrOpened | Merged
    public string? Branch { get; set; }
    public string? PrUrl { get; set; }
    public decimal CostUsd { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
