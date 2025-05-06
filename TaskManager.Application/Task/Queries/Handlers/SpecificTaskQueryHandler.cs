using System.Threading.Tasks;
using Accessory.Builder.CQRS.Dapper.Queries.Handlers;
using Accessory.Builder.CQRS.Dapper.Sql;
using TaskManager.Application.Task.DTO;

namespace TaskManager.Application.Task.Queries.Handlers;

public class SpecificTaskQueryHandler : DapperSingleQueryHandler<SpecificTaskQuery, TaskDTO>
{
    public SpecificTaskQueryHandler(ISqlConnectionFactory connectionFactory) : base(connectionFactory)
    { }

    protected override string TableOrViewName => "Tasks";
    public override async Task<TaskDTO> HandleAsync(SpecificTaskQuery query)
    {
        query.SqlBuilder.Where("Name = @Name", new { query.Name });
        var result = await base.HandleAsync(query);
        return result;
    }
}