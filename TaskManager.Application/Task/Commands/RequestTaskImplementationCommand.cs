using Accessory.Builder.CQRS.Core.Commands;

namespace TaskManager.Application.Task.Commands;

public class RequestTaskImplementationCommand : ICommand
{
    public string? Name { get; set; }

    public string? RepositoryUrl { get; set; }

    // Requesting developer; used to resolve their connected Claude account for the run.
    public string? UserId { get; set; }
}
