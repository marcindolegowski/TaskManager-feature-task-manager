using System.Data;

namespace Accessory.Builder.CQRS.Dapper.Sql;

public interface ISqlConnectionFactory
{
    IDbConnection CreateDbConnection();
}