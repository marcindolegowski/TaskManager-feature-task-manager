using Accessory.Builder.CQRS.Core.Queries;
using Accessory.Builder.CQRS.Core.Queries.DTO;
using Accessory.Builder.CQRS.Dapper.Sql;

namespace Accessory.Builder.CQRS.Dapper.Queries;

public class DapperGridQuery<T> : GridQuery<T>, IDapperQuery<PagedResult<T>>
{
    public DapperGridQuery(GridConfiguration gridConfiguration)
        : this(gridConfiguration, new SqlBuilderAdapter())
    {
    }

    public DapperGridQuery(GridConfiguration gridConfiguration, ISqlBuilder sqlBuilder)
        : base(gridConfiguration)
    {
        SqlBuilder = sqlBuilder;
    }

    public ISqlBuilder SqlBuilder { get; }
}