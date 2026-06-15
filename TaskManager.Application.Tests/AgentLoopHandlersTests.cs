using Accessory.Builder.Core.Domain.Exceptions;
using TaskManager.Application.Task.Commands;
using TaskManager.Application.Task.Commands.Handlers;
using Xunit;

namespace TaskManager.Application.Tests;

// Tests for the loop-closure handlers: status rides the existing Task state machine.
public class AgentLoopHandlersTests
{
    [Fact]
    public async System.Threading.Tasks.Task RecordResult_records_pr_and_moves_NotStarted_to_InProgress()
    {
        var task = new Core.Domain.Task.Task("feat", "do it"); // NotStarted
        var store = new FakeAgentRunStore();
        var handler = new RecordAgentResultCommandHandler(store, new FakeTaskRepository(task));

        await handler.Handle(new RecordAgentResultCommand
        {
            TaskId = task.Id.Value.ToString(),
            PrUrl = "https://github.com/o/r/pull/1",
            CostUsd = 0.42m,
        });

        Assert.Single(store.PrOpened);
        Assert.Equal("https://github.com/o/r/pull/1", store.PrOpened[0].PrUrl);
        Assert.Equal(Core.Domain.Task.Status.InProgress, task.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task RecordResult_throws_when_taskId_invalid()
    {
        var handler = new RecordAgentResultCommandHandler(new FakeAgentRunStore(), new FakeTaskRepository(null));
        await Assert.ThrowsAsync<BrokenBusinessRuleException>(() =>
            handler.Handle(new RecordAgentResultCommand { TaskId = "not-a-guid", PrUrl = "https://x" }));
    }

    [Fact]
    public async System.Threading.Tasks.Task CompleteMerged_moves_InProgress_to_Completed()
    {
        var task = new Core.Domain.Task.Task("feat", "do it");
        task.MoveToNextStatus(); // -> InProgress
        var store = new FakeAgentRunStore();
        var handler = new CompleteMergedTaskCommandHandler(store, new FakeTaskRepository(task));

        await handler.Handle(new CompleteMergedTaskCommand { TaskId = task.Id.Value.ToString() });

        Assert.Single(store.Merged);
        Assert.Equal(Core.Domain.Task.Status.Completed, task.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task CompleteMerged_does_not_advance_a_NotStarted_task()
    {
        var task = new Core.Domain.Task.Task("feat", "do it"); // NotStarted (no PR opened)
        var handler = new CompleteMergedTaskCommandHandler(new FakeAgentRunStore(), new FakeTaskRepository(task));

        await handler.Handle(new CompleteMergedTaskCommand { TaskId = task.Id.Value.ToString() });

        Assert.Equal(Core.Domain.Task.Status.NotStarted, task.Status);
    }
}
