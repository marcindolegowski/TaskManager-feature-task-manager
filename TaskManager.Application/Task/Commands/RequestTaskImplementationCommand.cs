using Accessory.Builder.CQRS.Core.Commands;

namespace TaskManager.Application.Task.Commands;

public class RequestTaskImplementationCommand : ICommand
{
    public string? Name { get; set; }

    public string? RepositoryUrl { get; set; }
}
