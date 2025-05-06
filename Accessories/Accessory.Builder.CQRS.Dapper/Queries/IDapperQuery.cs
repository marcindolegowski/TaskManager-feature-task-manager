using Accessory.Builder.CQRS.Core.Queries;
using Accessory.Builder.CQRS.Dapper.Sql;

namespace Accessory.Builder.CQRS.Dapper.Queries;

public interface IDapperQuery<T> : IQuery<T>, IDapperQuery
{
}

public interface IDapperQuery : IQuery
{
    public ISqlBuilder SqlBuilder { get; }
}