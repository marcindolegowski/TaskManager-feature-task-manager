using Accessory.Builder.CQRS.Dapper.Queries.Handlers;
using Accessory.Builder.CQRS.Dapper.Sql;
using TaskManager.Application.Task.DTO;

namespace TaskManager.Application.Task.Queries.Handlers;

public class TasksQueryHandler : DapperQueryHandler<TasksQuery, TaskDTO>
{
    public TasksQueryHandler(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
    { }

    protected override string TableOrViewName => "Tasks";
}