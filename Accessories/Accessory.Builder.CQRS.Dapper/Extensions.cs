using Accessory.Builder.Core.Builders;
using Accessory.Builder.CQRS.Core.Queries;
using Accessory.Builder.CQRS.Dapper.Queries.Handlers;
using Accessory.Builder.CQRS.Dapper.Queries.Providers;
using Accessory.Builder.CQRS.Dapper.Sql;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.CQRS.Dapper;

public static class Extensions
{
    public static IAccessoryBuilder AddDapperForQueries(this IAccessoryBuilder builder, IDapperInitializer? dapperInitializer = null, string sectionName = "Dapper")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;
            
        dapperInitializer?.Init();
        builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        builder.Services.Decorate(typeof(IQueryHandler<,>), typeof(FilteredQueryHandler<,>));
        builder.Services.Decorate(typeof(IQueryHandler<,>), typeof(PagedQueryHandler<,>));
        builder.Services.Decorate(typeof(IQueryHandler<,>), typeof(SortedQueryHandler<,>));
        builder.Services.AddTransient<IDynamicParametersExtractor, DynamicParametersExtractor>();
            
        return builder;
    }
}