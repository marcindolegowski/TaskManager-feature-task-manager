using Accessory.Builder.CQRS.Core.Queries;
using TaskManager.Application.Task.DTO;

namespace TaskManager.Application.Task.Queries;

public class SpecificCachedTaskQuery : IQuery<TaskDTO>
{
    public string? Name { get; set; }
}