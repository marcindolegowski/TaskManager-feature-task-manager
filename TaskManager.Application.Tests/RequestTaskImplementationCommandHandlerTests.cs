using System;
using System.Collections.Generic;
using Accessory.Builder.Core.Domain.Exceptions;
using Accessory.Builder.MessageBus.Common;
using TaskManager.Application.Agents;
using TaskManager.Application.Task.Commands;
using TaskManager.Application.Task.Commands.Handlers;
using TaskManager.Application.Task.Events;
using TaskManager.Core.Domain.User;
using TaskManager.Core.Repositories;
using Xunit;

namespace TaskManager.Application.Tests;

public class RequestTaskImplementationCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Throws_when_name_missing()
    {
        var handler = new RequestTaskImplementationCommandHandler(
            new FakeTaskRepository(null), new FakeBusPublisher(), new FakeAgentRunStore());

        await Assert.ThrowsAsync<BrokenBusinessRuleException>(() =>
            handler.Handle(new RequestTaskImplementationCommand { Name = "", RepositoryUrl = "https://repo.git" }));
    }

    [Fact]
    public async System.Threading.Tasks.Task Throws_when_task_not_found()
    {
        var handler = new RequestTaskImplementationCommandHandler(
            new FakeTaskRepository(null), new FakeBusPublisher(), new FakeAgentRunStore());

        await Assert.ThrowsAsync<BrokenBusinessRuleException>(() =>
            handler.Handle(new RequestTaskImplementationCommand { Name = "missing", RepositoryUrl = "https://repo.git" }));
    }

    [Fact]
    public async System.Threading.Tasks.Task Publishes_event_and_records_run_on_happy_path()
    {
        var task = new Core.Domain.Task.Task("build-feature", "do the thing");
        var bus = new FakeBusPublisher();
        var store = new FakeAgentRunStore();
        var handler = new RequestTaskImplementationCommandHandler(new FakeTaskRepository(task), bus, store);

        await handler.Handle(new RequestTaskImplementationCommand { Name = "build-feature", RepositoryUrl = "https://repo.git" });

        Assert.Single(bus.Published);
        Assert.Equal("build-feature", bus.Published[0].Name);
        Assert.Single(store.Requested);
        Assert.Equal(task.Id.Value, store.Requested[0]);
    }
}

internal class FakeTaskRepository : ITaskRepository
{
    private readonly Core.Domain.Task.Task? _task;
    public FakeTaskRepository(Core.Domain.Task.Task? task) => _task = task;

    public System.Threading.Tasks.Task<Core.Domain.Task.Task?> FindByTaskName(string name) =>
        System.Threading.Tasks.Task.FromResult(_task);

    public System.Threading.Tasks.Task<bool> ExistsAsync(TaskId id) =>
        System.Threading.Tasks.Task.FromResult(false);

    public void Add(Core.Domain.Task.Task entity) { }

    public System.Threading.Tasks.Task AddRangeAsync(IEnumerable<Core.Domain.Task.Task> entities) =>
        System.Threading.Tasks.Task.CompletedTask;

    public System.Threading.Tasks.Task<Core.Domain.Task.Task?> FindByIdAsync(TaskId id) =>
        System.Threading.Tasks.Task.FromResult(_task);
}

internal class FakeBusPublisher : IBusPublisher<TaskImplementationRequested>
{
    public List<TaskImplementationRequested> Published { get; } = new();

    public System.Threading.Tasks.Task PublishEventAsync(TaskImplementationRequested payload, string? sessionId = null)
    {
        Published.Add(payload);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}

internal class FakeAgentRunStore : IAgentRunStore
{
    public List<Guid> Requested { get; } = new();

    public System.Threading.Tasks.Task RecordRequestedAsync(Guid taskId, string branch)
    {
        Requested.Add(taskId);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task RecordPrOpenedAsync(Guid taskId, string prUrl, decimal costUsd) =>
        System.Threading.Tasks.Task.CompletedTask;

    public System.Threading.Tasks.Task RecordMergedAsync(Guid taskId) =>
        System.Threading.Tasks.Task.CompletedTask;
}
