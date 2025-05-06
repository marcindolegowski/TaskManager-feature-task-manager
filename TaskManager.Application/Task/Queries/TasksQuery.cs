using Accessory.Builder.CQRS.Dapper.Queries;
using System.Collections.Generic;
using TaskManager.Application.Task.DTO;

namespace TaskManager.Application.Task.Queries;

public class TasksQuery : DapperQuery<IEnumerable<TaskDTO>>
{
}