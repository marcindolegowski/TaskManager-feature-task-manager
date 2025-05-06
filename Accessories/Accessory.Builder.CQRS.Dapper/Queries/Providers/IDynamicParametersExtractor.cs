using Dapper;
using Accessory.Builder.CQRS.Core.Queries;

namespace Accessory.Builder.CQRS.Dapper.Queries.Providers;

public interface IDynamicParametersExtractor
{
    public DynamicParameters ConfigureParameters(IGridQuery query);
}