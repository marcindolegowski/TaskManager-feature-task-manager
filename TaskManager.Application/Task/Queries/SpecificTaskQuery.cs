using Accessory.Builder.CQRS.Dapper.Queries;
using TaskManager.Application.Task.DTO;

namespace TaskManager.Application.Task.Queries;

public class SpecificTaskQuery : DapperQuery<TaskDTO>
{
    public string? Name { get; set; }
}